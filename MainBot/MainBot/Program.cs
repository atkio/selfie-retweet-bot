using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace SelfieBot
{
    class Program
    {

        static List<Process> currentProcess = new List<Process>();
        static void Main(string[] args)
        {
            if (!File.Exists(SelfieBotConfig.Instance.DBConnectString))
            {
                new BotSqliteConnect().CreateDB_TABLE();
                new BotSqliteConnect().DB_WriteDefine();
            }

            var timers=
                SelfieBotConfig.Instance.Bot.Select(kv => CreateProcTimer(kv.Key, kv.Value * 60 * 1000)).ToList();
            //var aTimer1 = CreateProcTimer(@".\SelfieTweetSearch.exe", 13*60*1000);
            //var aTimer2 = CreateProcTimer(@".\SelfieTweetHTLWatcher.exe", 17 * 60 * 1000);
            //var aTimer3 = CreateProcTimer(@".\SelfieFacerecognizer.exe", 5 * 60 * 1000);
            //var aTimer4 = CreateProcTimer(@".\SelfieRetweet.exe", 11 * 60 * 1000);

            Console.WriteLine("push any key to end.");
            Console.ReadKey();

            //aTimer1.Enabled = false;
            //aTimer2.Enabled = false;
            //aTimer3.Enabled = false;
            //aTimer4.Enabled = false;

            timers.ForEach(t => t.Enabled = false);
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
                currentProcess.Add(myProcess);
            });
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
            return aTimer;
        }

        static void Init()
        {
            SelfieBotConfig.Instance.Bot = new Dictionary<string, int>()
            {
                {@".\SelfieTweetSearch.exe",13 },
                 {@".\SelfieTweetHTLWatcher.exe",17 },
                  {@".\SelfieFacerecognizer.exe",5 },
                   {@".\SelfieRetweet.exe",11 },
            };
            SelfieBotConfig.Instance.init();

            SelfieBotConfig config = SelfieBotConfig.Instance;
            new BotSqliteConnect().CreateDB_TABLE();
            new BotSqliteConnect().DB_WriteDefine();
        }
    }
}
