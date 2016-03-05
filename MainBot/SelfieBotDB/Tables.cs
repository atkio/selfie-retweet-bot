using System.Data.Linq.Mapping;

namespace SelfieBot
{
    [Table(Name = "WaitRecognizer")]
    public class WaitRecognizer
    {
        [Column(Name = "UID", CanBeNull = false, DbType = "varchar")]
        public string UID { get; set; }

        [Column(Name = "TID", CanBeNull = false, DbType = "varchar")]
        public string TID { get; set; }

        [Column(Name = "PhotoPath", CanBeNull = true, DbType = "varchar")]
        public string PhotoPath { get; set; }

        [Column(Name = "PhotoUrl", CanBeNull = true, DbType = "varchar")]
        public string PhotoUrl { get; set; }
    }

    [Table(Name = "WaitRetweet")]
    public class WaitRetweet
    {
        [Column(Name = "TID", IsPrimaryKey = true, DbType = "varchar")]
        public string TID { get; set; }
    }

    [Table(Name = "HomeTimeLineMAXID")]
    public class HomeTimeLineMAXID
    {
        [Column(Name = "SINCEID", IsPrimaryKey = true, DbType = "varchar")]
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

}
