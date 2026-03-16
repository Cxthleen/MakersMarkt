using MakersMarkt.Data.Context;
using MakersMarkt.Data.Models;
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

            CurrentUser = e.Parameter as User;

            if (CurrentUser == null)
                throw new Exception("ProfilePage requires a User object.");

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
    }
}
