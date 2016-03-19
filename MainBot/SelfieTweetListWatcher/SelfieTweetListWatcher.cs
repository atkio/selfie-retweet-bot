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
                foreach (var def in db.getLTLMaxid())
                {
                    ulong maxid;
                    var result = GetData(def.UID,def.LIST,ulong.Parse(def.SINCEID),out maxid);
                    if (result.Count > 0)
                    {
                        def.SINCEID = maxid.ToString();
                        db.updateLTLMaxid(def);
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

        public static List<Status> GetData(string ownerScreenName, string slug, ulong sinceID, out ulong retmaxid)
        {
            if (auth == null)
                prepare();

            retmaxid = sinceID;

            var twitterCtx = new TwitterContext(auth);

            int maxStatuses = 3200;
            int lastStatusCount = 0;
                   
            ulong maxID;
            int count = 200;
            var statusList = new List<Status>();

            // only count
            var listResponse =
                (from list in twitterCtx.List
                 where list.Type == ListType.Statuses &&
                       list.OwnerScreenName == ownerScreenName &&
                       list.Slug == slug &&
                       list.Count == count
                 select list)
                .SingleOrDefault();

            if (listResponse != null && listResponse.Statuses != null)
            {
                List<Status> newStatuses = listResponse.Statuses;
                // first tweet processed on current query
                maxID = newStatuses.Min(status => status.StatusID) - 1;
                statusList.AddRange(newStatuses);

                do
                {
                    // now add sinceID and maxID
                    listResponse =                     
                        (from list in twitterCtx.List
                         where list.Type == ListType.Statuses &&
                               list.OwnerScreenName == ownerScreenName &&
                               list.Slug == slug &&
                               list.Count == count &&
                               list.SinceID == sinceID &&
                               list.MaxID == maxID
                         select list)
                        .SingleOrDefault();

                    if (listResponse == null)
                        break;

                    newStatuses = listResponse.Statuses;
                    // first tweet processed on current query
                    maxID = newStatuses.Min(status => status.StatusID) - 1;
                    statusList.AddRange(newStatuses);

                    lastStatusCount = newStatuses.Count;
                }
                while (lastStatusCount != 0 && statusList.Count < maxStatuses);
              
            }
            statusList = statusList.Where(st => st.StatusID > sinceID).ToList();

            if(statusList.Count >0)
                retmaxid = statusList.Max(st => st.StatusID);

            return SelfieTweetFilter.Filter(statusList);


        }
    }
}
