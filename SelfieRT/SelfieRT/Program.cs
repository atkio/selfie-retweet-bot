using SelfieRT.Tweet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelfieRT
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                //检查所有等待识别的图片
                SelfieFacerecognizer.Instance.checkALL();

                var tf = SelfieTweetFunc.Instance;

                //搜索本人timeline
                tf.searchTimeline();

                //根据关键字搜索
                tf.SearchTweets();

                //根据list搜索
                tf.searchList();

                //转推所有识别完的推文
                tf.RTAll();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }


        }

    }
}
