using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelfieBot
{
    class Program
    {
        static void Main(string[] args)
        {
            SelfieBotConfig config = SelfieBotConfig.Instance;
            new BotSqliteConnect().CreateDB_TABLE();
            new   BotSqliteConnect().DB_WriteDefine();
        }
    }
}
