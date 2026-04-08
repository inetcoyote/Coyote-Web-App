using System;
using System.IO;
using System.Net;
using System.Text;

public class FtpHelper
{
    public string _host;
    public string _username;
    public string _password;

    public FtpHelper(string host, string username, string password)
    {
        _host = host;
        _username = username;
        _password = password;
    }

    public string DownloadFile(string filePath)
    {
        try
        {
            var request = (FtpWebRequest)WebRequest.Create($"ftp://{_host}/{filePath}");
            request.Method = WebRequestMethods.Ftp.DownloadFile;
            request.Credentials = new NetworkCredential(_username, _password);

            using (var response = (FtpWebResponse)request.GetResponse())
            using (var stream = response.GetResponseStream())
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Ошибка загрузки с FTP: {ex.Message}");
        }
    }
}
