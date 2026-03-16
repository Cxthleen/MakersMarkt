using MakersMarkt.Dashboards;
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
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public AppHeader Header => HeaderControl;
        public User LoggedInUser { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            RootFrame.Navigate(typeof(MakersMarkt.LoginPage));
        }

        public void NavigateToLogin()
        {
            RootFrame.Navigate(typeof(LoginPage));
        }

        public void NavigateToProfile()
        {
            RootFrame.Navigate(typeof(ProfilePage), LoggedInUser);
        }

        public void NavigateToHome()
        {
            if (LoggedInUser == null)
                return;

            switch (LoggedInUser.Role)
            {
                case "admin":
                    RootFrame.Navigate(typeof(AdminDashboard), LoggedInUser);
                    break;

                case "seller":
                    RootFrame.Navigate(typeof(SellerDashboard), LoggedInUser);
                    break;

                case "buyer":
                    RootFrame.Navigate(typeof(BuyerDashboard), LoggedInUser);
                    break;
            }
        }
    }
}
