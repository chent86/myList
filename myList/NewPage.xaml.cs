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
        private Singleton m_singleton;
        public NewPage()
        {
            this.InitializeComponent();
            m_singleton = Singleton.Instance;
            NavigationCacheMode = NavigationCacheMode.Enabled;    //缓存页面
            ImageBrush imageBrush = new ImageBrush();
            imageBrush.ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/wood.jpg", UriKind.Absolute));
            default_image = new BitmapImage(new Uri("ms-appx:///Assets/picture0.jpg"));
            this.bar.Background = imageBrush;
            this.main.Background = imageBrush;
            this.pic.DataContext = "0";
            Frame rootFrame = Window.Current.Content as Frame;
            //rootFrame.Navigated += OnNavigated;
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;
            this.pic.DataContext = "0";
        }

        private void OnBackRequested(object sender, Windows.UI.Core.BackRequestedEventArgs e)
        {
            Singleton tmp = Singleton.Instance;
            tmp.set_signal("modify_or_simple");
            Frame frame = Window.Current.Content as Frame;
            frame.Navigate(typeof(MainPage), "");
            Window.Current.Content = frame;
            Window.Current.Activate();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            bool suspending = ((App)App.Current).issuspend;
            if (suspending)
            {
                ApplicationDataCompositeValue composite = new ApplicationDataCompositeValue();
                composite["title"] = title.Text;
                composite["detail"] = detail.Text;
                composite["date"] = datepick.Date;
                composite["picture"] = this.pic.DataContext;   //原本的current已经丢失了
                Singleton tmp = Singleton.Instance;
                composite["signal"] = tmp.get_signal();
                ApplicationData.Current.LocalSettings.Values["newpage"] = composite;
            }
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.New)
            {
                ApplicationData.Current.LocalSettings.Values.Remove("newpage");
            }
            else if (ApplicationData.Current.LocalSettings.Values.ContainsKey("newpage"))
            {
                var composite = ApplicationData.Current.LocalSettings.Values["newpage"] as ApplicationDataCompositeValue;
                this.pic.DataContext = (string)composite["picture"];
                string image = "picture"+this.pic.DataContext.ToString()+".jpg";
                StorageFolder appInstalledFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
                StorageFolder assetsFolder = await appInstalledFolder.GetFolderAsync("Assets");
                StorageFile logoImage = await assetsFolder.GetFileAsync(image);

                IRandomAccessStream ir = await logoImage.OpenAsync(FileAccessMode.Read);
                BitmapImage bi = new BitmapImage();
                await bi.SetSourceAsync(ir);
                pic.Source = bi;
                title.Text = (string)composite["title"];
                detail.Text = (string)composite["detail"];
                datepick.Date = (DateTimeOffset)composite["date"];
                //DateTimeOffset tmp_date = (DateTimeOffset)composite["date"];
                //t.set_todo(new Todo
                //            {
                //                Title = (string)composite["title"],
                //                Detail = (string)composite["detail"],
                //                Date = tmp_date.ToString(),
                //                Picture = bi,
                //                picture_id = (string)composite["picture"]
                //            });
                //m_singleton.set_signal((string)composite["signal"]);
                ApplicationData.Current.LocalSettings.Values.Remove("newpage");

            }

            base.OnNavigatedTo(e);
            current = m_singleton.get_todo();

            if(m_singleton.get_signal() == "simple")
            {
                this.Create.Content = "Create";
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
                this.delete.Opacity = 0;
                this.share.Opacity = 0;
                this.pic.DataContext = "0";
            }
            else if(m_singleton.get_signal() == "update")
            {
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
                this.Create.Content = "Update";
                this.title.Text = current.Title;
                this.detail.Text = current.Detail;
                this.pic.DataContext = current.picture_id;
                string image = "picture" + this.pic.DataContext.ToString() + ".jpg";
                StorageFolder appInstalledFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
                StorageFolder assetsFolder = await appInstalledFolder.GetFolderAsync("Assets");
                StorageFile logoImage = await assetsFolder.GetFileAsync(image);

                IRandomAccessStream ir = await logoImage.OpenAsync(FileAccessMode.Read);
                BitmapImage bi = new BitmapImage();
                await bi.SetSourceAsync(ir);
                pic.Source = bi;

                this.datepick.Date = DateTimeOffset.Parse(current.Date);
                this.delete.Opacity = 1;
                this.share.Opacity = 1;
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
                // Application now has read/write access to the picked file                       图片加载
                IRandomAccessStream ir = await file.OpenAsync(FileAccessMode.Read);
                BitmapImage bi = new BitmapImage();
                await bi.SetSourceAsync(ir);
                pic.Source = bi;
                StorageFolder appInstalledFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
                StorageFolder assetsFolder = await appInstalledFolder.GetFolderAsync("Assets");
                string picture_name;
                if (m_singleton.get_signal() != "update")
                    picture_name = "picture" + m_singleton.get_picture_count() + ".jpg";
                else
                    picture_name = "picture" + this.pic.DataContext.ToString() + ".jpg";
                StorageFile copiedFile = await file.CopyAsync(assetsFolder, picture_name, NameCollisionOption.ReplaceExisting);
                pic.DataContext = m_singleton.get_picture_count();
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
            Singleton tmp = Singleton.Instance;
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
                if (current == null)
                    current = new Todo();
                current.Title = this.title.Text;
                current.Detail = this.detail.Text;
                current.Date = this.datepick.Date.ToString();
                current.Picture = this.pic.Source;
                current.picture_id = this.pic.DataContext.ToString();
                reset_RightPart();
                tmp.set_signal("add");
                if (pic.DataContext.ToString() != "0")
                    m_singleton.add_picture_count();
                Frame frame = Window.Current.Content as Frame;
                frame.Navigate(typeof(MainPage), "");
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

                tmp.set_signal("modify_or_simple");
                Frame frame = Window.Current.Content as Frame;
                frame.Navigate(typeof(MainPage), "");
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
            Singleton tmp = Singleton.Instance;
            tmp.set_signal("delete");
            Frame frame = Window.Current.Content as Frame;
            frame.Navigate(typeof(MainPage), "");
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

        private async void share_Click(object sender, RoutedEventArgs e)
        {
            if (this.share.Opacity == 1)
            {
                var emailMessage = new Windows.ApplicationModel.Email.EmailMessage();
                emailMessage.Subject = "仙草";
                emailMessage.Body = "哈哈哈哈哈哈哈哈哈哈哈哈";
                StorageFolder MyFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;

                StorageFolder appInstalledFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
                StorageFolder assetsFolder = await appInstalledFolder.GetFolderAsync("Assets");
                StorageFile attachmentFile;
                var composite = ApplicationData.Current.LocalSettings.Values["newpage"] as ApplicationDataCompositeValue;
                attachmentFile = await assetsFolder.GetFileAsync("picture"+pic.DataContext+".jpg");
                if (attachmentFile != null)
                {
                    var stream = Windows.Storage.Streams.RandomAccessStreamReference.CreateFromFile(attachmentFile);
                    var attachment = new Windows.ApplicationModel.Email.EmailAttachment(
                             attachmentFile.Name,
                             stream);
                    emailMessage.Attachments.Add(attachment);
                }
                await Windows.ApplicationModel.Email.EmailManager.ShowComposeNewEmailAsync(emailMessage);
            }
        }
    }
}
