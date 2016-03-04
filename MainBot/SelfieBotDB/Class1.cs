using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelfieBot
{
    public class SelfieBotDB
    {
        public SelfieBotDB()
        {

        }

        private string dbconnectString;

        public List<WaitRecognizer> getAllWaitRecognizer()
        {
            return new List<WaitRecognizer>();
        }
       

        public void addToRetweet()
        {
            throw new NotImplementedException();
        }

        public List<string> getBlockTexts()
        {
            throw new NotImplementedException();
        }

        public List<string> getNameBlockTexts()
        {
            throw new NotImplementedException();
        }

        public List<string> getBandIDs()
        {
            throw new NotImplementedException();
        }

        public void addWaitRecognizer(WaitRecognizer ul)
        {
            throw new NotImplementedException();
        }

        public void removeWaitRecognizer(WaitRecognizer nr)
        {
            throw new NotImplementedException();
        }
    }
}
