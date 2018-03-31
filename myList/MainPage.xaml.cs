using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
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

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace myList
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public struct package
    {
        public Todo processing;
        public int type;  // 0 : 普通跳转  // 1: 更新跳转
    }
    public sealed partial class MainPage : Page
    {
        public TodoManager ViewModel { get; set; }
        public Todo current;
        private BitmapImage default_image;
        public MainPage()
        {
            this.InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Enabled;    //缓存页面
            this.ViewModel = new TodoManager();
            default_image = new BitmapImage(new Uri("ms-appx:///Assets/bird.jpg"));
            ImageBrush imageBrush = new ImageBrush();
            imageBrush.ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/wood.jpg", UriKind.Absolute));   //设置背景图片
            this.main.Background = imageBrush;
            this.bar.Background = imageBrush;

            this.SizeChanged += (s, e) =>
            {
                if (e.NewSize.Width < 1200)
                {
                    this.todo_list.Width = 500 + e.NewSize.Width - 600;
                }
                else
                {
                    this.todo_list.Width = 500;
                }
            };           

        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            current = e.ClickedItem as Todo;
            if (Window.Current.Bounds.Width < 1200)
            {
                Singleton tmp = Singleton.Instance;
                tmp.set_todo(current);
                tmp.set_signal("update");
                Frame frame = Window.Current.Content as Frame;    //点击计划
                frame.Navigate(typeof(NewPage), "");
                Window.Current.Content = frame;
                Window.Current.Activate();
            }
            else
            {
                this.title.Text = current.Title;
                this.detail.Text = current.Detail;
                this.pic.Source = current.Picture;
                this.datepick.Date = DateTimeOffset.Parse(current.Date);
                this.create.Content = "Update";
                this.delete.Opacity = 1;
                
            }

        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            bool suspending = ((App)App.Current).issuspend;
            if(suspending)
            {
                ApplicationDataCompositeValue composite = new ApplicationDataCompositeValue();
                composite["title"] = title.Text;
                composite["detail"] = detail.Text;
                composite["date"] = datepick.Date;
                composite["picture"] = this.pic.DataContext;
                string result = "";
                int i = 0;
                for (i = 0; i < this.ViewModel.DefaultTodo.Count(); i++)
                    if (this.ViewModel.DefaultTodo.ElementAt(i).Is_check == true)
                        result += "1";
                    else
                        result += "0";
                composite["check"] = result;
                ApplicationData.Current.LocalSettings.Values["mainpage"] = composite;
            }
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            reset_RightPart();
            if(e.NavigationMode == NavigationMode.New)
            {
                ApplicationData.Current.LocalSettings.Values.Remove("mainpage");
            }
            else if (ApplicationData.Current.LocalSettings.Values.ContainsKey("mainpage"))
            {
                var composite = ApplicationData.Current.LocalSettings.Values["mainpage"] as ApplicationDataCompositeValue;
                title.Text = (string)composite["title"];
                detail.Text = (string)composite["detail"];
                datepick.Date = (DateTimeOffset)composite["date"];

                string result = (string)composite["check"];
                int i = 0;
                for (i = 0; i < result.Length; i++)
                    if (result[i] == '0')
                        this.ViewModel.DefaultTodo.ElementAt(i).Is_check = false;
                    else
                        this.ViewModel.DefaultTodo.ElementAt(i).Is_check = true;

                StorageFolder appInstalledFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
                string image = "tmp.jpg";
                StorageFile logoImage = await appInstalledFolder.GetFileAsync(image);

                IRandomAccessStream ir = await logoImage.OpenAsync(FileAccessMode.Read);
                BitmapImage bi = new BitmapImage();
                await bi.SetSourceAsync(ir);
                if ((string)composite["picture"] != "default")
                    pic.Source = bi;

                ApplicationData.Current.LocalSettings.Values.Remove("mainpage");

            }

            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;

            base.OnNavigatedTo(e);            
            Singleton tmp = Singleton.Instance;
            if (tmp.get_signal() == "add")
            {
                this.ViewModel.DefaultTodo.Add(current);     //0:添加项  1:修改项或者普通返回  2:删除项
            }
            else if(tmp.get_signal() == "modify_or_simple")
            {
                //do nothing
            }
            else if(tmp.get_signal() == "delete")
            {
                this.ViewModel.DefaultTodo.Remove(current);
            }
        }

        private void reset_RightPart()
        {
            title.Text = detail.Text = "";
            pic.Source = default_image;
            datepick.Date = DateTime.Now;
            this.pic.DataContext = "default";
        }

        private void Create_Button(object sender, RoutedEventArgs e)
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
            if (result == "" && this.create.Content.ToString() == "Create")              //主页面添加项
            {
                Todo newTodo = new Todo
                {
                    Title = title.Text, Detail = detail.Text, Date = datepick.Date.ToString(), Picture = pic.Source
                };
                ViewModel.DefaultTodo.Add(newTodo);
                reset_RightPart();
            }
            else if (result == "" && this.create.Content.ToString() == "Update")        //主页面更新项
            {
                current.Title = title.Text;
                current.Detail = detail.Text;
                current.Date = datepick.Date.ToString();
                current.Picture = pic.Source;

                reset_RightPart();

                this.create.Content = "Create";
                this.delete.Opacity = 0;
                
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

        private void Cancel_Button(object sender, RoutedEventArgs e)
        {
            if(this.create.Content.ToString() == "Create")
            {
                reset_RightPart();
            }
            else
            {
                this.title.Text = current.Title;
                this.detail.Text = current.Detail;
                this.pic.Source = current.Picture;
                this.datepick.Date = DateTimeOffset.Parse(current.Date);       //主页面取消项
            }
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
                // Application now has read/write access to the picked file                       图片加载
                IRandomAccessStream ir = await file.OpenAsync(FileAccessMode.Read);
                BitmapImage bi = new BitmapImage();
                await bi.SetSourceAsync(ir);
                pic.Source = bi;
                StorageFolder appInstalledFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
                StorageFile copiedFile = await file.CopyAsync(appInstalledFolder, "tmp.jpg", NameCollisionOption.ReplaceExisting);
                pic.DataContext = "selected_picture";
            }
        }

        private void Slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            this.pic.Width = sli.Value + 300;
        }

        private void delete_Click(object sender, RoutedEventArgs e)
        {
            if(this.delete.Opacity == 1)
            {
                this.ViewModel.DefaultTodo.Remove(current);
                reset_RightPart();
                this.delete.Opacity = 0;                                              //删除项
                this.create.Content = "Create";
            }
        }

        private void go_Click(object sender, RoutedEventArgs e)
        {
            if (Window.Current.Bounds.Width < 1200)
            {
                current = new Todo{};                                          
                Singleton tmp = Singleton.Instance;
                tmp.set_todo(current);
                tmp.set_signal("simple");
                
                Frame frame = Window.Current.Content as Frame;                   //页面跳转
                frame.Navigate(typeof(NewPage), "");
                Window.Current.Content = frame;
                Window.Current.Activate();
            }
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
