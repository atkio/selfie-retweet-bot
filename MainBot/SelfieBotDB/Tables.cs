using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        [Column(Name = "PhotoPath", CanBeNull = false, DbType = "varchar")]
        public string PhotoPath { get; set; }

        [Column(Name = "PhotoUrl", CanBeNull = false, DbType = "varchar")]
        public string PhotoUrl { get; set; }
    }

    [Table(Name = "WaitRetweet")]
    public class WaitRetweet
    {
        [Column(Name = "TID", CanBeNull = false, DbType = "varchar")]
        public string TID { get; set; }
    }
}
