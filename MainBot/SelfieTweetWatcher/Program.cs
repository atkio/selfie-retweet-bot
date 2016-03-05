using LinqToTwitter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SelfieBot
{
    class SelfieTweetHTLWatcher
    {
        static Mutex mutex = new Mutex(false, "japaninfoz.com SelfieTweetWatcher");

        static void Main(string[] args)
        {
            if (!mutex.WaitOne(TimeSpan.FromSeconds(5), false))
            {
                Console.WriteLine("Another instance of the app is running. Bye!");
                return;
            }

            var db = new SelfieBotDB();
            try
            {
                db.getAllWaitRecognizer()
                    .ForEach(nr =>
                    {
                        if (Detect(nr.PhotoPath, nr.PhotoUrl)) db.addToRetweet();
                        //
                        db.removeWaitRecognizer(nr);
                        File.Delete(nr.PhotoPath);
                    });

            }
            finally
            {
                mutex.ReleaseMutex();
            }

        }

        static SelfieBotConfig config = new SelfieBotConfig();
        private static SingleUserAuthorizer auth = null;

        static void prepare()
        {
            try
            {
                auth = new SingleUserAuthorizer
                {
                    CredentialStore = new SingleUserInMemoryCredentialStore
                    {
                        ConsumerKey = config.ConsumerKey,
                        ConsumerSecret = config.ConsumerSecret,
                        AccessToken = config.AccessToken,
                        AccessTokenSecret = config.AccessTokenSecret
                    }
                };
                auth.AuthorizeAsync().Wait();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public static List<Status> GetHomeTL(ulong sinceid)
        {
            if (auth == null)
                prepare();


            var twitterCtx = new TwitterContext(auth);

            var rslist =
               (from tweet in twitterCtx.Status
                where tweet.Type == StatusType.Home &&
                   tweet.Count == 200 &&
                   tweet.SinceID == sinceid
                select tweet)
               .ToList();
               

            while (rslist.Count < 500 && rslist.Count > 0)
            {
                if (rslist.Min(st => st.StatusID) < sinceid)
                    break;

                ulong maxid = rslist.Min(st => st.StatusID) - 1;

                Thread.Sleep(10 * 1000);

                var searchResponse =
                     (from tweet in twitterCtx.Status
                      where tweet.Type == StatusType.Home &&
                         tweet.Count == 200 &&
                         tweet.SinceID == sinceid &&
                         tweet.MaxID == maxid
                      select tweet)
                       .ToList();


                if (searchResponse != null)
                {
                    if (searchResponse.Count < 1)
                        break;

                    rslist.AddRange(searchResponse);
                    rslist = rslist.OrderBy(tw => tw.StatusID).ToList();
                }
                else
                {
                    break;
                }

            }

            rslist = rslist.Where(st => st.StatusID > sinceid).ToList();
            return SelfieTweetFilter.Filter(rslist);

        }

    }
}
