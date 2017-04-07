using SelfieRT.Tweet;
using System;
using System.IO;

namespace SelfieRT
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                //检查所有等待识别的图片
                DebugLogger.Instance.W("SelfieFacerecognizer.checkALL()");

                SelfieFacerecognizer.Instance.checkALL();

                var tf = SelfieTweetFunc.Instance;


                DebugLogger.Instance.W("tf.searchTimeline()");

                //搜索本人timeline
                tf.searchTimeline();

                DebugLogger.Instance.W("tf.SearchTweets()");

                //根据关键字搜索
                tf.SearchTweets();

                DebugLogger.Instance.W("tf.searchList()");

                //根据list搜索
                tf.searchList();

                DebugLogger.Instance.W("tf.RTAll()");

                //转推所有识别完的推文
                tf.RTAll();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }


        }

    }

    class DebugLogger
    {
        private static volatile DebugLogger instance;
        private static object syncRoot = new Object();

        public static DebugLogger Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                        {
                            if(File.Exists("DEBUG"))
                                instance = new DebugLogger(true);
                            else
                                instance = new DebugLogger();
                        }
                    }
                }

                return instance;
            }
        }

        private DebugLogger()
        {

        }

        private DebugLogger(bool debugmode)
        {
          
            W = outputreal;

        }

        public Action<string> W = outputfake;

        static void outputfake(string s)
        {

        }

        static void outputreal(string s)
        {
            Console.WriteLine(DateTime.Now.ToString("hh:mm:ss.fff") + ":" + s);
        }
    }
}
