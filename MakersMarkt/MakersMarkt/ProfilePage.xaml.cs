using MakersMarkt.Data.Context;
using MakersMarkt.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MakersMarkt
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ProfilePage : Page
    {
        public User CurrentUser { get; set; }

        public ProfilePage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var user = e.Parameter as User;

            using var db = new AppDbContext();

            CurrentUser = db.Users.Include(u => u.Notifications).FirstOrDefault(u => u.Id == user.Id);

            DataContext = CurrentUser;
        }

        private async void ChangeProfilePicture_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");

            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainAppWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            var file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                CurrentUser.ProfileImage = file.Path;

                DataContext = null;
                DataContext = CurrentUser;

                (App.MainAppWindow as MainWindow)?.Header.SetUser(CurrentUser);

                using var db = new AppDbContext();
                db.Users.Update(CurrentUser);
                db.SaveChanges();
            }
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            ProfileTitle.Text = CurrentUser.DisplayName;

            try
            {
                using var db = new AppDbContext();

                db.Users.Update(CurrentUser);
                await db.SaveChangesAsync();

                (App.MainAppWindow as MainWindow)?.Header.SetUser(CurrentUser);

                ContentDialog dialog = new ContentDialog
                {
                    Title = "Profile Updated",
                    Content = "Your profile has been saved successfully.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot,
                    Style = (Style)Application.Current.Resources["PastelDialogStyle"]
                };

                await dialog.ShowAsync();
            }
            catch (Exception ex)
            {
                ContentDialog errorDialog = new ContentDialog
                {
                    Title = "Error",
                    Content = $"Something went wrong: {ex.Message}",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };

                await errorDialog.ShowAsync();
            }
        }

        private void DisplayName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (CurrentUser != null)
            {
                ProfileTitle.Text = CurrentUser.DisplayName;
            }
        }
        private async void OpenNotifications_Click(object sender, RoutedEventArgs e)
        {
            await NotificationsDialog.ShowAsync();
        }
        private async void Notification_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var notification = (sender as FrameworkElement).DataContext as Notification;

            if (notification != null && !notification.IsRead)
            {
                using var db = new AppDbContext();
                notification.IsRead = true;
                db.Notifications.Update(notification);
                await db.SaveChangesAsync();

                DataContext = null;
                DataContext = CurrentUser;
            }
        }

        private async void MarkAllAsRead_Click(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            using var db = new AppDbContext();

            foreach (var n in CurrentUser.Notifications)
            {
                n.IsRead = true;
                db.Notifications.Update(n);
            }

            await db.SaveChangesAsync();

            DataContext = null;
            DataContext = CurrentUser;
        }
        private void OpenOrders_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(OrdersPage), CurrentUser);
        }

        private void OpenReviews_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(ReviewsPage), CurrentUser);
        }
    }
    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool isRead = (bool)value;
            return isRead
                ? new SolidColorBrush(Colors.Transparent)
                : new SolidColorBrush(Colors.CornflowerBlue);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }
    public class DateTimeToPrettyStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is DateTime dt)
            {
                var now = DateTime.Now;
                var diff = now - dt;

                if (diff.TotalSeconds < 60)
                    return "Just now";

                if (diff.TotalMinutes < 60)
                    return $"{(int)diff.TotalMinutes} minutes ago";

                if (diff.TotalHours < 24)
                    return $"{(int)diff.TotalHours} hours ago";

                if (diff.TotalDays < 2)
                    return "Yesterday";

                if (diff.TotalDays < 7)
                    return $"{(int)diff.TotalDays} days ago";

                return dt.ToString("MMM dd HH:mm");
            }

            return "";
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }
    public class BoolToFontWeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool isRead = (bool)value;
            return isRead ? FontWeights.Normal : FontWeights.Bold;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }
}
