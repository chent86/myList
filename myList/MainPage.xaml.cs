using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Xml;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace myList
{
    public struct package
    {
        public string title;
        public string detail;
        public string date;
        public int checking;
        public BitmapImage image;
    }
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public int num;
        public int checking_id;
        public string[] title_array = new string[100];
        public string[] detail_array = new string[100];        //简陋的数据存储
        public string[] date_array = new string[100];
        public int[] undo_array = new int[100];
        public BitmapImage[] image_array = new BitmapImage[100];

        public MainPage()
        {
            this.InitializeComponent();
            ImageBrush imageBrush = new ImageBrush();
            imageBrush.ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/wood.jpg", UriKind.Absolute));
            this.main.Background = imageBrush;
            this.bar.Background = imageBrush;
            this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
            //ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(200, 100));
            num = 1;
            checking_id = -1;      //已勾选的计划的id号，当要进行页面跳转时，确定要传递的信息
            this.create.Content = "Create";
            int i;
            for (i = 0; i < 100; i++)
                undo_array[i] = 0;

            var tmp_uri = new Uri(BaseUri, "/Assets/bird.jpg");
            BitmapImage tmp_image = new BitmapImage(tmp_uri);

            for (i = 0; i < 8; i++)
                AddData("auto" + i.ToString(), "abc", "2018/5/21 0:10:30 +08:00", tmp_image);
            this.SizeChanged += (s, e) =>
            {
                if(e.NewSize.Width < 600)
                {
                    int count = this.list.Children.Count;
                    int j;
                    for (j = 0; j < count; j++)
                    {
                        StackPanel st = this.list.Children.ElementAt(j) as StackPanel;
                        Image im = st.Children.ElementAt(1) as Image;
                        im.Width = 0;                                               //小于600，图片隐藏
                        TextBlock tb = st.Children.ElementAt(2) as TextBlock;
                        tb.Margin = new Thickness(-110, 0, 3, 0);
                    }
                } else
                {
                    int num = this.list.Children.Count;
                    int k;
                    for (k = 0; k < num; k++)
                    {
                        StackPanel st = this.list.Children.ElementAt(k) as StackPanel;
                        TextBlock tb = st.Children.ElementAt(2) as TextBlock;    //大于600，图片恢复
                        tb.Margin = new Thickness(0, 0, 3, 0);
                        Image im = st.Children.ElementAt(1) as Image;
                        im.Width = 90;
                    }
                    if (e.NewSize.Width < 1200)
                    {
                        this.todo_list.Width =  500 + e.NewSize.Width - 600;
                        this.list_scroll.Width = 500 + e.NewSize.Width - 600;
                        for (k = 0; k < num; k++)
                        {
                            StackPanel st = this.list.Children.ElementAt(k) as StackPanel;

                            //TextBlock tb = st.Children.ElementAt(2) as TextBlock;
                            st.Width = e.NewSize.Width - 600 + 500;
                            //tb.Width = (e.NewSize.Width - 600)/2 + 200;
                            Line li = st.Children.ElementAt(3) as Line;
                            li.X2 = e.NewSize.Width - 600 + 150;
                        }
                    }
                    if (e.NewSize.Width >= 1200)
                    {
                        this.todo_list.Width = this.list_scroll.Width = 500;
                        int count = this.list.Children.Count;
                        int j;
                        for (j = 0; j < count; j++)
                        {
                            StackPanel st = this.list.Children.ElementAt(j) as StackPanel;
                            st.Width = 500;
                            Image im = st.Children.ElementAt(1) as Image;
                            im.Width = 90;
                            TextBlock tb = st.Children.ElementAt(2) as TextBlock;
                            tb.Width = 200;
                            tb.Margin = new Thickness(0, 0, 3, 0);
                            Line li = st.Children.ElementAt(3) as Line;
                            li.X2 = 200;
                        }
                    }
                }
            };
        }

        private void go1_Click(object sender, RoutedEventArgs e)
        {
            if (Window.Current.Bounds.Width < 1200)
            {
                package tmp = new package();
                Frame frame = Window.Current.Content as Frame;
                if(checking_id != -1)
                {
                    
                    tmp.title = title_array[checking_id];
                    tmp.detail = detail_array[checking_id];
                    tmp.date = date_array[checking_id];
                }
                tmp.checking = checking_id;

                frame.Navigate(typeof(another), tmp);
                Window.Current.Content = frame;
                Window.Current.Activate();
            }
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter.ToString() == "")
                return;
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
            base.OnNavigatedTo(e);
            package tmp = (package)e.Parameter;
            if(tmp.checking == -1)
            {
                //do nothing
            }
            else if(tmp.checking == -2)
            {
                AddData(tmp.title, tmp.detail, tmp.date, tmp.image);    //添加计划
            }
            else if(tmp.checking == -3)
            {
                int id = int.Parse(tmp.title);
                int count = this.list.Children.Count;
                int j;
                for (j = 0; j < count; j++)
                {
                    StackPanel st = this.list.Children.ElementAt(j) as StackPanel;
                    if (st.Name == "sp" + id.ToString())
                    {
                        this.list.Children.Remove(st);
                        break;
                    }
                }

            }
            else
            {
                int id = tmp.checking;                   //修改计划
                title_array[id] =tmp.title;
                detail_array[id] = tmp.detail;
                date_array[id] = tmp.date;
                image_array[id] = tmp.image;
                int count = this.list.Children.Count;
                int j;
                for (j = 0; j < count; j++)
                {
                    StackPanel st = this.list.Children.ElementAt(j) as StackPanel;
                    if (st.Name == "sp" + id.ToString())
                    {
                        TextBlock tb = st.Children.ElementAt(2) as TextBlock;
                        tb.Text = title_array[id];
                        Image img = st.Children.ElementAt(1) as Image;
                        img.Source = image_array[id];
                    }
                }
            }
        }

        private void AddData(string title, string detailt, string date, BitmapImage picture)
        {
            num++;
            title_array[num] = title;
            detail_array[num] = detailt;                   //创建计划
            date_array[num] = date;
            undo_array[num] = 1;
            image_array[num] = picture;

            StackPanel sp = new StackPanel();
            sp.Orientation = Orientation.Horizontal;
            sp.HorizontalAlignment = HorizontalAlignment.Left;
            sp.Width = 600;
            sp.Name = "sp" + num.ToString();
            sp.Margin = new Thickness(10, 0, 0, 0);
            CheckBox cb = new CheckBox();
            cb.HorizontalAlignment = HorizontalAlignment.Left;
            cb.Width = 32;
            cb.Height = 32;
            cb.FontSize = 36;
            //cb.Name = num.ToString();  //提供勾选checkbox时的索引

            Image img = new Image();
            img.Source = picture;
            image_array[num] = picture;
            img.Width = img.Height = 90;
            img.Margin = new Thickness(-100, 0, 0, 0);

            TextBlock tb = new TextBlock();
            tb.HorizontalAlignment = HorizontalAlignment.Left;
            tb.Width = 200;
            tb.Height = 50; 
            //Binding binding = new Binding() { Path = new PropertyPath("title_array[num]") };
            //tb.SetBinding(TextBox.TextProperty, binding);
            tb.Text = title;
            tb.Padding = new Thickness(30, 0, 0, 0);
            tb.Name = "textbox" + num.ToString();
            tb.FontSize = 24;

            Line li = new Line();
            li.HorizontalAlignment = HorizontalAlignment.Left;
            li.X1 = 20;
            li.X2 = 200;
            li.Stretch = Stretch.Fill;
            li.Opacity = 0;
            li.Stroke = new SolidColorBrush(Windows.UI.Colors.Black);
            li.StrokeThickness = 2;
            li.Margin = new Thickness(-200, 0, 0, 15);

            AppBarButton ab = new AppBarButton();
            ab.Icon = new SymbolIcon(Symbol.Setting);
            ab.IsCompact = true;
            ab.VerticalAlignment = VerticalAlignment.Center;
            //ab.HorizontalAlignment = HorizontalAlignment.Right;
            ab.Name = num.ToString(); //提供编辑button的索引

            MenuFlyout mfly = new MenuFlyout();

            MenuFlyoutItem edit = new MenuFlyoutItem();
            MenuFlyoutItem del = new MenuFlyoutItem();
            edit.Text = "Edit";
            del.Text = "Delete";
            mfly.Items.Add(edit);
            mfly.Items.Add(del);

            FlyoutBase.SetAttachedFlyout(ab, mfly);

            ab.Click += (object sender1, RoutedEventArgs e1) =>
            {
                FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender1);
            };

            edit.Click += (object sender1, RoutedEventArgs e1) =>
            {
                if (Window.Current.Bounds.Width >= 1200)
                {
                    int id = int.Parse(ab.Name);
                    this.title1.Text = title_array[id];                    //勾选单个计划的函数
                    this.detail.Text = detail_array[id];
                    this.datepick.Date = DateTimeOffset.Parse(date_array[id]);
                    this.pic.Source = image_array[id];
                    this.title1.Name = id.ToString();  //提供更新时的索引
                    this.create.Content = "Upgrade";
                    checking_id = id;
                }
                else
                {
                    package tmp = new package();
                    int id = int.Parse(ab.Name);
                    tmp.title = title_array[id];
                    tmp.detail = detail_array[id];
                    tmp.date = date_array[id];
                    tmp.image = image_array[id];
                    tmp.checking = id;

                    Frame frame = Window.Current.Content as Frame;
                    frame.Navigate(typeof(another), tmp);
                    Window.Current.Content = frame;
                    Window.Current.Activate();
                }
            };

            del.Click += (object sender1, RoutedEventArgs e1) =>
            {
                sp.Children.Remove(cb);
                sp.Children.Remove(tb);
                sp.Children.Remove(li);  //删除单个计划的函数
                sp.Children.Remove(ab);
                sp.Children.Remove(img);
                this.list.Children.Remove(sp);

                title1.Text = "";
                detail.Text = "";
                datepick.Date = DateTime.Now;
                Uri tmp = new Uri(BaseUri, "Assets/bird.jpg");
                pic.Source = new BitmapImage(tmp);
                this.create.Content = "Create";

                undo_array[num] = 0;
            };

            cb.Checked += (object sender1, RoutedEventArgs e1) =>
            {
                li.Opacity = 1;
            };
            cb.Unchecked += (object sender1, RoutedEventArgs e1) =>
            {
                li.Opacity = 0;
            };

            sp.Children.Add(cb);
            sp.Children.Add(img);
            sp.Children.Add(tb);
            sp.Children.Add(li);
            //sp.Children.Add(bt);
            sp.Children.Add(ab);
            this.list.Children.Add(sp);

            title1.Text = "";
            detail.Text = "";                    //创建后清空title和detail
            datepick.Date = DateTime.Now;
            Uri tmp_uri = new Uri(BaseUri, "Assets/bird.jpg");
            pic.Source = new BitmapImage(tmp_uri);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string result = "";
            if (this.title1.Text == "")
                result += "Title can't be empty!\n";
            if (this.detail.Text == "")
                result += "Detail can't be empty!\n";
            DateTime dt = DateTime.Now;
            if (datepick.Date < dt)
                result += "The due date has passed!\n";
            ShowMessageDialog(result);
            if (result == "" && this.create.Content.ToString() == "Create")
            {
                AddData(this.title1.Text, this.detail.Text, this.datepick.Date.ToString(), (BitmapImage)this.pic.Source);
 
            }
            else if (result == "" && this.create.Content.ToString() == "Upgrade")
            {
                int id = int.Parse(this.title1.Name);
                title_array[id] = this.title1.Text;                                    //修改信息
                detail_array[id] = this.detail.Text;
                date_array[id] = this.datepick.Date.ToString();
                image_array[id] = this.pic.Source as BitmapImage;
                title1.Text = "";
                detail.Text = "";
                datepick.Date = DateTime.Now;
                Uri tmp_uri = new Uri(BaseUri, "Assets/bird.jpg");
                pic.Source = new BitmapImage(tmp_uri);
                this.create.Content = "Create";
                
                checking_id = -1;
                int count = this.list.Children.Count;
                int j;
                for(j = 0; j < count; j++)
                {
                    StackPanel st = this.list.Children.ElementAt(j) as StackPanel;
                    if (st.Name == "sp"+id.ToString())
                    {
                        TextBlock tb = st.Children.ElementAt(2) as TextBlock;
                        tb.Text = title_array[id];
                        Image t_img = st.Children.ElementAt(1) as Image;
                        t_img.Source = image_array[id];
                    }
                }

            }
        }
        private async void ShowMessageDialog(string result)
        {
            var msgDialog = new Windows.UI.Popups.MessageDialog("create");
            //msgDialog.Commands.Add(new Windows.UI.Popups.UICommand("确定", uiCommand => { this.tb.Text = $"您点击了：{uiCommand.Label}"; }));
            //msgDialog.Commands.Add(new Windows.UI.Popups.UICommand("取消", uiCommand => { this.tb.Text = $"您点击了：{uiCommand.Label}"; }));
            if (result == "")
                result = "success!";
            msgDialog.Content = result;                   //弹框函数
            await msgDialog.ShowAsync();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (this.create.Content.ToString() == "Create")
            {
                title1.Text = "";
                detail.Text = "";                                               //详情页面取消函数
                datepick.Date = DateTime.Now;                                   //创建时取消
                var tmp_uri = new Uri(BaseUri, "/Assets/bird.jpg");
                BitmapImage tmp_image = new BitmapImage(tmp_uri);
                pic.Source = tmp_image;
            }
            else if (this.create.Content.ToString() == "Upgrade")
            {
                int id = int.Parse(this.title1.Name);
                this.title1.Text = title_array[id];
                this.detail.Text = detail_array[id];
                this.datepick.Date = DateTimeOffset.Parse(date_array[id]);      //修改时取消
                pic.Source = image_array[id];
            }
        }

        private void Slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            this.pic.Width = sli.Value + 300;
        }

        private async void Select_picture(object sender, RoutedEventArgs e)
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".jpeg");
            openPicker.FileTypeFilter.Add(".png");

            StorageFile file = await openPicker.PickSingleFileAsync();
            if (file != null)
            {
                // Application now has read/write access to the picked file
                IRandomAccessStream ir = await file.OpenAsync(FileAccessMode.Read);
                BitmapImage bi = new BitmapImage();
                await bi.SetSourceAsync(ir);
                pic.Source = bi;
            }
        }
    }
}
