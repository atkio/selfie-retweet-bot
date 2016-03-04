using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinqToTwitter;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;


namespace SelfieBot
{
    public class SelfieTweetSearch
    {
        static void Main(string[] args)
        {
        }

        private static ApplicationOnlyAuthorizer auth = null;

        static void prepare()
        {
            try
            {
                auth = new ApplicationOnlyAuthorizer
                {
                    CredentialStore = new InMemoryCredentialStore()
                    {
                        ConsumerKey = "o95VBskri0fnYBAxZ68yLg",
                        ConsumerSecret = "XS2ZpjJhN111P8rf8mwyjbBjePPQXbcqW2WfJg5ckNk"
                    }
                };
                auth.AuthorizeAsync().Wait();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public static List<Status> GetData(string regstr, ulong sinceid)
        {

            if (auth == null)
                prepare();

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

            if (searchResponse != null && searchResponse.Statuses != null)
            {
                rslist.AddRange(searchResponse.Statuses.Where(st => st.StatusID > sinceid));
                rslist = rslist.OrderBy(tw => tw.StatusID).ToList();

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
                            search.Query == regstr &&
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
                    rslist = rslist.OrderBy(tw => tw.StatusID).ToList();
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
