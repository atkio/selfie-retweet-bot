using LinqToTwitter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SelfieRT.Tweet
{
    public class SelfieTweetFunc
    {

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
            authuser = new SingleUserAuthorizer
            {
                CredentialStore = new SingleUserInMemoryCredentialStore
                {
                    ConsumerKey = config.Twitter.ConsumerKey,
                    ConsumerSecret = config.Twitter.ConsumerSecret,
                    AccessToken = config.Twitter.AccessToken,
                    AccessTokenSecret = config.Twitter.AccessTokenSecret
                }
            };

            authapp = new ApplicationOnlyAuthorizer
            {
                CredentialStore = new InMemoryCredentialStore()
                {
                    ConsumerKey = config.Twitter.ConsumerKey,
                    ConsumerSecret = config.Twitter.ConsumerSecret
                }
            };
            db = new SelfieBotDB();

        }

        private TwitterContext AuthorizeContext(IAuthorizer auth)
        {
            try
            {
                auth.AuthorizeAsync().Wait(5 * 1000);
                return new TwitterContext(auth);
            }
            catch (Exception e)
            {
                throw new Exception("TwitterAPI　failed." + e.Message);
            }
        }

        /// <summary>
        /// 带用户的认证，本人timeline和转推用
        /// </summary>
        private SingleUserAuthorizer authuser;

        /// <summary>
        /// 不带用户的认证，搜文字和list用
        /// </summary>
        private ApplicationOnlyAuthorizer authapp;

        private SelfieBotConfig config;
        private SelfieBotDB db;
        #endregion


        /// <summary>
        /// 根据定义的关键字查找
        /// </summary>
        public void SearchTweets()
        {
            foreach (var kv in db.getSearchKey())
            {
                ulong maxid;
                DebugLogger.Instance.W("SearchTweets:" + kv.Value + " >" + kv.Key);

                var bids = TweetHelper.GetBlockedIDs(authuser);
                var result = SelfieTweetFilter.Filter(TweetHelper.SearchTweet(authapp, kv.Key, kv.Value, out maxid), bids);

                DebugLogger.Instance.W("SearchTweets newid  >" + maxid);
                db.updateSearchKey(kv.Key, maxid);

                if (result.Count > 0)
                {
                    var todownload = SelfieTweetFilter.GetImageURL(result).ToArray();
                    ImageDownloader.Download(todownload);
                }

            }
        }


        /// <summary>
        /// 查找本人timeline
        /// </summary>
        public void searchTimeline()
        {

            ulong HTLMaxid = db.getHTLMaxid();
            ulong newid;
            DebugLogger.Instance.W("searchTimeline HTLMaxid:" + HTLMaxid);
            var result = SelfieTweetFilter.Filter(TweetHelper.GetHomeTL(authuser, HTLMaxid, out newid));
            DebugLogger.Instance.W("searchTimeline newid:" + newid);
            db.updateHTLMaxid(newid);

            if (result.Count > 0)
            {
                var todownload = SelfieTweetFilter.GetImageURL(result).ToArray();
                ImageDownloader.Download(todownload);
            }
        }





        /// <summary>
        /// 获取list
        /// </summary>
        public void searchList()
        {

            foreach (var listd in db.getLTLMaxid())
            {
                ulong newid;
                var result = SelfieTweetFilter.Filter(TweetHelper.GetList(authapp, listd.UID, listd.LIST, ulong.Parse(listd.SINCEID), out newid));
                listd.SINCEID = newid.ToString();
                db.updateLTLMaxid(listd);
                if (result.Count > 0)
                {

                    var todownload = SelfieTweetFilter.GetImageURL(result).ToArray();
                    ImageDownloader.Download(todownload);
                }
            }


        }

        #region 转推方法
        /// <summary>
        /// 发推
        /// </summary>
        /// <param name="st"></param>
        public void post(string st)
        {

            var twitterContext = new TwitterContext(authuser);
            var tweet = twitterContext.TweetAsync(st).Result;
        }

        /// <summary>
        /// 转推
        /// </summary>
        /// <param name="tweetID"></param>
        /// <returns></returns>
        public void reTweet(ulong tweetID)
        {

            var twitterContext = new TwitterContext(authuser);
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

    /// <summary>
    /// 
    /// </summary>
    public class TweetHelper
    {
        const ulong MINTWITTERID = 204251866668871681;


        static TwitterContext AuthTwitterContext(IAuthorizer authapp)
        {
            authapp.AuthorizeAsync().Wait();
            return new TwitterContext(authapp);
        }


        public static List<string> GetBlockedIDs(SingleUserAuthorizer authuser)
        {
            var twitterCtx = AuthTwitterContext(authuser);
            var blockResponse =
                (from block in twitterCtx.Blocks
                 where block.Type == BlockingType.List
                 select block)
                .SingleOrDefaultAsync()
                .Result;

            if (blockResponse != null && blockResponse.Users != null)
            {
                return blockResponse.Users.Select(user => user.ScreenNameResponse).ToList();
            }
            return new List<string>();
        }

        /// <summary>
        /// 搜索推文
        /// https://github.com/JoeMayo/LinqToTwitter/wiki/Searching-Twitter
        /// </summary>
        /// <param name="authapp">认证器</param>
        /// <param name="searchtext">搜索文字</param>
        /// <param name="mintid">最小twitterID</param>
        /// <param name="retid">返回最新twitterID</param>
        /// <param name="maxcount">最大搜素推特数</param>
        /// <returns></returns>
        public static List<Status> SearchTweet(ApplicationOnlyAuthorizer authapp, string searchtext, ulong mintid, out ulong retid, int maxcount = 500)
        {

            if (mintid < MINTWITTERID) mintid = MINTWITTERID;
            retid = mintid;

            
            var twitterCtx = AuthTwitterContext(authapp);
            var rslist = new List<Status>();
            var searchResponse =
              (from search in twitterCtx.Search
               where search.Type == SearchType.Search &&
                     search.Query == searchtext  &&
                     search.SinceID == mintid
               select search)
              .SingleOrDefaultAsync()
              .Result;


            if (searchResponse != null && searchResponse.Statuses != null && searchResponse.Statuses.Count > 0)
            {
                retid = searchResponse.Statuses.Max(tw => tw.StatusID);
                rslist.AddRange(searchResponse.Statuses.Where(st => st.StatusID > mintid));
                DebugLogger.Instance.W("SearchData   >" + rslist.Count);
            }
            else
            {
                return rslist;
            }

            while (rslist.Count < maxcount && rslist.Count > 0)
            {
                if (rslist.Min(st => st.StatusID) < mintid)
                    break;

                ulong maxid = rslist.Min(st => st.StatusID) - 1;

                Thread.Sleep(10 * 1000);

                searchResponse =
                     (from search in twitterCtx.Search
                      where search.Type == SearchType.Search &&
                            search.Query == searchtext &&
                            search.SinceID == mintid &&
                            search.MaxID == maxid
                      select search)
                      .SingleOrDefaultAsync()
                      .Result;

                if (searchResponse != null && searchResponse.Statuses != null && searchResponse.Statuses.Count > 0)
                {
                    rslist.AddRange(searchResponse.Statuses.Where(st => st.StatusID > mintid));
                    DebugLogger.Instance.W("SearchData   >" + rslist.Count);
                }
                else
                {
                    break;
                }
            }

            return rslist;
        }

        /// <summary>
        /// 搜索list
        /// https://github.com/JoeMayo/LinqToTwitter/wiki/Reading-List-Statuses
        /// </summary>
        /// <param name="authapp"></param>
        /// <param name="username"></param>
        /// <param name="listname"></param>
        /// <param name="mintid"></param>
        /// <param name="retid"></param>
        /// <param name="maxStatuses"></param>
        /// <returns></returns>
        public static List<Status> GetList(ApplicationOnlyAuthorizer authapp, string username, string listname, ulong mintid, out ulong retid, int maxStatuses = 500)
        {

            var twitterCtx = AuthTwitterContext(authapp);
            string ownerScreenName = username;
            string slug = listname;
            int lastStatusCount = 0;
            // last tweet processed on previous query
            ulong sinceID = mintid > MINTWITTERID ? mintid : MINTWITTERID;
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

                retid = newStatuses.Max(s => s.StatusID);

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
                    
                    if (newStatuses.Count == 0)
                        break;
                    // first tweet processed on current query
                    maxID = newStatuses.Min(status => status.StatusID) - 1;
                    statusList.AddRange(newStatuses);

                    lastStatusCount = newStatuses.Count;
                }
                while (lastStatusCount != 0 && statusList.Count < maxStatuses);


                return statusList;
            }
            retid = sinceID;
            return statusList;

        }

        /// <summary>
        /// 搜索本人的TIMELINE
        /// https://github.com/JoeMayo/LinqToTwitter/wiki/Querying-the-Home-Timeline
        /// </summary>
        /// <param name="authuser"></param>
        /// <param name="mintid"></param>
        /// <param name="retid"></param>
        /// <param name="maxcount"></param>
        /// <returns></returns>
        public static List<Status> GetHomeTL(SingleUserAuthorizer authuser, ulong mintid, out ulong retid, int maxcount = 500)
        {

            if (mintid < MINTWITTERID) mintid = MINTWITTERID;
            var twitterCtx = AuthTwitterContext(authuser);

            retid = mintid;

            var rslist =
               (from tweet in twitterCtx.Status
                where tweet.Type == StatusType.Home &&
                   tweet.Count == 200 &&
                   tweet.SinceID == mintid
                select tweet)
               .ToList();


            while (rslist.Count < maxcount && rslist.Count > 0)
            {
                if (rslist.Min(st => st.StatusID) <= mintid)
                    break;

                retid = rslist.Max(st => st.StatusID);

                ulong maxid = rslist.Min(st => st.StatusID) - 1;

                Thread.Sleep(10 * 1000);

                var searchResponse =
                     (from tweet in twitterCtx.Status
                      where tweet.Type == StatusType.Home &&
                         tweet.Count == 200 &&
                         tweet.SinceID == mintid &&
                         tweet.MaxID == maxid
                      select tweet)
                       .ToList();


                if (searchResponse != null && searchResponse.Count > 0)
                {
                    rslist.AddRange(searchResponse);
                }
                else
                {
                    break;
                }

            }



            return rslist;

        }
    }

    /// <summary>
    /// 
    /// </summary>
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

        /// <summary>
        /// 过滤推文
        /// </summary>
        /// <param name="src">推文</param>
        /// <returns></returns>
        public static List<Status> Filter(List<Status> src, List<string>  blockedids)
        {
            var iblockedids = new List<string>();
            iblockedids.AddRange(blockedids);
            iblockedids.AddRange(BandIDs);
            return src
             .AsParallel()
             .Where(tw => !BlockTexts.Any(bt => tw.Text.Contains(bt)) && /*推特文字过滤*/
                          !iblockedids.Contains(tw.User.ScreenNameResponse) && /*推特黑名单过滤*/
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
                          Tweet = s.Text.Substring(0, 10),
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
