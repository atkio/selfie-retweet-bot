using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SelfieRT
{
    class SelfieFacerecognizer
    {
        private static volatile SelfieFacerecognizer instance;
        private static object syncRoot = new Object();

        public static SelfieFacerecognizer Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new SelfieFacerecognizer();
                    }
                }

                return instance;
            }
        }

        private SelfieFacerecognizer()
        {


        }


        /// <summary>
        /// 查所有需要识别的图片
        /// </summary>
        public void checkALL()
        {
            var db = new SelfieBotDB();
          
            SelfieBotConfig config = SelfieBotConfig.Instance;
            MicrosoftFace face = MicrosoftFace.Instance;
            MicrosoftVision vision = MicrosoftVision.Instance;
            var nrs = db.getAllWaitRecognizer();

            List<WaitRecognizer> isfaces;


            //广告用户，相同文字（20字）多个用户同时出现
            var tweets = nrs.GroupBy(n => n.Tweet)
                          .Where(grp => grp.Select(n => n.TID).Distinct().Count() > 1)
                          .SelectMany(grp => grp.Select(n => n.TID))
                          .Distinct()
                          .ToList();

            DebugLogger.Instance.W("found same text >" + tweets.Count);

            var valueTweets = nrs.Where(n => !tweets.Contains(n.TID)).ToList();

            //本地检查黄色内容
            if (!String.IsNullOrEmpty(config.NsfwScript))
            {
                var nsfwdic = LocalCheckNsfw(config.NsfwScript);
                foreach(var kv in nsfwdic)
                {
                    DebugLogger.Instance.W("NFSW;" + kv.Key + ":" + kv.Value);
                }

                valueTweets = valueTweets
                               .GroupBy(n => n.TID)
                               .Where(t => t.All(tt=> nsfwdic[tt.PhotoPath] == false))
                               .SelectMany(t=>t)
                               .ToList();
            }

            //本地查出有脸图片
            isfaces = valueTweets
                         .GroupBy(n => n.TID)
                         .Where(grp => grp.Any(n => Detect(n.PhotoPath)))
                         .SelectMany(grp => grp)
                         .ToList();

            DebugLogger.Instance.W("found faces >" + isfaces.Count);

            if (config.MicrosoftCognitiveServices.UseFace)
            {
                isfaces = isfaces.GroupBy(wr => wr.TID)
                              .Where(grp => grp.Any(nr => face.MakeRequestUrl(nr.PhotoUrl)))
                              .SelectMany(grp => grp)
                              .ToList();

                DebugLogger.Instance.W("found faces with CognitiveServices  >" + isfaces.Count);
            }

            if (config.MicrosoftCognitiveServices.UseComputerVision)
            {
                var adultida = isfaces.GroupBy(n => n.UID)
                              .Where(grp => grp.Any(nr => vision.AnalyzeUrlAdult(nr.PhotoUrl)))
                              .Select(grp => grp.Key).ToList();

                //成人内容的用户加入黑名单
                db.addBandIDs(adultida);

                DebugLogger.Instance.W("found adult users  >" + adultida.Count);

                isfaces = isfaces.Where(wr => !adultida.Contains(wr.UID))
                                 .ToList();

            }


            foreach (var tid in isfaces.Select(i => i.TID).Distinct())
            {
                DebugLogger.Instance.W("addtoretweet >" + tid);

                db.addToRetweet(tid);
            }

            db.removeAllWaitRecognizer();

            DebugLogger.Instance.W("removeAllWaitRecognizer");

            foreach (FileInfo file in new DirectoryInfo(config.PhotoTempPath).GetFiles())
            {
                file.Delete();
            }

            DebugLogger.Instance.W("Deleted all files");
        }

        protected static bool IsFileLocked(string file)
        {
            FileStream stream = null;

            try
            {
                stream = File.Open(file, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
        }
        #region 本地检测
        const string anifaceFileName = "lbpcascade_animeface.xml";
        const string faceFileName2 = "haarcascade_frontalface_default.xml";
        const string eyeFileName = "haarcascade_eye.xml";
        const string faceFileName = "visionary_FACES_01_LBP_5k_7k_50x50.xml";

        public static Dictionary<string,bool> LocalCheckNsfw(string script)
        {
            return 
            CallScript(script)
                .Where(str => str.StartsWith("NSFW:"))
                .Select(str => str.Substring(5).Split(','))
                .ToDictionary(kv => kv[0], kv => float.Parse(kv[1]) >0.75);

        }

        static List<string> CallScript(string script)
        {
            var rtn = new List<string>();

            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.StartInfo.FileName = script;
         

            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.CreateNoWindow = true;
         
            proc.Start();
            StreamReader q = proc.StandardOutput;
            while (!proc.HasExited)
                rtn.Add(q.ReadLine());


            return rtn;
        }

        public static bool Detect(string file)
        {
            try
            {
                using (CascadeClassifier aniface = new CascadeClassifier(anifaceFileName))
                using (CascadeClassifier face = new CascadeClassifier(faceFileName))
                using (CascadeClassifier face2 = new CascadeClassifier(faceFileName2))
                using (UMat ugray = new UMat())
                using (Image<Bgr, byte> image = new Image<Bgr, byte>(file))
                {
                    CvInvoke.CvtColor(image.Mat, ugray, ColorConversion.Bgr2Gray);
                    CvInvoke.EqualizeHist(ugray, ugray);

                    /*     卡通人物false      */
                    if (aniface.DetectMultiScale(
                        ugray,
                        1.1,
                        10,
                        new Size(20, 20)).Count() > 0)
                        return false;

                    /*    haarcascade_frontalface   判断true */
                    if (face.DetectMultiScale(
                        ugray,
                        1.1,
                        10,
                        new Size(20, 20)).Count() > 0)
                        return true;

                    /*   Visionary_FACES   判断true */
                    if (face2.DetectMultiScale(
                        ugray,
                        1.1,
                        10,
                        new Size(20, 20)).Count() > 0)
                        return true;

                    // 判断后无人脸
                    return false;

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                DebugLogger.Instance.W(e.StackTrace);
                return false;
            }
            finally
            {
                //防止OPENCV内存泄露
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }



        }
        #endregion
        
    }


}
