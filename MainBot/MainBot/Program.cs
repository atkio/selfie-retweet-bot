using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace SelfieBot
{
    class Program
    {
        static void Main(string[] args)
        {
            var aTimer1 = CreateProcTimer(@".\SelfieTweetSearch.exe", 13*60*1000);
            var aTimer2 = CreateProcTimer(@".\SelfieTweetHTLWatcher.exe", 17 * 60 * 1000);
            var aTimer3 = CreateProcTimer(@".\SelfieFacerecognizer.exe", 5 * 60 * 1000);
            var aTimer4 = CreateProcTimer(@".\SelfieRetweet.exe", 11 * 60 * 1000);

            Console.WriteLine("push any key to end.");
            Console.ReadKey();

            aTimer1.Enabled = false;
            aTimer2.Enabled = false;
            aTimer3.Enabled = false;
            aTimer4.Enabled = false;
        }

        static Timer CreateProcTimer(string binfile,double interval)
        {
            var aTimer = new Timer(interval);
            aTimer.Elapsed += new ElapsedEventHandler((obj,e)=> {
                var  myProcess = new Process();
                myProcess.StartInfo.UseShellExecute = true;
                myProcess.StartInfo.FileName = binfile;
                myProcess.StartInfo.CreateNoWindow = true;
                myProcess.Start();
            });
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
            return aTimer;
        }

        static void Init()
        {
            SelfieBotConfig config = SelfieBotConfig.Instance;
            new BotSqliteConnect().CreateDB_TABLE();
            new BotSqliteConnect().DB_WriteDefine();
        }
    }
}
