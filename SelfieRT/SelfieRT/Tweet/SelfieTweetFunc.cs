using LinqToTwitter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SelfieRT.Tweet
{
    public class SelfieTweetFunc
    {
        const ulong MINTWITTERID = 204251866668871681;
        #region 定义
        private static volatile SelfieTweetFunc instance;
        private static object syncRoot = new Object();

        public static SelfieTweetFunc Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new SelfieTweetFunc();
                    }
                }

                return instance;
            }
        }

        private SelfieTweetFunc()
        {
            config = SelfieBotConfig.Instance;
            auth = new SingleUserAuthorizer
            {
                CredentialStore = new SingleUserInMemoryCredentialStore
                {
                    ConsumerKey = config.Twitter.ConsumerKey,
                    ConsumerSecret = config.Twitter.ConsumerSecret,
                    AccessToken = config.Twitter.AccessToken,
                    AccessTokenSecret = config.Twitter.AccessTokenSecret
                }
            };
            db = new SelfieBotDB();
            try
            {
                auth.AuthorizeAsync().Wait(5*1000);
            }
            catch
            {
                throw new Exception("TwitterAPI　failed.");
            }
        }
        private SingleUserAuthorizer auth;
        private SelfieBotConfig config;
        private SelfieBotDB db;
        #endregion

        #region 根据定义的关键字查找
        public void SearchTweets()
        {
            foreach (var kv in db.getSearchKey())
            {
                ulong maxid;
                var result = SearchData(kv.Key, kv.Value, out maxid);
                db.updateSearchKey(kv.Key, maxid);

                if (result.Count > 0)
                {
                    var todownload = SelfieTweetFilter.GetImageURL(result).ToArray();
                    ImageDownloader.Download(todownload);
                }

            }
        }

        private List<Status> SearchData(string regstr, ulong sinceid, out ulong retmaxid)
        {
            if (sinceid < MINTWITTERID) sinceid = MINTWITTERID;
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
            retmaxid = rslist.Max(tw => tw.StatusID);
            return SelfieTweetFilter.Filter(rslist);
        }

        #endregion
        #region 查找本人timeline

        public void searchTimeline()
        {
            ulong HTLMaxid = db.getHTLMaxid();
            ulong newid;
            var result = GetHomeTL(HTLMaxid, out newid);
            if (result.Count > 0)
            {
                db.updateHTLMaxid(newid);
                var todownload = SelfieTweetFilter.GetImageURL(result).ToArray();
                ImageDownloader.Download(todownload);
            }
        }

        private List<Status> GetHomeTL(ulong sinceid, out ulong retmaxid)
        {

            if(sinceid < MINTWITTERID ) sinceid = MINTWITTERID;
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

            retmaxid = sinceid;
            rslist = rslist.Where(st => st.StatusID > sinceid).ToList();
            if (rslist.Count < 1)
                return rslist;

            sinceid = rslist.Max(st => st.StatusID);


            return SelfieTweetFilter.Filter(rslist);

        }
        #endregion

        #region 获取list

        public void searchList()
        {

            foreach (var listd in db.getLTLMaxid())
            {
                ulong newid;
                var result = GetList(listd.UID,listd.LIST,ulong.Parse(listd.SINCEID), out newid);
                if (result.Count > 0)
                {
                    listd.SINCEID = newid.ToString();
                    db.updateLTLMaxid(listd);
                    var todownload = SelfieTweetFilter.GetImageURL(result).ToArray();
                    ImageDownloader.Download(todownload);
                }
            }

           
        }

        private List<Status> GetList(string username,string listname,ulong sinceid, out ulong retmaxid)
        {
            var twitterCtx = new TwitterContext(auth);
            string ownerScreenName = username;
            string slug = listname;
            int maxStatuses = 300;
            int lastStatusCount = 0;
            // last tweet processed on previous query
            ulong sinceID = sinceid>MINTWITTERID? sinceid: MINTWITTERID;
            ulong maxID;
            int count = 10;
            var statusList = new List<Status>();

            // only count
            var listResponse =
                 (from list in twitterCtx.List
                 where list.Type == ListType.Statuses &&
                       list.OwnerScreenName == ownerScreenName &&
                       list.Slug == slug &&
                       list.Count == count
                 select list)
                .SingleOrDefaultAsync().Result;

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
                        .SingleOrDefaultAsync().Result;

                    if (listResponse == null)
                        break;

                    newStatuses = listResponse.Statuses;
                    // first tweet processed on current query
                    maxID = newStatuses.Min(status => status.StatusID) - 1;
                    statusList.AddRange(newStatuses);

                    lastStatusCount = newStatuses.Count;
                }
                while (lastStatusCount != 0 && statusList.Count < maxStatuses);

                retmaxid = statusList.Max(s => s.StatusID);
                return SelfieTweetFilter.Filter(statusList);
            }
            retmaxid = sinceID;
            return statusList;

        }

        #endregion
            #region 转推方法
            /// <summary>
            /// 发推
            /// </summary>
            /// <param name="st"></param>
        public void post(string st)
        {
            var twitterContext = new TwitterContext(auth);
            var tweet = twitterContext.TweetAsync(st).Result;
        }

        /// <summary>
        /// 转推
        /// </summary>
        /// <param name="tweetID"></param>
        /// <returns></returns>
        public void reTweet(ulong tweetID)
        {
            var twitterContext = new TwitterContext(auth);
            try
            {
                var retweet = twitterContext.RetweetAsync(tweetID).Result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

        }

        public void RTAll()
        {
            foreach (ulong id in db.getWaitRetweet())
            {
                reTweet(id);
                db.removeRetweet(id);
                Thread.Sleep(90 * 1000);
            }
        }
        #endregion
    }

    public class SelfieTweetFilter
    {
        static SelfieBotDB db = new SelfieBotDB();
        static SelfieBotConfig config = SelfieBotConfig.Instance;
        static List<string> BlockTexts = db.getBlockTexts();
        static List<string> BandIDs = db.getBandIDs();
        static List<string> NameBlockTexts = db.getNameBlockTexts();

        /// <summary>
        /// 过滤推文
        /// </summary>
        /// <param name="src">推文</param>
        /// <returns></returns>
        public static List<Status> Filter(List<Status> src)
        {
            return src
             .AsParallel()
             .Where(tw => !BlockTexts.Any(bt => tw.Text.Contains(bt)) && /*推特文字过滤*/
                          !BandIDs.Contains(tw.User.ScreenNameResponse) && /*推特黑名单过滤*/
                          !NameBlockTexts.Any(bt => tw.User.Name.Contains(bt)) && /*推特用户名过滤*/
                          tw.RetweetedStatus.StatusID == 0) /*非转推*/
            .ToList();
        }

        ///// <summary>
        ///// 从推文中获取图片链接
        ///// </summary>
        ///// <param name="src"></param>
        ///// <returns></returns>
        //public static Dictionary<Status, List<string>> GetImageURL(List<Status> src)
        //{
        //    return src.Distinct()
        //           .Select(s => new KeyValuePair<Status, List<string>>(s, urls(s)))
        //           .Where(kv => kv.Value.Count > 0)
        //           .ToDictionary(kv => kv.Key, kv => kv.Value);
        //}

        /// <summary>
        /// 从推文中获取图片链接
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public static List<WaitRecognizer> GetImageURL(List<Status> src)
        {
            return src.Distinct()
                   .SelectMany(s => urls(s), (s, url) =>
                      new WaitRecognizer()
                      {
                          TID = s.StatusID.ToString(),
                          UID = s.User.ScreenNameResponse,
                          Tweet = s.Text.Substring(0,20),
                          PhotoUrl = url,
                          PhotoPath = ""
                      })
                   .ToList();
        }

        /// <summary>
        /// 清除重复文字的推
        /// </summary>
        public static void ClearGarbTweet()
        {
            var grabusers =
            db.getAllWaitRecognizer()
                .GroupBy(r => r.Tweet)
                .Where(grp => grp.Select(r => r.UID).Distinct().Count() > 1)
                .SelectMany(grp => grp)
                .ToList();

            //db.addBandIDs(grabusers.Select(u => u.UID).Distinct().ToList());

            grabusers.ForEach(u => db.removeWaitRecognizer(u));


        }

        /// <summary>
        /// 从推文中获取图片链接
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        static List<string> urls(Status data)
        {
            var ret = new List<string>();
            if (data.Entities.MediaEntities != null)
            {
                ret.AddRange(data.Entities.MediaEntities.Select(media => media.MediaUrl));
            }

            if (data.ExtendedEntities.MediaEntities != null)
            {
                ret.AddRange(data.ExtendedEntities.MediaEntities.Select(media => media.MediaUrl));
            }

            if (data.Entities.UrlEntities != null)
            {
                ret.AddRange(
                 data.Entities.UrlEntities
                 .Where(urlEntity => (urlEntity.ExpandedUrl.Contains("instagram.com") || urlEntity.ExpandedUrl.Contains("instagr.am")))
                 .Select(urlEntity => urlEntity.ExpandedUrl));
            }
            return ret;
        }
    }
}
