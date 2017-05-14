using Mono.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.IO;
using System.Linq;

namespace SelfieRT
{
    public class SelfieBotDB
    {

        public SelfieBotDB()
        {
            if (!File.Exists(DBFile))
            {
                CreateDB();
            }
        }

        const string DBFile = "SelfieBot.sqlite";
        public static IDbConnection GetSqlConnection()
        {
            return new SqliteConnection("URI=file:" + DBFile + ";DbLinqProvider=sqlite;");
        }


        const string BandIDs = "CREATE TABLE `BandIDs` ( `ID`	varchar(100), PRIMARY KEY(`ID`));";
        const string BlockName = "CREATE TABLE `BlockName` (`NAME`	varchar(100),PRIMARY KEY(`NAME`));";
        const string BlockText = "CREATE TABLE `BlockText` (`TEXT`	varchar(100),PRIMARY KEY(`TEXT`));";
        const string HomeTimeLineMAXID = "CREATE TABLE `HomeTimeLineMAXID` (`SINCEID`	varchar(100),PRIMARY KEY(`SINCEID`));";
        const string SearchKeys = "CREATE TABLE `SearchKeys` (`KEYWORDS`	varchar(100),`SINCEID`	varchar(100) NOT NULL,PRIMARY KEY(`KEYWORDS`));";
        const string WaitRecognizer = "CREATE TABLE `WaitRecognizer` (`UID`	varchar(100) NOT NULL,`TID`	varchar(100) NOT NULL,`Tweet`	varchar(100),`PhotoPath`	varchar(100),`PhotoUrl`	varchar(100),PRIMARY KEY(`PhotoUrl`));";
        const string WaitRetweet = "CREATE TABLE `WaitRetweet` (`TID`	varchar(100),`UID`	varchar(100),`RANK`	INTEGER,PRIMARY KEY(`TID`));";
        const string WatchUsers = "CREATE TABLE `WatchUsers` (`UID`	varchar(100),`SINCEID`	varchar(100) NOT NULL,PRIMARY KEY(`UID`));";
        const string ListTimeLineMAXID = "CREATE TABLE `ListTimeLineMAXID` (`UID`	varchar(100) NOT NULL,`LIST`	varchar(100) NOT NULL,`SINCEID`	varchar(100) NOT NULL);";

