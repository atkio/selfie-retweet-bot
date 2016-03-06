using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Linq;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelfieBot
{
    public class SelfieBotDB
    {

        public SelfieBotDB()
        {
            con = BotSqliteConnect.GetSqlConnection();
        }

        IDbConnection con = null;

        #region Define
        public List<string> getBlockTexts()
        {
            using (var context = new DataContext(con))
            {
                return
                 context.GetTable<BlockText>().
                     Select(bt => bt.TEXT)
                     .ToList();
            }

        }

        public List<string> getNameBlockTexts()
        {
            using (var context = new DataContext(con))
            {
                return
                 context.GetTable<BlockName>().
                     Select(bt => bt.NAME)
                     .ToList();
            }
        }

        public Dictionary<string, ulong> getUserList()
        {
            using (var context = new DataContext(con))
            {
                return
                 context.GetTable<WatchUsers>()
                 .ToDictionary(bt => bt.UID, bt => ulong.Parse(bt.SINCEID));
            }
        }

        public List<string> getBandIDs()
        {
            using (var context = new DataContext(con))
            {

                return context.GetTable<BandIDs>()
                    .Select(bi => bi.ID).ToList();
            }
        }

        public void setBlockTexts(List<string> values)
        {
            using (var context = new DataContext(con))
            {
                context.GetTable<BlockText>()
                   .InsertAllOnSubmit(values.Select(v => new BlockText() { TEXT = v }));
                context.SubmitChanges();
            }

        }

        public void setNameBlockTexts(List<string>  values)
        {
            using (var context = new DataContext(con))
            {                
                 context.GetTable<BlockName>()
                      .InsertAllOnSubmit(values.Select(v => new BlockName() { NAME = v }));
                context.SubmitChanges();
            }
        }

        public void setUserList(List<string> values)
        {
            using (var context = new DataContext(con))
            {                
                 context.GetTable<WatchUsers>()
                      .InsertAllOnSubmit(values.Select(v => new WatchUsers() { UID =v,SINCEID="3200" }));
                 context.SubmitChanges();
            }
        }

        public void setUserList(Dictionary<string, string> values)
        {
            using (var context = new DataContext(con))
            {
                context.GetTable<WatchUsers>()
                     .InsertAllOnSubmit(values.Select(v => new WatchUsers() { UID = v.Key, SINCEID = v.Value }));
                context.SubmitChanges();
            }
        }

        public void setBandIDs(List<string> values)
        {
            using (var context = new DataContext(con))
            {
                context.GetTable<BandIDs>()
                     .InsertAllOnSubmit(values.Select(v => new BandIDs() { ID = v }));
                context.SubmitChanges();
            }
        }

        public Dictionary<string, ulong> getSearchKey()
        {
            using (var context = new DataContext(con))
            {

                return context.GetTable<SearchKeys>()
                    .ToDictionary(data => data.KEYWORDS,
                    data => ulong.Parse(data.SINCEID));
            }
        }

        public void updateSearchKey(string key, ulong v)
        {
            using (var context = new DataContext(con))
            {
                var table = context.GetTable<SearchKeys>();
                var datas = table.Where(d => d.KEYWORDS == key)
                    .ToList();

                if (datas.Count > 0)
                {
                    datas.First().SINCEID = v.ToString();
                }
                else
                {
                    table.InsertOnSubmit(new SearchKeys()
                    {
                        KEYWORDS = key,
                        SINCEID = v.ToString()
                    });
                }
                context.SubmitChanges();
            }
        }

        #endregion

        

       
        public List<ulong> getWaitRetweet()
        {
            using (var context = new DataContext(con))
            {
                return
                 context.GetTable<WaitRetweet>().
                     Select(bt => ulong.Parse (bt.TID))
                     .ToList();
            }
        }

        public void removeRetweet(ulong id)
        {
            using (var context = new DataContext(con))
            {

                var table = context.GetTable<WaitRetweet>();
                table.Where(bt => ulong.Parse(bt.TID) == id)
                     .ToList()
                     .ForEach(d =>
                     table.DeleteOnSubmit(d));

                context.SubmitChanges();
            }
        }

       

        public void updateUserList(string key, ulong maxid)
        {
            using (var context = new DataContext(con))
            {

                var table = context.GetTable<WatchUsers>();
                table.Where(bt => bt.UID == key)
                     .ToList()
                     .ForEach(d => d.SINCEID =maxid.ToString());

                context.SubmitChanges();
            }
        }

        public void addToRetweet(string tID)
        {
            using (var context = new DataContext(con))
            {
                try
                {
                    var table = context.GetTable<WaitRetweet>();
                    table.InsertOnSubmit(new WaitRetweet() { TID = tID });
                    context.SubmitChanges();
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }


        public ulong getHTLMaxid()
        {
            using (var context = new DataContext(con))
            {
                var datas =
                 context.GetTable<HomeTimeLineMAXID>()
                 .Select(d => d.SINCEID)
                 .ToList();

                if (datas.Count < 1)
                    return 3200;
                else
                    return ulong.Parse(datas.First());
            }
        }

        public void updateHTLMaxid(ulong newid)
        {
            using (var context = new DataContext(con))
            {

                var table = context.GetTable<HomeTimeLineMAXID>();
                table.DeleteAllOnSubmit(table);
                context.SubmitChanges();
                table.InsertOnSubmit(new HomeTimeLineMAXID() { SINCEID = newid.ToString() });
                context.SubmitChanges();
            }
        }


        public List<WaitRecognizer> getAllWaitRecognizer()
        {
            using (var context = new DataContext(con))
            {
                return
                 context.GetTable<WaitRecognizer>()
                     .ToList();
            }
        }


        public void addWaitRecognizer(WaitRecognizer ul)
        {
            try
            {
                using (var context = new DataContext(con))
                {
                    var table = context.GetTable<WaitRecognizer>();
                    table.InsertOnSubmit(ul);
                    context.SubmitChanges();
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public void removeWaitRecognizer(WaitRecognizer nr)
        {
            using (var context = new DataContext(con))
            {
                var table = context.GetTable<WaitRecognizer>();
                table.Where(data =>
                data.TID == nr.TID &&
                data.PhotoUrl == nr.PhotoUrl)
                .ToList()
                .ForEach(data => table.DeleteOnSubmit(data));
                context.SubmitChanges();
            }
        }
    }
}
