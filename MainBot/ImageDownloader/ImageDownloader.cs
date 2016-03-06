using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SelfieBot
{
    public class ImageDownloader
    {

        static SelfieBotConfig config = SelfieBotConfig.Instance;
        public static void Download(params WaitRecognizer[] defs)
        {
            var db = new SelfieBotDB();
            new List<WaitRecognizer>(defs)
                .AsParallel()
                .WithDegreeOfParallelism(5)
                .ForAll(def => {
                    if (dl(def)) db.addWaitRecognizer(def);
                        });
        }

        private static bool dl(WaitRecognizer def)
        {
            if (def.PhotoUrl.Contains("profile_images") || def.PhotoUrl.Contains("emoji"))
                return false;


            if(def.PhotoUrl.Contains("instagram.com") || def.PhotoUrl.Contains("instagr.am"))
            {
                string str1 = new WebClient().DownloadString(def.PhotoUrl);
                string str2 = "og:image";
                string str3 = ".jpg";
                int num1 = str1.IndexOf(str2) + 11;
                int num2 = str1.IndexOf(str3, num1 + str2.Length) + 4;
                def.PhotoUrl = str1.Substring(num1 + str2.Length, num2 - num1 - str2.Length);
                
                return savefile(def);
            }

            def.PhotoUrl = def.PhotoUrl + ":orig";
            return savefile(def);
        }

        public static bool savefile(WaitRecognizer def)
        {
            try
            {
                WebClient webClient = new WebClient();
                Uri address = new Uri(def.PhotoUrl);
                string localpath = address.LocalPath.EndsWith(":orig") ?
                     address.LocalPath.Substring(0, address.LocalPath.Length -5) :
                     address.LocalPath;
                string fileName = Path.GetFileName(localpath);
                def.PhotoPath = config.RecognizerTempPath + "\\" + fileName;
                webClient.DownloadFile(address.AbsoluteUri, def.PhotoPath);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }

        }
    }
}
