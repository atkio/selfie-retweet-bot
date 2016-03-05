using LinqToTwitter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SelfieBot
{
    class SelfieTweetListWatcher
    {
        static Mutex mutex = new Mutex(false, "japaninfoz.com SelfieTweetListWatcher");

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
                foreach (var kv in db.getUserList())
                {
                    ulong maxid;
                    var result = GetData(kv.Key, kv.Value, out maxid);
                    if (result.Count > 0)
                    {
                        db.updateUserList(kv.Key, maxid);
                        var todownload = SelfieTweetFilter.GetImageURL(result)
                             .SelectMany(dkv => dkv.Value, (s, v) =>
                                 new WaitRecognizer()
                                 {
                                     TID = s.Key.StatusID.ToString(),
                                     UID = s.Key.ScreenName,
                                     PhotoUrl = v
                                 }).ToArray();

                        ImageDownloader.Download(todownload);
                    }

                }

            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }

        static SelfieBotConfig config = SelfieBotConfig.Instance;
        private static ApplicationOnlyAuthorizer auth = null;

        static void prepare()
        {
            try
            {
                auth = new ApplicationOnlyAuthorizer
                {
                    CredentialStore = new InMemoryCredentialStore()
                    {
                        ConsumerKey = config.ConsumerKey,
                        ConsumerSecret = config.ConsumerSecret
                    }
                };
                auth.AuthorizeAsync().Wait();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public static List<Status> GetData(string uid, ulong sinceid,out ulong retmaxid)
        {
            if (auth == null)
                prepare();

            retmaxid = sinceid;

            var twitterCtx = new TwitterContext(auth);

            var rslist = new List<Status>();
            var searchResponse =
               twitterCtx.Status
                    .Where(tweet =>
                        tweet.Type == 0 &&
                        tweet.ScreenName == uid &&
                        tweet.SinceID == sinceid &&
                        tweet.Count == 200 &&
                        tweet.IncludeRetweets == false &&
                        tweet.IncludeEntities == true)                
                .ToList();
            
            rslist.AddRange(searchResponse);


            while (rslist.Count < 500 && rslist.Count > 0)
            {
                if (rslist.Min(st => st.StatusID) < sinceid)
                    break;

                ulong maxid = rslist.Min(st => st.StatusID) - 1;

                Thread.Sleep(10 * 1000);

                searchResponse =
                twitterCtx.Status
                   .Where(tweet =>
                       tweet.Type == 0 &&
                       tweet.ScreenName == uid &&
                       tweet.SinceID == sinceid &&
                       tweet.Count == 200 &&
                       tweet.IncludeRetweets == false &&
                       tweet.IncludeEntities == true &&
                       tweet.MaxID == maxid)
               .OrderBy(tweet => tweet.StatusID)
               .ToList();

                if (searchResponse.Count < 1)
                    break;

                rslist.AddRange(searchResponse.Where(st => st.StatusID > sinceid));
                rslist = rslist.OrderBy(tw => tw.StatusID).ToList();
            }

            rslist = rslist.Where(st => st.StatusID > sinceid).ToList();
            retmaxid = rslist.Max(st => st.StatusID);
            return SelfieTweetFilter.Filter(rslist);


        }
    }
}
