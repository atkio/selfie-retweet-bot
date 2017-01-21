using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelfieBot
{

    public class SelfieBotConfig
    {
        public string AccessToken { get; set; }
        public string AccessTokenSecret { get; set; }
        public string ConsumerKey { get; set; }
        public string ConsumerSecret { get; set; }
        public string MyTwitterID { get; set; }

        public string DBType { get; set; }
        public string DBConnectString { get; set; }

        public string RecognizerKey { get; set; }
        public string RecognizerTempPath { get; set; }
        public string RecognizerService { get; set; }

        public Dictionary<string,int> Bot { get; set; }


        public static SelfieBotConfig Instance
        {
            get
            {
                if(_Instance==null) _Instance= JsonConvert.DeserializeObject<SelfieBotConfig>(File.ReadAllText(@".\default.conf"));
                return _Instance;
            }
        }

        public string AdultCheck { get; set; }
        public string AdultCheckKey { get; set; }

        private static SelfieBotConfig _Instance = null;


        public  void init()
        {
            File.WriteAllText(@".\init.conf", JsonConvert.SerializeObject(this, Formatting.Indented));
        }
    }
}
