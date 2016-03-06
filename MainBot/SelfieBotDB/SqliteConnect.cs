using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Data.SQLite;
using System.IO;

namespace SelfieBot
{
    public class BotSqliteConnect
    {
        public void CreateDB_TABLE()
        {
            SQLiteConnection.CreateFile(config.DBConnectString);
            using (var con =  GetSqlConnection())
            {
                try
                {
                    using (var command = new SQLiteCommand(con.OpenAndReturn()))
                    {
                        
                        command.CommandText = CreateCommand(typeof(WaitRecognizer));
                        command.ExecuteNonQuery();
                        command.CommandText = CreateCommand(typeof(WaitRetweet));
                        command.ExecuteNonQuery();
                        command.CommandText = CreateCommand(typeof(HomeTimeLineMAXID));
                        command.ExecuteNonQuery();
                        command.CommandText = CreateCommand(typeof(SearchKeys));
                        command.ExecuteNonQuery();
                        command.CommandText = CreateCommand(typeof(WatchUsers));
                        command.ExecuteNonQuery();
                        command.CommandText = CreateCommand(typeof(BandIDs));
                        command.ExecuteNonQuery();
                        command.CommandText = CreateCommand(typeof(BlockText));
                        command.ExecuteNonQuery();
                        command.CommandText = CreateCommand(typeof(BlockName));
                        command.ExecuteNonQuery();
                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        public void DB_WriteDefine()
        {

            SelfieBotDB db = new SelfieBotDB();
            var lines= File.ReadAllLines(@"D:\Source\Repos\selfie-retweet-bot\Defines\blockKeywords.txt").ToList();
            db.setBlockTexts(lines);

            lines = File.ReadAllLines(@"D:\Source\Repos\selfie-retweet-bot\Defines\IDBlackList.txt").ToList();
            db.setBandIDs(lines);

            lines = File.ReadAllLines(@"D:\Source\Repos\selfie-retweet-bot\Defines\IDWhiteList.txt").ToList();
            db.setUserList(lines);

            lines = File.ReadAllLines(@"D:\Source\Repos\selfie-retweet-bot\Defines\NameBlockKeywords.txt").ToList();
            db.setNameBlockTexts(lines);

            lines = File.ReadAllLines(@"D:\Source\Repos\selfie-retweet-bot\Defines\SearchKeywords.txt").ToList();
            foreach(var line in lines)
            db.updateSearchKey(line,3200);

        }



        static SelfieBotConfig config = SelfieBotConfig.Instance;
        public static SQLiteConnection GetSqlConnection()
        {
            return new SQLiteConnection(
                new SQLiteConnectionStringBuilder
                {
                    DataSource = config.DBConnectString
                }.ConnectionString);
        }

        public static string CreateCommand(Type type)
        {

            var tableName = (Attribute.GetCustomAttribute(type, typeof(TableAttribute)) as TableAttribute).Name;
            var sb = new StringBuilder("CREATE TABLE IF NOT EXISTS ");
            sb.Append(tableName).Append("(");

            var attrs = type.GetProperties()
                            .Select(x => Attribute.GetCustomAttribute(x, typeof(ColumnAttribute)) as ColumnAttribute);
            foreach (var fieldAttr in attrs)
            {
                var columnName = fieldAttr.Name;
                var dbType = fieldAttr.DbType;
                sb.AppendFormat("{0} {1}", columnName, dbType);

                if (!fieldAttr.CanBeNull)
                {
                    sb.Append(" NOT NULL");
                }

                sb.Append(",");
            }

            sb.Remove(sb.Length - 1, 1);

            if (attrs.Any(x => x.IsPrimaryKey))
            {
                sb.Append(", PRIMARY KEY(");
                foreach (var primaryKey in attrs.Where(x => x.IsPrimaryKey))
                {
                    sb.Append(primaryKey.Name).Append(",");
                }

                sb.Remove(sb.Length - 1, 1);
                sb.Append(")");
            }

            sb.Append(");");

            return sb.ToString();
        }
    }
}
