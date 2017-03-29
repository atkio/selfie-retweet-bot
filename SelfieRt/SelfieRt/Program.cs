using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SelfieRt
{
    class Program
    {
        static SelfieBotConfig config = SelfieBotConfig.Instance;
        static void Main(string[] args)
        {
            try
            {
              
                if(args.Count() >0)
                {
                    BotSqliteConnect.CreateDB_TABLE();
                    Console.ReadKey();
                    return;
                }

                var db = new SelfieBotDB();

                var nrs = db.getAllWaitRecognizer();

                List<WaitRecognizer> isfaces = new List<WaitRecognizer>();
                List<WaitRecognizer> nofaces = new List<WaitRecognizer>();

                foreach (var grp in nrs.GroupBy(n => n.TID))
                {
                    if (grp.Any(n => SelfieFacerecognizer.Detect(n.PhotoPath, n.PhotoUrl)))
                        isfaces.AddRange(grp);
                    else
                        nofaces.AddRange(grp);
                }
                
                foreach(var todel in nrs)
                {
                     File.Delete(todel.PhotoPath);
                }

                if (config.RecognizerService == "true")
                {
                    var adultida = isfaces.GroupBy(n => n.UID)
                                  .Where(grp => grp.Any(nr => MicrosoftFace.AnalyzeUrlAdult(nr.PhotoUrl)))
                                  .Select(grp=>grp.Key).ToList();

                    db.addBandIDs(adultida);

                    isfaces = isfaces.Where(wr => !adultida.Contains(wr.UID))
                                  .GroupBy(wr => wr.TID)
                                  .Where(grp => grp.Any(nr => MicrosoftFace.MakeRequestUrl(nr.PhotoUrl)))
                                  .SelectMany(grp => grp)
                                  .ToList();                                   

                }
                

                foreach (var tid in isfaces.Select(i=>i.TID).Distinct())
                {
                    db.addToRetweet(tid);
                }

                db.removeAllWaitRecognizer();

                //
                //SelfieFacerecognizer.Run();

                //
                SelfieRetweet.Run();

                //
                SelfieTweetHTLWatcher.Run();

                //
                SelfieTweetSearch.Run();


             
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
               Console.WriteLine(e.StackTrace);
            }
        }
    }
}
