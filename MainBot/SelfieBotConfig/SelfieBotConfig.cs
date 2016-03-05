using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelfieBot
{
    public class SelfieBotConfig
    {
        public readonly string AccessToken;
        public readonly string AccessTokenSecret;
        public readonly string ConsumerKey;
        public readonly string ConsumerSecret;

        public string DB { get;  }
        public string RecognizerKey { get { return "b72a8f29ddc4470897525e21a36b58d3"; } }

        public string TempPath { get; set; }

        public string MyID()
        {
            throw new NotImplementedException();
        }
    }
}
