using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Xml;
using Windows.Foundation;
using Windows.Foundation.Collections;
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

        public MainPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
            //ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(200, 100));
            num = 1;
            checking_id = -1;      //已勾选的计划的id号，当要进行页面跳转时，确定要传递的信息
            int i;
            for (i = 0; i < 100; i++)
                undo_array[i] = 0;
            this.title1.Text = "100";
        }

        private void go1_Click(object sender, RoutedEventArgs e)
        {
            if(Window.Current.Bounds.Width < 800)
            {
                string[] package = new string[3];
                package[0] = "";
                package[1] = "";
                package[2] = "";
                Frame frame = Window.Current.Content as Frame;
                if(checking_id != -1)
                {
                    package[0] = title_array[checking_id];
                    package[1] = detail_array[checking_id];
                    package[2] = date_array[checking_id];
                }
                List<string> send = new List<string>(package);
                frame.Navigate(typeof(another), send);
                Window.Current.Content = frame;
                Window.Current.Activate();
            }
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
                num++;
                title_array[num] = this.title1.Text;
                detail_array[num] = this.detail.Text;                   //创建计划
                date_array[num] = datepick.Date.ToString();
                undo_array[num] = 1;

                StackPanel sp = new StackPanel();
                sp.Orientation = Orientation.Horizontal;
                sp.HorizontalAlignment = HorizontalAlignment.Center;
                sp.Width = 600;
                CheckBox cb = new CheckBox();
                cb.Margin = new Thickness(100, 0, 0, 0);
                cb.HorizontalAlignment = HorizontalAlignment.Center;
                cb.Width = 50;
                cb.Height = 50;
                cb.FontSize = 36;
                cb.Name = num.ToString();  //提供勾选checkbox时的索引

                Image img = new Image();
                Uri uri = new Uri(BaseUri, "ms-appx:/Assets/1.jpg");
                img.Source = new BitmapImage(uri);
                img.Width = img.Height = 50;
                img.Margin = new Thickness(-100, 0, 0, 0);

                TextBlock todo = new TextBlock();
                todo.Text = "TODO";
                todo.FontSize = 15;
                todo.Width = todo.Height = 50;
                todo.Margin = new Thickness(-100, 0, 0, 0);
                todo.Padding = new Thickness(2, 15, 0, 0);

                TextBlock tb = new TextBlock();
                tb.HorizontalAlignment = HorizontalAlignment.Left;
                tb.Width = 200;
                tb.Height = 50;
                //Binding binding = new Binding() { Path = new PropertyPath("title_array[num]") };
                //tb.SetBinding(TextBox.TextProperty, binding);
                tb.Text = this.title1.Text;
                tb.Name = "textbox" + num.ToString();
                tb.FontSize = 24;

                Line li = new Line();
                li.HorizontalAlignment = HorizontalAlignment.Left;
                li.X1 = -20;
                li.X2 = 200;
                li.Y1 = li.Y2 = num * 5+12;
                li.Opacity = 0;
                li.Stroke = new SolidColorBrush(Windows.UI.Colors.Black);
                li.StrokeThickness = 5;
                li.Margin = new Thickness(-200, 0, 0, 0);

                Button bt = new Button();
                bt.Width = bt.Height = 50;
                bt.Opacity = 0;
                bt.Background = new SolidColorBrush(Windows.UI.Colors.Red);
                bt.Click += (object sender1, RoutedEventArgs e1) => {

                    sp.Children.Remove(cb);
                    sp.Children.Remove(tb);
                    sp.Children.Remove(li);
                    sp.Children.Remove(bt);             //删除单个计划的函数
                    sp.Children.Remove(img);
                    sp.Children.Remove(todo);

                    title1.Text = "";
                    detail.Text = "";
                    datepick.Date = DateTime.Now;
                    this.create.Content = "Create";

                    undo_array[num] = 0;
                };

                cb.Checked += (object sender1, RoutedEventArgs e1) => {
                    li.Opacity = 1;
                    bt.Opacity = 1;
                    int id = int.Parse(cb.Name);
                    this.title1.Text = title_array[id];                    //勾选单个计划的函数
                    this.detail.Text = detail_array[id];
                    this.datepick.Date = DateTimeOffset.Parse(date_array[id]);
                    this.title1.Name = id.ToString();  //提供更新时的索引
                    this.create.Content = "Upgrade";
                    checking_id = id;
                };
                cb.Unchecked += (object sender1, RoutedEventArgs e1) => 
                {
                    li.Opacity = 0;
                    bt.Opacity = 0;
                    title1.Text = "";
                    detail.Text = "";                   
                    //取消勾选
                    datepick.Date = DateTime.Now;
                    this.create.Content = "Create";
                    checking_id = -1;
                };

                sp.Children.Add(cb);
                sp.Children.Add(img);
                sp.Children.Add(todo);
                sp.Children.Add(tb);
                sp.Children.Add(li);
                sp.Children.Add(bt);
                this.list.Children.Add(sp);

                title1.Text = "";
                detail.Text = "";                    //创建后清空title和detail
                datepick.Date = DateTime.Now;
            }
            else if(result == "" && this.create.Content.ToString() == "Upgrade")
            {
                int id = int.Parse(this.title1.Name);
                title_array[id] = this.title1.Text;                                    //修改信息
                detail_array[id] = this.detail.Text;
                date_array[id] = this.datepick.Date.ToString();
                //string tmp = "textbox" + id.ToString();
                //TextBox change_title = this.FindName(tmp) as TextBox;
                //change_title.Text = this.title1.Text.ToString();
                //this.title.Text = change_title.Text.ToString();

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
            if(this.create.Content.ToString() == "Create")
            {
                title1.Text = "";
                detail.Text = "";                                               //详情页面取消函数
                datepick.Date = DateTime.Now;                                   //创建时取消
            }
            else if(this.create.Content.ToString() == "Upgrade")
            {
                int id = int.Parse(this.title1.Name);
                this.title1.Text = title_array[id];
                this.detail.Text = detail_array[id];                           
                this.datepick.Date = DateTimeOffset.Parse(date_array[id]);      //修改时取消
            }
        }

        private void Slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            this.pic.Width = sli.Value+300;
        }
    }
}
