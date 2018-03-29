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
    public sealed partial class NewPage : Page
    {
        private Todo current;
        private BitmapImage default_image;
        public NewPage()
        {
            this.InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Enabled;    //缓存页面
            ImageBrush imageBrush = new ImageBrush();
            imageBrush.ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/wood.jpg", UriKind.Absolute));
            default_image = new BitmapImage(new Uri("ms-appx:///Assets/bird.jpg"));
            this.bar.Background = imageBrush;
            this.main.Background = imageBrush;
            Frame rootFrame = Window.Current.Content as Frame;
            rootFrame.Navigated += OnNavigated;
            SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;
        }

        private void OnNavigated(object sender, NavigationEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                ((Frame)sender).CanGoBack ?
                AppViewBackButtonVisibility.Visible :
                AppViewBackButtonVisibility.Collapsed;
        }

        private void OnBackRequested(object sender, Windows.UI.Core.BackRequestedEventArgs e)
        {
            Frame frame = Window.Current.Content as Frame;
            frame.Navigate(typeof(MainPage), "1");
            Window.Current.Content = frame;
            Window.Current.Activate();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            package tmp = (package)e.Parameter;
            current = tmp.processing;

            if(tmp.type == 0)
            {
                this.Create.Content = "Create";
                this.delete.Opacity = 0;
            }
            else if(tmp.type == 1)
            {
                this.Create.Content = "Update";
                this.title.Text = current.Title;
                this.detail.Text = current.Detail;
                this.pic.Source = current.Picture;
                this.datepick.Date = DateTimeOffset.Parse(current.Date);
            }
        }

        private void sli_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            this.pic.Width = sli.Value + 300;
        }

        private async void select_Button(object sender, RoutedEventArgs e)
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

        private void reset_RightPart()
        {
            title.Text = detail.Text = "";
            pic.Source = default_image;
            datepick.Date = DateTime.Now;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            if (this.Create.Content.ToString() == "Create")
            {
                reset_RightPart();
            }
            else
            {
                this.title.Text = current.Title;
                this.detail.Text = current.Detail;
                this.pic.Source = current.Picture;
                this.datepick.Date = DateTimeOffset.Parse(current.Date);
            }
        }

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            string result = "";
            if (this.title.Text == "")
                result += "Title can't be empty!\n";
            if (this.detail.Text == "")
                result += "Detail can't be empty!\n";
            DateTime dt = DateTime.Now;
            if (datepick.Date < dt)
                result += "The due date has passed!\n";
            ShowMessageDialog(result);
            if (result == "" && this.Create.Content.ToString() == "Create")
            {
                current.Title = this.title.Text;
                current.Detail = this.detail.Text;
                current.Date = this.datepick.Date.ToString();
                current.Picture = this.pic.Source;
                reset_RightPart();
                Frame frame = Window.Current.Content as Frame;
                frame.Navigate(typeof(MainPage), "0");
                Window.Current.Content = frame;
                Window.Current.Activate();

            }
            else if (result == "" && this.Create.Content.ToString() == "Update")
            {
                current.Title = title.Text;
                current.Detail = detail.Text;
                current.Date = datepick.Date.ToString();
                current.Picture = pic.Source;

                reset_RightPart();

                this.Create.Content = "Create";
                this.delete.Opacity = 0;

                Frame frame = Window.Current.Content as Frame;
                frame.Navigate(typeof(MainPage), "1");
                Window.Current.Content = frame;
                Window.Current.Activate();
            }
        }

        private async void ShowMessageDialog(string result)
        {
            var msgDialog = new Windows.UI.Popups.MessageDialog("create");
            if (result == "")
                result = "success!";
            msgDialog.Content = result;
            await msgDialog.ShowAsync();
        }

        private void delete_Click(object sender, RoutedEventArgs e)
        {
            Frame frame = Window.Current.Content as Frame;
            frame.Navigate(typeof(MainPage), "2");
            Window.Current.Content = frame;
            Window.Current.Activate();
        }

        private async void change_background_Click(object sender, RoutedEventArgs e)
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
                // Application now has read/write access to the picked file                      更改背景图片
                IRandomAccessStream ir = await file.OpenAsync(FileAccessMode.Read);
                BitmapImage bi = new BitmapImage();
                await bi.SetSourceAsync(ir);
                ImageBrush imageBrush = new ImageBrush();
                imageBrush.ImageSource = bi;
                this.main.Background = imageBrush;
                this.bar.Background = new SolidColorBrush(Windows.UI.Colors.Gray);
            }
        }
    }
}
