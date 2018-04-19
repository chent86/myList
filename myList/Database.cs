using SQLitePCL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace myList
{
    public class Database
    {
        private static Database instance;

        private SQLiteConnection conn;

        private static string sql_table = "TODO";

        private static string sql_create_table = "CREATE TABLE IF NOT EXISTS " + sql_table + " (CREATE_DATE TEXT NOT NULL,TITLE TEXT NOT NULL, DETAIL TEXT NOT NULL, DATE TEXT NOT NULL, PICTURE TEXT NOT NULL, LINE TEXT NOT NULL)";

        private static string sql_insert = "INSERT INTO " +sql_table+ " VALUES(?,?,?,?,?,?);";

        private static string sql_select = "SELECT CREATE_DATE, TITLE, DETAIL, DATE, PICTURE, LINE FROM "+ sql_table;

        private static string sql_update = "UPDATE " + sql_table + " SET TITLE = ?, DETAIL = ?, DATE = ?, PICTURE = ? WHERE CREATE_DATE = ?";

        private static string sql_delete = "DELETE FROM " + sql_table + " WHERE CREATE_DATE = ?";

        private static string sql_update_line = "UPDATE " + sql_table + " SET LINE = ? WHERE CREATE_DATE = ?";

        private static string sql_info_table = "INFO";

        private static string sql_create_info_table = "CREATE TABLE IF NOT EXISTS " + sql_info_table + " (PICTURE_COUNT TEXT NOT NULL, BACKGROUND TEXT NOT NULL)";

        private static string sql_select_info = "SELECT PICTURE_COUNT FROM " + sql_info_table;

        private static string sql_update_info = "UPDATE " + sql_info_table + " SET PICTURE_COUNT = ? ";

        private static string sql_select_background = "SELECT BACKGROUND FROM " + sql_info_table;

        private static string sql_update_background = "UPDATE " + sql_info_table + " SET BACKGROUND = ? ";

        private static string sql_fuzzy_search = "SELECT TITLE, DETAIL, DATE FROM " + sql_table + " WHERE TITLE LIKE ? OR DETAIL LIKE ?";

        private Database()
        {
            conn = new SQLiteConnection("mylist.db");
            using (var statement = conn.Prepare(sql_create_table))
            {
                statement.Step();
            }
            using (var statement = conn.Prepare(sql_create_info_table))
            {
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

        public void insert(Todo newtodo)
        {
            using (var statement = conn.Prepare(sql_insert))
            {
                statement.Bind(1, newtodo.create_date);
                statement.Bind(2, newtodo.Title);
                statement.Bind(3, newtodo.Detail);
                statement.Bind(4, newtodo.Date);
                statement.Bind(5, newtodo.picture_id);
                statement.Bind(6, "notcheck");
                statement.Step();
            }
        }

        public void delete(string create_time)
        {
            using (var statement = conn.Prepare(sql_delete))
            {
                statement.Bind(1, create_time);
                statement.Step();
            }
        }

        public void update_line(string check,string create_time)
        {
            using (var statement = conn.Prepare(sql_update_line))
            {
                statement.Bind(1, check);
                statement.Bind(2, create_time);
                statement.Step();
            }
        }

        public void update(Todo newtodo)
        {
            using (var statement = conn.Prepare(sql_update))
            {
                statement.Bind(1, newtodo.Title);
                statement.Bind(2, newtodo.Detail);
                statement.Bind(3, newtodo.Date);
                statement.Bind(4, newtodo.picture_id);  //实际上id不会改，只是图片改了
                statement.Bind(5, newtodo.create_date);
                statement.Step();
            }
        }

        public List<Todo> reload()
        {
            List<Todo> todo_list = new List<Todo>();
            using (var statement = conn.Prepare(sql_select))
            {
                while (SQLiteResult.ROW == statement.Step())
                {
                    Todo tmp = new Todo { create_date = (string)statement[0], Title = (string)statement[1], Detail = (string)statement[2], Date = (string)statement[3], picture_id = (string)statement[4] };
                    if ((string)statement[5] == "check")
                        tmp.Is_check = true;
                    else
                        tmp.Is_check = false;
                    get_picture(tmp);
                    todo_list.Add(tmp);
                }
            }
            return todo_list;
        }

        private async void get_picture(Todo tmp)
        {
            StorageFolder appInstalledFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            string image = @"Assets\picture" + tmp.picture_id + ".jpg";
            StorageFile logoImage = await appInstalledFolder.GetFileAsync(image);

            IRandomAccessStream ir = await logoImage.OpenAsync(FileAccessMode.Read);
            BitmapImage bi = new BitmapImage();
            await bi.SetSourceAsync(ir);
            tmp.Picture = bi;
            
        }

        public int get_picture_count()
        {
            string count = "-1";
            using (var statement = conn.Prepare(sql_select_info))
            {
                if(SQLiteResult.ROW == statement.Step())
                {
                    count = (string)statement[0];
                }
            }
            return int.Parse(count);
        }

        public void set_picture_count(string count)
        {
            using (var statement = conn.Prepare(sql_update_info))
            {
                statement.Bind(1, count);
                statement.Step();
            }
        }

        public string get_background()
        {
            string background = "wood.jpg";
            using (var statement = conn.Prepare(sql_select_background))
            {
                if (SQLiteResult.ROW == statement.Step())
                {
                    background = (string)statement[0];
                }
            }
            return background;
        }

        public void set_background(string background)
        {
            using (var statement = conn.Prepare(sql_update_background))
            {
                statement.Bind(1, background);
                statement.Step();
            }
        }

        public void init_count()
        {
            using (var new_statement = conn.Prepare("INSERT INTO " + sql_info_table + " VALUES(?,?);"))
            {
                new_statement.Bind(1, "1");
                new_statement.Bind(2, "wood.jpg");
                new_statement.Step();
            }
        }

        public List<Todo> fuzzy_search(string info)
        {
            List<Todo> todo_list = new List<Todo>();
            string fuzzy = "%"+info+"%";
            using (var statement = conn.Prepare(sql_fuzzy_search))
            {
                statement.Bind(1, fuzzy);
                statement.Bind(2, fuzzy);
                while (SQLiteResult.ROW == statement.Step())
                {
                    Todo tmp = new Todo {Title = (string)statement[0], Detail = (string)statement[1], Date = (string)statement[2]};
                    todo_list.Add(tmp);
                }
            }
            return todo_list;
        }
    }
}
