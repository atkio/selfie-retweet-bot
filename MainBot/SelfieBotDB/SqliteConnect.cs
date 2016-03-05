using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Data.SQLite;

namespace SelfieBot
{
    class SqliteConnect
    {

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