        public void CreateDB()
        {
            SqliteConnection.CreateFile(DBFile);
            using (var con = GetSqlConnection())
            {
                try
                {
                    con.Open();
                    IDbCommand command = con.CreateCommand();
                    command.CommandText = BandIDs;
                    command.ExecuteNonQuery();

                    command.CommandText = BlockName;
                    command.ExecuteNonQuery();

                    command.CommandText = BlockText;
                    command.ExecuteNonQuery();

                    command.CommandText = HomeTimeLineMAXID;
                    command.ExecuteNonQuery();

                    command.CommandText = SearchKeys;
                    command.ExecuteNonQuery();

                    command.CommandText = WaitRecognizer;
                    command.ExecuteNonQuery();

                    command.CommandText = WaitRetweet;
                    command.ExecuteNonQuery();

                    command.CommandText = WatchUsers;
                    command.ExecuteNonQuery();

                    command.CommandText = ListTimeLineMAXID;
                    command.ExecuteNonQuery();

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        #region Define
        public List<string> getBlockTexts()
        {
            using (var context = new DataContext(GetSqlConnection()))
            {
                return
                 context.GetTable<BlockText>().
                     Select(bt => bt.TEXT)
                     .ToList();
            }

        }

        public List<string> getNameBlockTexts()
        {
            using (var context = new DataContext(GetSqlConnection()))
            {
                return
                 context.GetTable<BlockName>().
                     Select(bt => bt.NAME)
                     .ToList();
            }
        }



        public Dictionary<string, ulong> getUserList()
        {
            using (var context = new DataContext(GetSqlConnection()))
            {
                return
                 context.GetTable<WatchUsers>()
                 .ToDictionary(bt => bt.UID, bt => ulong.Parse(bt.SINCEID));
            }
        }

        public List<string> getBandIDs()
        {
            using (var context = new DataContext(GetSqlConnection()))
            {

                return context.GetTable<BandIDs>()
                    .Select(bi => bi.ID).ToList();
            }
        }

        public void addBandIDs(List<string> tID)
        {
            using (var context = new DataContext(GetSqlConnection()))
            {
                try
                {
                    var table = context.GetTable<BandIDs>();
                    foreach (var t in tID)
                        table.InsertOnSubmit(new BandIDs() { ID = t });
                    context.SubmitChanges();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        public void setBlockTexts(List<string> values)
        {
            using (var context = new DataContext(GetSqlConnection()))
            {
                context.GetTable<BlockText>()
                   .InsertAllOnSubmit(values.Select(v => new BlockText() { TEXT = v }));
                context.SubmitChanges();
            }

        }

        public void setNameBlockTexts(List<string> values)
        {
            using (var context = new DataContext(GetSqlConnection()))
            {
                context.GetTable<BlockName>()
                     .InsertAllOnSubmit(values.Select(v => new BlockName() { NAME = v }));
                context.SubmitChanges();
            }
        }

        public void setUserList(List<string> values)
        {
            using (var context = new DataContext(GetSqlConnection()))
            {
                context.GetTable<WatchUsers>()
                     .InsertAllOnSubmit(values.Select(v => new WatchUsers() { UID = v, SINCEID = "3200" }));
                context.SubmitChanges();
            }
        }

        public void setUserList(Dictionary<string, string> values)
        {
            using (var context = new DataContext(GetSqlConnection()))
            {
                context.GetTable<WatchUsers>()
                     .InsertAllOnSubmit(values.Select(v => new WatchUsers() { UID = v.Key, SINCEID = v.Value }));
                context.SubmitChanges();
            }
        }

        public void setBandIDs(List<string> values)
        {
            using (var context = new DataContext(GetSqlConnection()))
            {
                context.GetTable<BandIDs>()
                     .InsertAllOnSubmit(values.Select(v => new BandIDs() { ID = v }));
                context.SubmitChanges();
            }
        }

        public Dictionary<string, ulong> getSearchKey()
        {
            using (var context = new DataContext(GetSqlConnection()))
            {

                return context.GetTable<SearchKeys>()
                    .ToDictionary(data => data.KEYWORDS,
                    data => ulong.Parse(data.SINCEID));
            }
        }

        public void updateSearchKey(string key, ulong v)
        {
            using (var context = new DataContext(GetSqlConnection()))
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
            using (var context = new DataContext(GetSqlConnection()))
            {
                return
                 context.GetTable<WaitRetweet>().
                     Select(bt => ulong.Parse(bt.TID))
                     .ToList();
            }
        }

        public void removeRetweet(ulong id)
        {
            using (var context = new DataContext(GetSqlConnection()))
            {

                var table = context.GetTable<WaitRetweet>();
                table.Where(bt => bt.TID == id.ToString())
                     .ToList()
                     .ForEach(d =>
                     table.DeleteOnSubmit(d));

                context.SubmitChanges();
            }
        }



        public void updateUserList(string key, ulong maxid)
        {
            using (var context = new DataContext(GetSqlConnection()))
            {

                var table = context.GetTable<WatchUsers>();
                table.Where(bt => bt.UID == key)
                     .ToList()
                     .ForEach(d => d.SINCEID = maxid.ToString());

                context.SubmitChanges();
            }
        }

        public void addToRetweet(string tID)
        {
            using (var context = new DataContext(GetSqlConnection()))
            {
                try
                {
                    var table = context.GetTable<WaitRetweet>();
                    table.InsertOnSubmit(new WaitRetweet() { TID = tID, RANK = "3", UID = tID });
                    context.SubmitChanges();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }


        public ulong getHTLMaxid()
        {
            using (var context = new DataContext(GetSqlConnection()))
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
            using (var context = new DataContext(GetSqlConnection()))
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
            using (var context = new DataContext(GetSqlConnection()))
            {
                return
                 context.GetTable<WaitRecognizer>()
                     .ToList();
            }
        }

        internal List<WaitRecognizer> getAllWaitRecognizerWithTID(String TID)
        {
            using (var context = new DataContext(GetSqlConnection()))
            {
                return
                 context.GetTable<WaitRecognizer>()
                    .Where(nr => nr.TID == TID)
                     .ToList();
            }
        }

        public void addWaitRecognizer(WaitRecognizer ul)
        {
            try
            {
                using (var context = new DataContext(GetSqlConnection()))
                {
                    var table = context.GetTable<WaitRecognizer>();
                    table.InsertOnSubmit(ul);
                    context.SubmitChanges();
                }
            }
            catch (Exception)
            {
                // Console.WriteLine(e.Message);
            }
        }

        public void removeAllWaitRecognizer()
        {
            using (var context = new DataContext(GetSqlConnection()))
            {
                context.ExecuteCommand("DELETE FROM WaitRecognizer");
                context.SubmitChanges();
            }
        }

        public void removeWaitRecognizer(WaitRecognizer nr)
        {
            using (var context = new DataContext(GetSqlConnection()))
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

        internal void removeWaitRecognizerWithTID(WaitRecognizer nr)
        {
            using (var context = new DataContext(GetSqlConnection()))
            {
                var table = context.GetTable<WaitRecognizer>();
                table.Where(data =>
                data.TID == nr.TID)
                .ToList()
                .ForEach(data => table.DeleteOnSubmit(data));
                context.SubmitChanges();
            }
        }


        public List<ListTimeLineMAXID> getLTLMaxid()
        {
            using (var context = new DataContext(GetSqlConnection()))
            {
                return
                 context.GetTable<ListTimeLineMAXID>()
                     .ToList();
            }
        }

        public void updateLTLMaxid(ListTimeLineMAXID data)
        {
            using (var context = new DataContext(GetSqlConnection()))
            {
                var table =
                 context.GetTable<ListTimeLineMAXID>();

                var datas = table.Where(d => d.UID == data.UID && d.LIST == data.LIST)
                   .ToList();

                if (datas.Count > 0)
                {
                    datas.First().SINCEID = data.SINCEID;
                }
                else
                {
                    table.InsertOnSubmit(data);
                }
                context.SubmitChanges();

            }
        }
    }


    #region Table定义
    [Table(Name = "WaitRecognizer")]
    public class WaitRecognizer
    {
        [Column(Name = "UID", CanBeNull = false, DbType = "varchar")]
        public string UID { get; set; }

        [Column(Name = "TID", CanBeNull = false, DbType = "varchar")]
        public string TID { get; set; }

        [Column(Name = "Tweet", CanBeNull = true, DbType = "varchar")]
        public string Tweet { get; set; }

        [Column(Name = "PhotoPath", CanBeNull = true, DbType = "varchar")]
        public string PhotoPath { get; set; }

        [Column(Name = "PhotoUrl", IsPrimaryKey = true, DbType = "varchar")]
        public string PhotoUrl { get; set; }

        [Column(Name = "Adult", CanBeNull = true, DbType = "INT")]
        public string Adult { get; set; }

        [Column(Name = "Gender", CanBeNull = true, DbType = "INT")]
        public string Gender { get; set; }

        [Column(Name = "Age", CanBeNull = true, DbType = "INT")]
        public string Age { get; set; }
    }

    [Table(Name = "WaitRetweet")]
    public class WaitRetweet
    {
        [Column(Name = "TID", IsPrimaryKey = true, DbType = "varchar")]
        public string TID { get; set; }

        [Column(Name = "UID", CanBeNull = false, DbType = "varchar")]
        public string UID { get; set; }

        [Column(Name = "RANK", CanBeNull = false, DbType = "INT")]
        public string RANK { get; set; }
    }

    [Table(Name = "HomeTimeLineMAXID")]
    public class HomeTimeLineMAXID
    {
        [Column(Name = "SINCEID", IsPrimaryKey = true, DbType = "varchar")]
        public string SINCEID { get; set; }
    }

    [Table(Name = "ListTimeLineMAXID")]
    public class ListTimeLineMAXID
    {
        [Column(Name = "UID", IsPrimaryKey = true, DbType = "varchar")]
        public string UID { get; set; }

        [Column(Name = "LIST", IsPrimaryKey = true, DbType = "varchar")]
        public string LIST { get; set; }

        [Column(Name = "SINCEID", CanBeNull = false, DbType = "varchar")]
        public string SINCEID { get; set; }
    }


    [Table(Name = "SearchKeys")]
    public class SearchKeys
    {
        [Column(Name = "KEYWORDS", IsPrimaryKey = true, DbType = "varchar")]
        public string KEYWORDS { get; set; }

        [Column(Name = "SINCEID", CanBeNull = false, DbType = "varchar")]
        public string SINCEID { get; set; }
    }

    [Table(Name = "WatchUsers")]
    public class WatchUsers
    {
        [Column(Name = "UID", IsPrimaryKey = true, DbType = "varchar")]
        public string UID { get; set; }

        [Column(Name = "SINCEID", CanBeNull = false, DbType = "varchar")]
        public string SINCEID { get; set; }
    }

    [Table(Name = "BandIDs")]
    public class BandIDs
    {
        [Column(Name = "ID", IsPrimaryKey = true, DbType = "varchar")]
        public string ID { get; set; }
    }

    [Table(Name = "BlockText")]
    public class BlockText
    {
        [Column(Name = "TEXT", IsPrimaryKey = true, DbType = "varchar")]
        public string TEXT { get; set; }
    }

    [Table(Name = "BlockName")]
    public class BlockName
    {
        [Column(Name = "NAME", IsPrimaryKey = true, DbType = "varchar")]
        public string NAME { get; set; }
    }
    #endregion
}
