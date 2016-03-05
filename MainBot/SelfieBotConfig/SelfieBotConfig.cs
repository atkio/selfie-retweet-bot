using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelfieBot
{
    
    public class SelfieBotConfig
    {
        public  string AccessToken { get; set; }
        public  string AccessTokenSecret { get; set; }
        public  string ConsumerKey { get; set; }
        public  string ConsumerSecret { get; set; }
        public  string MyTwitterID { get; set; }

        public string DBType { get; set; }
        public string DBConnectString { get; set; }

        public string RecognizerKey { get; set; }
        public string RecognizerTempPath { get; set; }
       
    }
}
