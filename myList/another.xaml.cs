using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace myList
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class another : Page
    {
        public another()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            //将传过来的数据 类型转换一下
            var myList = e.Parameter as List<string>;
            this.title1.Text = myList[0];
            this.detail.Text = myList[1];
            if(myList[2] != "")
                this.datepick.Date = DateTimeOffset.Parse(myList[2]);
        }

        private void back2_Click(object sender, RoutedEventArgs e)
        {
            Frame frame = new Frame();
            frame.Navigate(typeof(MainPage), "");
            Window.Current.Content = frame;
            Window.Current.Activate();
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
        }

        private async void ShowMessageDialog(string result)
        {
            var msgDialog = new Windows.UI.Popups.MessageDialog("create");
            //msgDialog.Commands.Add(new Windows.UI.Popups.UICommand("确定", uiCommand => { this.tb.Text = $"您点击了：{uiCommand.Label}"; }));
            //msgDialog.Commands.Add(new Windows.UI.Popups.UICommand("取消", uiCommand => { this.tb.Text = $"您点击了：{uiCommand.Label}"; }));
            if (result == "")
                result = "success!";
            msgDialog.Content = result;
            await msgDialog.ShowAsync();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            title1.Text = "";                              //cancel按钮
            detail.Text = "";
            datepick.Date = DateTime.Now;
        }

        private void sli_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            this.pic.Width = sli.Value + 300;   //滑块调整图片大小
        }
    }
}
