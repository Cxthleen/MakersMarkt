using MakersMarkt.Data.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.NetworkOperators;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MakersMarkt
{
    public sealed partial class AppHeader : UserControl
    {
        public User CurrentUser { get; set; }
        public AppHeader()
        {
            InitializeComponent();
        }

        public void SetLoggedIn(bool isLoggedIn)
        {
            ProfileButton.Visibility = isLoggedIn ? Visibility.Visible : Visibility.Collapsed;
            HomeButton.Visibility = isLoggedIn ? Visibility.Visible : Visibility.Collapsed;
        }
        private void Home_Click(object sender, RoutedEventArgs e)
        {
            (App.MainAppWindow as MainWindow)?.NavigateToHome();
        }

        private void ViewProfile_Click(object sender, RoutedEventArgs e)
        {
            (App.MainAppWindow as MainWindow)?.NavigateToProfile();
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            (App.MainAppWindow as MainWindow)?.NavigateToLogin();
            SetLoggedIn(false);
        }

        public void SetUser(User user)
        {
            CurrentUser = user;
            DataContext = CurrentUser;

            ProfileButton.Visibility = Visibility.Visible;
            HomeButton.Visibility = Visibility.Visible;

            if (!string.IsNullOrEmpty(user.ProfileImage))
            {
                ProfileImageBrush.ImageSource = new BitmapImage(new Uri(user.ProfileImage));
            }
        }
    }

}
