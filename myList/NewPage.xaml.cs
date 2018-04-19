using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI;
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
        public TodoManager ViewModel { get; set; }
        private BitmapImage default_image;
        private Singleton m_singleton;
        private Database m_db;
        public NewPage()
        {
            this.InitializeComponent();
            m_singleton = Singleton.Instance;
            this.ViewModel = m_singleton.ViewModel;
            NavigationCacheMode = NavigationCacheMode.Enabled;    //缓存页面           
            default_image = new BitmapImage(new Uri("ms-appx:///Assets/picture0.jpg"));
            Frame rootFrame = Window.Current.Content as Frame;
            //rootFrame.Navigated += OnNavigated;
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;
            m_db = Database.Instance;
            this.SizeChanged += (s, e) =>
            {
                this.create_part.Height = 800 + e.NewSize.Height - 970;
            };
            DataTransferManager.GetForCurrentView().DataRequested += OnShare;
        }

        public async void OnShare(DataTransferManager sender, DataRequestedEventArgs args)
        {
            var request = args.Request;
            request.Data.Properties.Title = "仙草";
            request.Data.Properties.Description = "数据共享功能";
            request.Data.SetText("哈哈哈哈哈哈哈哈哈哈");
            StorageFolder MyFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;

            StorageFolder appInstalledFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            StorageFolder assetsFolder = await appInstalledFolder.GetFolderAsync("Assets");
            StorageFile attachmentFile = null;
            attachmentFile = await assetsFolder.GetFileAsync("picture" + pic.DataContext + ".jpg");
            if (attachmentFile != null)
            {
                request.Data.SetStorageItems(new List<StorageFile> { attachmentFile });
            }
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
                if (current != null)
                    composite["create_date"] = current.create_date;
                Singleton tmp = Singleton.Instance;
                composite["signal"] = tmp.get_signal();
                ApplicationData.Current.LocalSettings.Values["newpage"] = composite;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            string background = m_db.get_background();
            ImageBrush imageBrush = new ImageBrush();
            imageBrush.ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/" + background, UriKind.Absolute));   //设置背景图片
            this.main.Background = imageBrush;
            var color = (Color)this.Resources["SystemAccentColor"];  //跟随系统主题颜色
            this.bar.Background = new SolidColorBrush(color);
            this.set_color.Foreground = m_singleton.get_color();
            if (e.NavigationMode == NavigationMode.New)
            {
                ApplicationData.Current.LocalSettings.Values.Remove("newpage");
                base.OnNavigatedTo(e);
                current = m_singleton.get_todo();

                if (m_singleton.get_signal() == "simple")
                {
                    this.Create.Content = "Create";
                    SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
                    this.delete.Opacity = 0;
                    this.share.Opacity = 0;
                    this.pic.DataContext = "0";
                    save_to_tmp("picture0.jpg");
                }
                else if (m_singleton.get_signal() == "update")
                {
                    SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
                    this.Create.Content = "Update";
                    this.title.Text = current.Title;
                    this.detail.Text = current.Detail;
                    this.pic.DataContext = current.picture_id;
                    pic.Source = current.Picture;
                    this.datepick.Date = DateTimeOffset.Parse(current.Date);
                    this.delete.Opacity = 1;
                    this.share.Opacity = 1;
                }

            }
            else if (ApplicationData.Current.LocalSettings.Values.ContainsKey("newpage"))
            {
                var composite = ApplicationData.Current.LocalSettings.Values["newpage"] as ApplicationDataCompositeValue;
                if((string)composite["signal"] == "update")
                {
                    this.share.Opacity = 1;
                    this.delete.Opacity = 1;
                    this.Create.Content = "Update";
                    foreach (var item in this.ViewModel.DefaultTodo)
                    {
                        if (item.create_date == (string)composite["create_date"])
                        {
                            current = item;
                            break;
                        }
                    }
                    this.pic.DataContext = current.picture_id;
                    m_singleton.set_signal("update");
                    m_singleton.set_todo(current);
                    SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
                }
                else
                    SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
                title.Text = (string)composite["title"];
                detail.Text = (string)composite["detail"];
                datepick.Date = (DateTimeOffset)composite["date"];
                this.delete.Opacity = 1;
                this.share.Opacity = 1;
                this.Create.Content = "Update";
                get_cache_image();
                ApplicationData.Current.LocalSettings.Values.Remove("newpage");

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
                StorageFile copiedFile = await file.CopyAsync(assetsFolder, "tmp.jpg", NameCollisionOption.ReplaceExisting);
            }
        }

        private void reset_RightPart()
        {
            title.Text = detail.Text = "";
            pic.Source = default_image;
            datepick.Date = DateTime.Now;
            save_to_tmp("picture0.jpg");
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
                save_to_tmp("picture" + current.picture_id + ".jpg");
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
                current.picture_id = m_singleton.get_picture_count();
                tmp_to_save("picture"+current.picture_id+".jpg");
                current.create_date = DateTime.Now.ToString();
                reset_RightPart();
                tmp.set_signal("add");
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

                this.Create.Content = "Create";
                this.delete.Opacity = 0;

                tmp.set_signal("modify_or_simple");
                m_db.update(current);
                tmp_to_save("picture" + current.picture_id + ".jpg");
                reset_RightPart();
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

        private void share_Click(object sender, RoutedEventArgs e)
        {
            if (this.share.Opacity == 1)
            {
                DataTransferManager.ShowShareUI();
            }
        }

        private async void get_cache_image()
        {
            StorageFolder appInstalledFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            string image = @"Assets\tmp.jpg";
            StorageFile logoImage = await appInstalledFolder.GetFileAsync(image);

            IRandomAccessStream ir = await logoImage.OpenAsync(FileAccessMode.Read);
            BitmapImage bi = new BitmapImage();
            await bi.SetSourceAsync(ir);
            pic.Source = bi;
        }

        private async void save_to_tmp(string save_name)
        {
            StorageFolder MyFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;

            StorageFolder appInstalledFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            StorageFolder assetsFolder = await appInstalledFolder.GetFolderAsync("Assets");
            StorageFile attachmentFile;
            attachmentFile = await assetsFolder.GetFileAsync(save_name);
            if (attachmentFile != null)
            {
                StorageFile copiedFile = await attachmentFile.CopyAsync(assetsFolder, "tmp.jpg", NameCollisionOption.ReplaceExisting);
            }
        }

        private async void tmp_to_save(string save_name)
        {
            StorageFolder MyFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;

            StorageFolder appInstalledFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            StorageFolder assetsFolder = await appInstalledFolder.GetFolderAsync("Assets");
            StorageFile attachmentFile;
            attachmentFile = await assetsFolder.GetFileAsync("tmp.jpg");
            if (attachmentFile != null)
            {
                StorageFile copiedFile = await attachmentFile.CopyAsync(assetsFolder, save_name, NameCollisionOption.ReplaceExisting);
            }
        }
    }
}
