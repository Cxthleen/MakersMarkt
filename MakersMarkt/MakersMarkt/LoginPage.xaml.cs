using MakersMarkt.Dashboards;
using MakersMarkt.Data.Context;
using MakersMarkt.Data.Models;
using Microsoft.UI;
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
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MakersMarkt
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private async void Login_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(UsernameBox.Text) ||
                string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                await ShowMessage("Oops! You forgot something.");
                return;
            }
                (App.MainAppWindow as MainWindow)?.Header.SetLoggedIn(false);
            string username = UsernameBox.Text;
            string password = PasswordBox.Password;

            using var db = new AppDbContext();

            var user = db.Users.FirstOrDefault(u => u.Username == username);

            if (user != null && BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                (App.MainAppWindow as MainWindow).LoggedInUser = user;
                (App.MainAppWindow as MainWindow).Header.SetUser(user);

                await ShowMessage($"Welcome {user.Role} !");

                switch (user.Role)
                {
                    case "admin":
                        Frame.Navigate(typeof(AdminDashboard), user);
                        break;

                    case "seller":
                        Frame.Navigate(typeof(SellerDashboard), user);
                        break;

                    case "buyer":
                        Frame.Navigate(typeof(BuyerDashboard), user);
                        break;

                    default:
                        Frame.Navigate(typeof(LoginPage));
                        break;
                }
            }
            else
            {
                await ShowMessage("Hmm… that doesn’t look right.");
            }
        }
        private async void Register_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog registerDialog = new()
            {
                Title = "",
                PrimaryButtonText = "Register",
                CloseButtonText = "Cancel",
                XamlRoot = this.XamlRoot,
                RequestedTheme = ElementTheme.Light,
                Style = (Style)Application.Current.Resources["PastelDialogStyle"]
            };

            registerDialog.PrimaryButtonStyle = (Style)Application.Current.Resources["PastelButtonStyle"];
            registerDialog.CloseButtonStyle = (Style)Application.Current.Resources["PastelButtonStyle"];

            StackPanel layout = new()
            {
                Padding = new Thickness(20),
                Spacing = 10
            };

            TextBlock title = new()
            {
                Text = "Create Account",
                FontSize = 36,
                FontFamily = new FontFamily("Poppins"),
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20),
                Foreground = new SolidColorBrush(Color.FromArgb(255, 58, 47, 40))
            };
            layout.Children.Add(title);

            Grid userGrid = MakeInput("Username", "\uE77B", out TextBox usernameBox);
            layout.Children.Add(userGrid);

            Grid passGrid = MakePassword("Password", "\uE72E", out PasswordBox passwordBox);
            layout.Children.Add(passGrid);

            Grid confirmGrid = MakePassword("Confirm Password", "\uE72E", out PasswordBox confirmBox);
            layout.Children.Add(confirmGrid);

            Grid roleRow = new()
            {
                Height = 45,
                Margin = new Thickness(0, 10, 0, 0)
            };

            roleRow.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            roleRow.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

            FontIcon roleIcon = new()
            {
                Glyph = "\uE77B",
                FontSize = 24,
                Foreground = new SolidColorBrush(Color.FromArgb(255, 66, 53, 44)),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(0, 0, 8, 0)
            };
            Grid.SetColumn(roleIcon, 0);
            roleRow.Children.Add(roleIcon);

            ComboBox roleBox = new()
            {
                ItemsSource = new List<string> { "buyer", "seller" },
                SelectedIndex = 0,
                Style = (Style)Application.Current.Resources["PastelComboBox"],
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                MinWidth = 150
            };
            Grid.SetColumn(roleBox, 1);
            roleRow.Children.Add(roleBox);

            layout.Children.Add(roleRow);

            registerDialog.Content = layout;

            var result = await registerDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                string username = usernameBox.Text;
                string password = passwordBox.Password;
                string confirm = confirmBox.Password;
                string role = roleBox.SelectedItem.ToString();

                if (password != confirm)
                {
                    await ShowMessage("Nope! Try checking your password again.");
                    return;
                }

                using var db = new AppDbContext();

                if (db.Users.Any(u => u.Username == username))
                {
                    await ShowMessage("Oops! That name’s already in use.");
                    return;
                }

                string hashed = BCrypt.Net.BCrypt.HashPassword(password);

                var newUser = new User
                {
                    Username = username,
                    Password = hashed,
                    Role = role,
                    DisplayName = username,
                    Biography = ""
                };

                db.Users.Add(newUser);
                db.SaveChanges();

                await ShowMessage("Account created!");
            }
        }

        private Grid MakeInput(string placeholder, string icon, out TextBox box)
        {
            Grid g = new() { Height = 45, Margin = new Thickness(0, 5, 0, 5) };

            box = new TextBox()
            {
                PlaceholderText = placeholder,
                Height = 45,
                Background = (Brush)Resources["TextControlBackground"],
                BorderBrush = (Brush)Resources["TextControlBackground"],
                Foreground = new SolidColorBrush(Color.FromArgb(255, 66, 53, 44)),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(40, 11, 12, 0),
                VerticalContentAlignment = VerticalAlignment.Center
            };

            FontIcon ic = new()
            {
                Glyph = icon,
                FontSize = 20,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(12, 0, 0, 0),
                Foreground = new SolidColorBrush(Color.FromArgb(255, 66, 53, 44)),
                IsHitTestVisible = false
            };

            g.Children.Add(box);
            g.Children.Add(ic);

            return g;
        }

        private Grid MakePassword(string placeholder, string icon, out PasswordBox box)
        {
            Grid g = new() { Height = 45, Margin = new Thickness(0, 5, 0, 5) };

            box = new PasswordBox()
            {
                PlaceholderText = placeholder,
                Height = 45,
                Background = (Brush)Application.Current.Resources["PasswordBoxBackground"],
                BorderBrush = (Brush)Application.Current.Resources["PasswordBoxBackground"],
                Foreground = new SolidColorBrush(Color.FromArgb(255, 66, 53, 44)),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(40, 11, 12, 0),
                VerticalContentAlignment = VerticalAlignment.Center
            };

            FontIcon ic = new()
            {
                Glyph = icon,
                FontSize = 20,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(12, 0, 0, 0),
                Foreground = new SolidColorBrush(Color.FromArgb(255, 66, 53, 44)),
                IsHitTestVisible = false
            };

            g.Children.Add(box);
            g.Children.Add(ic);

            return g;
        }

        private async Task ShowMessage(string message)
        {
            ContentDialog dialog = new()
            {
                XamlRoot = this.XamlRoot,
                Background = new SolidColorBrush(ColorHelper.FromArgb(255, 255, 255, 255)),
                CornerRadius = new CornerRadius(25),
                Title = null
            };

            StackPanel panel = new()
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Spacing = 16,
                Margin = new Thickness(20)
            };

            StackPanel messageRow = new()
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Spacing = 10
            };

            string iconPath = "ms-appx:///Assets/bibblepoo.jpg";

            if (message.Contains("forgot", StringComparison.OrdinalIgnoreCase))
                iconPath = "ms-appx:///Assets/bibble.jpg";

            else if (message.Contains("weird", StringComparison.OrdinalIgnoreCase))
                iconPath = "ms-appx:///Assets/Bibbleshock.jpg";

            else if (message.Contains("Nope", StringComparison.OrdinalIgnoreCase))
                iconPath = "ms-appx:///Assets/bibble eyeroll.jpg";

            else if (message.Contains("doesn't", StringComparison.OrdinalIgnoreCase))
                iconPath = "ms-appx:///Assets/bibblepoo.jpg";

            else if (message.Contains("already", StringComparison.OrdinalIgnoreCase))
                iconPath = "ms-appx:///Assets/bibbleno.jpg";

            else if (message.Contains("created", StringComparison.OrdinalIgnoreCase))
                iconPath = "ms-appx:///Assets/bibbleyay.jpg";

            Image icon = new()
            {
                Source = new BitmapImage(new Uri(iconPath)),
                Width = 50,
                Height = 50,
                VerticalAlignment = VerticalAlignment.Center
            };

            TextBlock text = new()
            {
                Text = message,
                FontSize = 22,
                FontFamily = new FontFamily("Poppins"),
                Foreground = new SolidColorBrush(Color.FromArgb(255, 44, 35, 32)),
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            messageRow.Children.Add(icon);
            messageRow.Children.Add(text);

            Button okButton = new()
            {
                Content = "Okey",
                HorizontalAlignment = HorizontalAlignment.Center,
                Style = (Style)Application.Current.Resources["PastelButtonStyle"]
            };
            okButton.Click += (_, __) => dialog.Hide();

            panel.Children.Add(messageRow);
            panel.Children.Add(okButton);

            dialog.Content = panel;

            await dialog.ShowAsync();
        }
    }
}
