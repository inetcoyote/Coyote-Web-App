using Android.App;
using Android.OS;
using Android.Webkit;
using Android.Widget;
using System;
using System.IO;
using System.Threading;
using System.Net;

namespace CoyoteWebApp
{
    [Activity(Label = "CoyoteWebApp", Icon = "@mipmap/ic_launcher",  MainLauncher = true)]
    public class MainActivity : Activity
    {
        private WebView _webView;
        private FtpHelper _ftpHelper;
        private string _cacheDir;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            // Инициализация директории кэша
            _cacheDir = Path.Combine(GetExternalFilesDir(null).AbsolutePath, "webcache");
            if (!Directory.Exists(_cacheDir))
                Directory.CreateDirectory(_cacheDir);

            // Инициализация WebView
            _webView = FindViewById<WebView>(Resource.Id.webView);

            // Настройка WebView
            var settings = _webView.Settings;
            settings.JavaScriptEnabled = true;
            settings.SetSupportZoom(false);
            settings.BuiltInZoomControls = false;
            settings.DisplayZoomControls = false;
            settings.DomStorageEnabled = true;

            // Включение кэширования WebView
            settings.CacheMode = CacheModes.CacheElseNetwork;
            settings.SetAppCacheEnabled(true);
            settings.SetAppCachePath(_cacheDir);
            settings.DatabaseEnabled = true;

            string host = Resources.GetString(Resource.String.ftp_host);
            string username = Resources.GetString(Resource.String.ftp_username);
            string password = Resources.GetString(Resource.String.ftp_password);
            int port = int.Parse(Resources.GetString(Resource.String.ftp_port));

            _ftpHelper = new FtpHelper(host, username, password);

            // Установка кастомного клиента для обработки навигации
            _webView.SetWebViewClient(new CustomWebViewClient(this, _ftpHelper, _cacheDir));

            // Загрузка начальной страницы
            LoadPage("main.html");
        }

