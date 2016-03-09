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
        static Mutex mutex = new Mutex(false, "japaninfoz.com SelfieTweetHTLWatcher");

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
                ulong HTLMaxid = db.getHTLMaxid();
                ulong newid;
                var result = GetHomeTL(HTLMaxid,out newid);
                if (result.Count > 0)
                {
                    db.updateHTLMaxid(newid);
                    var todownload = SelfieTweetFilter.GetImageURL(result)
                            .SelectMany(dkv => dkv.Value, (s, v) =>
                                new WaitRecognizer()
                             {
                                 TID = s.Key.StatusID.ToString(),
                                 UID = s.Key.User.ScreenNameResponse,
                                 PhotoUrl = v,
                                 PhotoPath =""
                             }).ToArray();

                    ImageDownloader.Download(todownload);
                }

            }
            finally
            {
                mutex.ReleaseMutex();
            }

        }

        static SelfieBotConfig config = SelfieBotConfig.Instance;
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

        public static List<Status> GetHomeTL(ulong sinceid,out ulong retmaxid)
        {
            if (auth == null)
                prepare();

            retmaxid = sinceid;

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
                }
                else
                {
                    break;
                }

            }

            rslist = rslist.Where(st => st.StatusID > sinceid).ToList();
            if (rslist.Count < 1)
                return rslist;
            retmaxid = rslist.Max(st => st.StatusID);
            return SelfieTweetFilter.Filter(rslist);

        }

    }
}
