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

        public IEnumerable<ulong> getWaitRetweet()
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, ulong> getUserList()
        {
            throw new NotImplementedException();
        }

        public ulong getHTLMaxid()
        {
            throw new NotImplementedException();
        }

        public void updateUserList(string key, ulong maxid)
        {
            throw new NotImplementedException();
        }

        public void updateHTLMaxid(ulong newid)
        {
            throw new NotImplementedException();
        }

        public List<string> getBandIDs()
        {
            throw new NotImplementedException();
        }

        public Dictionary<string,ulong> getSearchKey()
        {
            throw new NotImplementedException();
        }

        public void updateSearchKey(string key, ulong v)
        {
            throw new NotImplementedException();
        }   

        public void addWaitDownload(string screenName, ulong statusID, List<string> value)
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
