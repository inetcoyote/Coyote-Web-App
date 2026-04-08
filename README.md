# Coyote Web App

Простое Android-приложение, разработанное на C# с использованием **Xamarin.Android** в **Visual Studio 2019**.  
Приложение демонстрирует базовую структуру мобильного приложения для платформы Android с использованием .NET.

---

## 📱 Описание

Приложение содержит одну активность (`MainActivity`), отображающую экран с кнопками.
При нажатии на кнопки происходит переход на другие страницы приложения.

---

## 🛠 Технологии

- **Язык**: C#
- **Платформа**: Xamarin.Android (.NET Framework)
- **IDE**: Visual Studio 2019
- **Целевая платформа**: Android 5.0 (API 21) и выше

---

## 📁 Структура проекта

Coyote Web App/

│ ├── CoyoteWebApp/<br>
│ ├── Assets/ <br>
│ │ └── offline.html # Офлайн страница приложения <br>
│ ├── Properties/ <br>
│ │ ├── AndroidManifest.xml # UI-макет главного экрана <br>
│ │ └── AssemblyInfo.cs # Информация о сборке <br>
│ ├── Resources/ <br>
│ │ ├── mipmap/ic_launcher.png # Иконка приложения <br>
│ │ ├── values/strings.xml # Настройки подключения к FTP <br>
│ │ └── drawable/icon.png # Иконка приложения <br>
│ │ │ ├── MainActivity.cs # Основная логика приложения <br>
│ │ │ ├── FtpHelper.cs # Логика взаимодействия с FTP-сервером <br>
│ └── CoyoteWebApp.csproj # Файл проекта <br>
│ └── CoyoteWebApp.sln # Решение Visual Studio

## 🧩 Дистрибутив проекта
Файл CoyoteWebApp.apk, расположен в директории https://github.com/inetcoyote/Coyote-Web-App/releases
