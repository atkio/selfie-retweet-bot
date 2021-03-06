﻿using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;

namespace SelfieRT.Tweet
{
    public class ImageDownloader
    {

        static SelfieBotConfig config = SelfieBotConfig.Instance;
        public static void Download(params WaitRecognizer[] defs)
        {
            var db = new SelfieBotDB();
            var stored = db.getAllWaitRecognizer().Select(nr => nr.TID).Distinct();
            foreach (var def in defs.Where(d => !stored.Contains(d.TID)))
            {
                if (dl(def)) db.addWaitRecognizer(def);
            }
        }

        private static bool dl(WaitRecognizer def)
        {
            if (def.PhotoUrl.Contains("profile_images") || def.PhotoUrl.Contains("emoji"))
                return false;


            if (def.PhotoUrl.Contains("instagram.com") || def.PhotoUrl.Contains("instagr.am"))
            {
                try
                {
                    string str1 = new WebClient().DownloadString(def.PhotoUrl);
                    string str2 = "og:image";
                    string str3 = ".jpg";
                    int num1 = str1.IndexOf(str2) + 11;
                    int num2 = str1.IndexOf(str3, num1 + str2.Length) + 4;
                    def.PhotoUrl = str1.Substring(num1 + str2.Length, num2 - num1 - str2.Length);

                    return savefile(def);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return false;
                }
            }

            def.PhotoUrl = def.PhotoUrl + ":orig";
            return savefile(def);
        }

        public static bool savefile(WaitRecognizer def)
        {
            try
            {
                if (!Directory.Exists(config.PhotoTempPath))
                {
                    Directory.CreateDirectory(config.PhotoTempPath);
                }

                Uri address = new Uri(def.PhotoUrl);
                string localpath = address.LocalPath.EndsWith(":orig") ?
                     address.LocalPath.Substring(0, address.LocalPath.Length - 5) :
                     address.LocalPath;
                string fileName = Path.GetFileName(localpath);
                def.PhotoPath = Path.Combine(config.PhotoTempPath, fileName);

                PoolAndDownloadFile(address, def.PhotoPath);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }

        }

        static void PoolAndDownloadFile(Uri uri, string filePath)
        {
            WebClient webClient = new WebClient();
            byte[] downloadedBytes = webClient.DownloadData(uri);
            int count = 0;
            while (downloadedBytes.Length == 0)
            {
                if (count > 4) throw new Exception("can not download:" + uri);
                Thread.Sleep(2000);
                downloadedBytes = webClient.DownloadData(uri);
                count++;
            }
            Stream file = File.Open(filePath, FileMode.Create);
            file.Write(downloadedBytes, 0, downloadedBytes.Length);
            file.Close();
        }
    }
}
