using LinqToTwitter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelfieRt
{
    public class SelfieTweetFilter
    {
        static SelfieBotDB db = new SelfieBotDB();
        static SelfieBotConfig config = SelfieBotConfig.Instance;
        static List<string> BlockTexts = db.getBlockTexts();
        static List<string> BandIDs = db.getBandIDs();
        static List<string> NameBlockTexts = db.getNameBlockTexts();
        public static List<Status> Filter(List<Status> src)
        {
            return src
             .AsParallel()
             .Where(tw => tw.User.ScreenNameResponse != config.MyTwitterID &&
                          !BlockTexts.Any(bt => tw.Text.Contains(bt)) &&
                          !BandIDs.Contains(tw.User.ScreenNameResponse) &&
                          !NameBlockTexts.Any(bt => tw.User.Name.Contains(bt)) &&
                          tw.RetweetedStatus.StatusID == 0)
            .ToList();
        }

        public static Dictionary<Status, List<string>> GetImageURL(List<Status> src)
        {
            return src.Distinct()
                   .Select(s => new KeyValuePair<Status, List<string>>(s, urls(s)))
                   .Where(kv => kv.Value.Count > 0)
                   .ToDictionary(kv => kv.Key, kv => kv.Value);
        }


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
