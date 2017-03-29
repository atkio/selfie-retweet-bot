using LinqToTwitter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SelfieRt
{
    public class SelfieTweetSearch
    {
   
        public static void Run()
        {
            

            var db = new SelfieBotDB();
            
                foreach (var kv in db.getSearchKey())
                {
                    ulong maxid;
                    var result = GetData(kv.Key, kv.Value, out maxid);
                    if (result.Count > 0)
                    {
                        db.updateSearchKey(kv.Key, maxid);
                        var todownload = SelfieTweetFilter.GetImageURL(result)
                             .SelectMany(dkv => dkv.Value, (s, v) =>
                                 new WaitRecognizer()
                                 {
                                     TID = s.Key.StatusID.ToString(),
                                     UID = s.Key.User.ScreenNameResponse,
                                     PhotoUrl = v,
                                     PhotoPath = ""
                                 }).ToArray();

                        ImageDownloader.Download(todownload);
                    }

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

        public static List<Status> GetData(string regstr, ulong sinceid, out ulong retmaxid)
        {

            if (auth == null)
                prepare();

            retmaxid = sinceid;

            var twitterCtx = new TwitterContext(auth);

            var rslist = new List<Status>();
            var searchResponse =
              (from search in twitterCtx.Search
               where search.Type == SearchType.Search &&
                     search.Query == regstr + "  -filter:retweets" &&
                     search.Count == 100 &&
                     search.SinceID == sinceid
               select search)
              .SingleOrDefault();

            if (searchResponse != null && searchResponse.Statuses != null && searchResponse.Statuses.Count > 0)
            {
                rslist.AddRange(searchResponse.Statuses.Where(st => st.StatusID > sinceid));
                retmaxid = rslist.Max(tw => tw.StatusID);
            }
            else
            {
                return rslist;
            }

            while (rslist.Count < 500 && rslist.Count > 0)
            {
                if (rslist.Min(st => st.StatusID) < sinceid)
                    break;

                ulong maxid = rslist.Min(st => st.StatusID) - 1;

                Thread.Sleep(10 * 1000);

                searchResponse =
                     (from search in twitterCtx.Search
                      where search.Type == SearchType.Search &&
                            search.Query == regstr + "  -filter:retweets" &&
                            search.Count == 100 &&
                            search.SinceID == sinceid &&
                            search.MaxID == maxid
                      select search)
                     .SingleOrDefault();

                if (searchResponse != null && searchResponse.Statuses != null)
                {
                    if (searchResponse.Statuses.Count < 1)
                        break;

                    rslist.AddRange(searchResponse.Statuses.Where(st => st.StatusID > sinceid));
                }
                else
                {
                    break;
                }
            }
            return SelfieTweetFilter.Filter(rslist);
        }

    }
}
