using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace myList
{
    public class Todo : INotifyPropertyChanged
    {
        private string title;
        private string detail;
        private ImageSource picture;
        private string date;
        private Windows.UI.Xaml.Visibility visi { get; set; }
        private Boolean? is_check;

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public string picture_id;
        public string create_date;
        private Database m_db;
        private int just_create;

        public Todo()
        {
            this.Title = "todo";
            this.detail = "testing";
            string path = "ms-appx:///Assets/picture0.jpg";
            this.Picture = new BitmapImage(new Uri(path));
            this.picture_id = "0";
            this.date = "2020/5/21 0:10:30 +08:00";
            this.create_date = DateTime.Now.ToString();
            visi = Windows.UI.Xaml.Visibility.Collapsed;
            Is_check = false;
            m_db = Database.Instance;
            just_create = 1;
        }

        public string Title
        {
            get { return this.title; }
            set
            {
                this.title = value;
                this.OnPropertyChanged();
            }
        }

        public string Detail
        {
            get { return this.detail; }
            set
            {
                this.detail = value;
                this.OnPropertyChanged();
            }
        }

        public ImageSource Picture
        {
            get { return this.picture; }
            set
            {
                this.picture = value;
                this.OnPropertyChanged();
            }
        }

        public string Date
        {
            get { return this.date; }
            set
            {
                this.date = value;
                this.OnPropertyChanged();
            }
        }

        public Windows.UI.Xaml.Visibility Visi
        {
            get { return this.visi; }
            set
            {
                this.visi = value;
                this.OnPropertyChanged();
            }
        }

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            // Raise the PropertyChanged event, passing the name of the property whose value has changed.
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public Boolean? Is_check
        {
            get { return this.is_check; }
            set
            {
                this.is_check = value;
                if (is_check == true)
                {
                    Visi = Windows.UI.Xaml.Visibility.Visible;
                    m_db.update_line("check", this.create_date);
                }
                else
                {
                    Visi = Windows.UI.Xaml.Visibility.Collapsed;
                    if (just_create != 0)
                        m_db.update_line("notcheck", this.create_date);
                    else
                        just_create = 1;
                }
                this.OnPropertyChanged();
            }
        }

    }

    public class TodoManager
    {
        private ObservableCollection<Todo> defaultTodo = new ObservableCollection<Todo>();
        public ObservableCollection<Todo> DefaultTodo { get { return this.defaultTodo; } }
        public TodoManager()
        {
            TileService.SetBadgeCountOnTile(0);
            int count = 0;
            Database m_db = Database.Instance;
            List<Todo> todo_list = m_db.reload();
            foreach (Todo i in todo_list)
            {
                this.DefaultTodo.Add(i);
                count++;
                TileService.SetBadgeCountOnTile(count);
            }
        }
    }
}
