using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace myList
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class another : Page
    {
        int updateing;
        string o_title;
        string o_detail;
        String o_date;
        BitmapImage o_img;
        public another()
        {
            this.InitializeComponent();
            ImageBrush imageBrush = new ImageBrush();
            imageBrush.ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/wood.jpg", UriKind.Absolute));
            this.bar.Background = imageBrush;
            this.main.Background = imageBrush;
            
            Frame rootFrame = Window.Current.Content as Frame;
            rootFrame.Navigated += OnNavigated;
            SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;
        }

        private void OnBackRequested(object sender, Windows.UI.Core.BackRequestedEventArgs e)
        {
            //Frame rootFrame = Window.Current.Content as Frame;
            //if (rootFrame == null)
            //    return;
            //if (rootFrame.CanGoBack && e.Handled == false)
            //{
            //    e.Handled = true;
            //    rootFrame.GoBack();
            //}
            package tmp = new package();
            Frame frame = Window.Current.Content as Frame;
            tmp.title = "";
            tmp.detail = "";
            tmp.date = "";
            updateing = -1;
            tmp.checking = updateing;

            frame.Navigate(typeof(MainPage), tmp);
            Window.Current.Content = frame;
            Window.Current.Activate();
        }

        private void OnNavigated(object sender, NavigationEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                ((Frame)sender).CanGoBack ?
                AppViewBackButtonVisibility.Visible :
                AppViewBackButtonVisibility.Collapsed;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            package tmp = (package)e.Parameter;
            if (tmp.checking != -1)
            {
                this.title1.Text = o_title = tmp.title;            //更新数据
                this.detail.Text = o_detail = tmp.detail;
                o_date = tmp.date;
                this.pic.Source = o_img = tmp.image;
                this.datepick.Date = DateTimeOffset.Parse(tmp.date);
                this.create.Content = "Update";
                this.delete.Opacity = 1;
            }
            else
                this.delete.Opacity = 0;
            updateing = tmp.checking;

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
            //Frame rootFrame = Window.Current.Content as Frame;
            //if (rootFrame == null)
            //    return;
            //if (rootFrame.CanGoBack)
            //{
            //    localSettings.Values["title"] = this.title1.Text;
            //    localSettings.Values["detail"] = this.detail.Text;
            //    localSettings.Values["date"] = this.datepick.Date.ToString();
            //    localSettings.Values["id"] = updateing.ToString();
            //    rootFrame.GoBack();
            //}
            if(result == "success!")
            {
                package tmp = new package();
                Frame frame = Window.Current.Content as Frame;
                tmp.title = this.title1.Text;
                tmp.detail = this.detail.Text;
                tmp.date = this.datepick.Date.ToString();
                tmp.image = (BitmapImage)this.pic.Source;
                if (updateing == -1)
                    updateing = -2;
                tmp.checking = updateing;

                frame.Navigate(typeof(MainPage), tmp);
                Window.Current.Content = frame;
                Window.Current.Activate();
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if(updateing == -1)
            {

                title1.Text = "";
                detail.Text = "";
                var tmp_uri = new Uri(BaseUri, "/Assets/bird.jpg");
                BitmapImage tmp_image = new BitmapImage(tmp_uri);
                pic.Source = new BitmapImage(tmp_uri);
                datepick.Date = DateTime.Now;
            }
            else
            {
                title1.Text = o_title;                              //cancel按钮
                detail.Text = o_detail;
                pic.Source = o_img;
                datepick.Date = DateTimeOffset.Parse(o_date);
            }
        }

        private void sli_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            this.pic.Width = sli.Value + 300;   //滑块调整图片大小
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

        private void delete_Click(object sender, RoutedEventArgs e)
        {
            if (updateing == -1)
                return;
            package tmp = new package();
            Frame frame = Window.Current.Content as Frame;
            tmp.checking = -3;
            tmp.title = updateing.ToString();
            frame.Navigate(typeof(MainPage), tmp);
            Window.Current.Content = frame;
            Window.Current.Activate();
        }
    }
}
