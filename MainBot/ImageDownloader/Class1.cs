using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ImageDownloader
{
    public class ImageDownloader
    {

        public static string GetRealFilename(string url)
        {
            string str1 = new WebClient().DownloadString(url);
            string str2 = "og:image";
            string str3 = ".jpg";
            int num1 = str1.IndexOf(str2) + 11;
            int num2 = str1.IndexOf(str3, num1 + str2.Length) + 4;
            return str1.Substring(num1 + str2.Length, num2 - num1 - str2.Length);
        }

        public static void savefile1(string url, string A_1)
        {
            try
            {
                if (url.Contains("profile_images") || url.Contains("emoji"))
                    return;
                WebClient webClient = new WebClient();
                Uri address = new Uri(url + ":orig");
                string fileName = Path.GetFileName(new Uri(url).LocalPath);
                if (!Directory.Exists(A_1))
                {
                    Directory.CreateDirectory(A_1);
                }
                webClient.DownloadFile(address, A_1 + "\\" + fileName);
            }
            catch (WebException ex)
            {
                if (ex.Status != WebExceptionStatus.ProtocolError)
                {
                    Console.WriteLine(ex.Message);
                }

            }
        }

        public static void savefile2(string url, string A_1)
        {
            try
            {
                if (url.Contains("profile_images") || url.Contains("emoji"))
                    return;
                WebClient webClient = new WebClient();
                Uri address = new Uri(url);
                string fileName = Path.GetFileName(address.LocalPath);
                if (!Directory.Exists(A_1))
                {
                    Directory.CreateDirectory(A_1);
                }
                webClient.DownloadFile(address.AbsoluteUri, A_1 + "\\" + fileName);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw e;
            }

        }
    }
}
