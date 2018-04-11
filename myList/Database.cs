using SQLitePCL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace myList
{
    public class Database
    {
        private static Database instance;

        private SQLiteConnection conn;

        private static string sql_table = "TODO";

        private static string sql_create_table = "CREATE TABLE IF NOT EXISTS " + sql_table + " (TITLE TEXT NOT NULL, DETAIL TEXT NOT NULL, DATE TEXT NOT NULL, PICTURE TEXT NOT NULL)";

        private static string sql_insert = "INSERT INTO " +sql_table+ " VALUES(?,?,?,?);";

        private static string sql_select = "SELECT TITLE, DETAIL, DATE, PICTURE FROM "+ sql_table + " WHERE TITLE = ?";

        private Database()
        {
            conn = new SQLiteConnection("mylist.db");
            using (var statement = conn.Prepare(sql_create_table))
            {
                statement.Step();
            }
            using (var statement = conn.Prepare(sql_insert))
            {
                statement.Bind(1, "TITLE");
                statement.Bind(2, "DETAIL");
                statement.Bind(3, "2018/5/21 0:10:30 +08:00");
                statement.Bind(4, "0");
                statement.Step();
            }
        }

        public static Database Instance
        {
            get
            {
                if (instance == null)
                    instance = new Database();
                return instance;
            }
        }

        public string command()
        {
            string picture = "123";
            using (var statement = conn.Prepare(sql_select))
            {
                statement.Bind(1, "TITLE");
                if(SQLiteResult.DONE == statement.Step())
                {
                    picture = (string)statement[1];
                }
            }
            return picture;

        }
    }
}