        /// <summary>
        /// Проверка доступности интернета
        /// </summary>
        private bool IsInternetAvailable()
        {
            try
            {
                using (var client = new WebClient())
                using (client.OpenRead("http://jourevg.narod.ru"))
                    return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Загрузка страницы: сначала пытается загрузить с FTP (при наличии интернета),
        /// если нет интернета — загружает из локального кэша,
        /// в крайнем случае — показывает локальную страницу из ресурсов
        /// </summary>
        public void LoadPage(string filePath)
        {
            // Нормализуем путь: заменяем обратные слеши, убираем дублирующие слеши
            filePath = filePath.Replace('\\', '/').Replace("//", "/");

            if (IsInternetAvailable())
            {
                // При наличии интернета всегда пытаемся загрузить с FTP
                LoadPageFromFtp(filePath);
            }
            else
            {
                // Если нет интернета — ищем в кэше
                string cachePath = Path.Combine(_cacheDir, filePath);
                if (File.Exists(cachePath))
                {
                    var cachedContent = File.ReadAllText(cachePath);
                    _webView.LoadDataWithBaseURL(
                $"ftp://{_ftpHelper._host}/{Path.GetDirectoryName(filePath)}/",
                cachedContent,
                "text/html",
                "UTF-8",
                null);
                }
                else
                {
                    // Если нет ни интернета, ни кэша — показываем локальную страницу
                    ShowLocalOfflinePage();
                }
            }
        }

        /// <summary>
        /// Загрузка HTML‑страницы с FTP‑сервера, кэширование и отображение в WebView
        /// При ошибке загрузки показывает локальный кэш или офлайн‑страницу
        /// </summary>
        private void LoadPageFromFtp(string filePath)
        {
            new Thread(() =>
            {
                try
                {
                    // Загрузка содержимого файла с FTP
                    var htmlContent = _ftpHelper.DownloadFile(filePath);

                    // Кэширование файла локально с сохранением структуры папок
                    string cachePath = Path.Combine(_cacheDir, filePath);
                    string cacheDirForFile = Path.GetDirectoryName(cachePath);
                    if (!Directory.Exists(cacheDirForFile))
                    {
                        Directory.CreateDirectory(cacheDirForFile);
                    }

                    File.WriteAllText(cachePath, htmlContent);

                    // Обновление UI в основном потоке
                    RunOnUiThread(() =>
                    {
                        _webView.LoadDataWithBaseURL(
                            $"ftp://{_ftpHelper._host}/{Path.GetDirectoryName(filePath)}/",
                            htmlContent,
                            "text/html",
                            "UTF-8",
                            null);
                    });
                }
                catch (Exception)
                {
                    // При ошибке загрузки с FTP проверяем кэш
                    RunOnUiThread(() =>
                    {
                        string cachePath = Path.Combine(_cacheDir, filePath);
                        if (File.Exists(cachePath))
                        {
                            var cachedContent = File.ReadAllText(cachePath);
                            _webView.LoadDataWithBaseURL(
                                $"ftp://{_ftpHelper._host}/{Path.GetDirectoryName(filePath)}/",
                                cachedContent,
                                "text/html",
                                "UTF-8",
                                null);
                        }
                        else
                        {
                            // Если нет кэша — показываем офлайн‑страницу из ресурсов
                            ShowLocalOfflinePage();
                        }
                    });
                }
            }).Start();
        }

        /// <summary>
        /// Отображение локальной HTML‑страницы из ресурсов приложения
        /// </summary>
        private void ShowLocalOfflinePage()
        {
            try
            {
                // Читаем HTML из ресурсов
                using (var stream = Assets.Open("offline.html"))
                using (var reader = new StreamReader(stream))
                {
                    string htmlContent = reader.ReadToEnd();
                    _webView.LoadDataWithBaseURL(
                    "file:///android_asset/",
                    htmlContent,
                    "text/html",
                    "UTF-8",
                    null);
                }
            }
            catch (Exception ex)
            {
                // Если ресурс не найден, показываем простой HTML
                _webView.LoadData(
                $"<html><body><h3>Нет подключения к интернету</h3><p>Приложение работает в офлайн‑режиме.</p><p>Ошибка: {ex.Message}</p></body></html>",
                "text/html",
                "UTF-8");
            }
        }

        /// <summary>
        /// Обработка кнопки «Назад»: возврат на предыдущую страницу в WebView,
        /// если история пуста — выход из приложения
        /// </summary>
        public override void OnBackPressed()
        {
            if (_webView.CanGoBack())
            {
                _webView.GoBack();
            }
            else
            {
                base.OnBackPressed();
            }
        }

        /// <summary>
        /// Кастомный WebViewClient для перехвата FTP‑ссылок и работы с кэшем
        /// </summary>
        public class CustomWebViewClient : WebViewClient
        {
            private MainActivity _activity;
            private FtpHelper _ftpHelper;
            private string _cacheDir;

            public CustomWebViewClient(MainActivity activity, FtpHelper ftpHelper, string cacheDir)
            {
                _activity = activity;
                _ftpHelper = ftpHelper;
                _cacheDir = cacheDir;
            }

            /// <summary>
            /// Перехват загрузки URL для обработки FTP‑ссылок
            /// </summary>
            public override bool ShouldOverrideUrlLoading(WebView view, string url)
            {
                if (url.StartsWith($"ftp://{_ftpHelper._host}/"))
                {
                    // Извлекаем путь после хоста и нормализуем его
                    var path = url.Substring($"ftp://{_ftpHelper._host}".Length);
                    path = path.TrimStart('/');

                    System.Diagnostics.Debug.WriteLine($"Intercepted URL: {url}");
                    System.Diagnostics.Debug.WriteLine($"Extracted path: {path}");

                    _activity.LoadPage(path);
                    return true;
                }

                return false;
            }
        }
    }
}