using MakersMarkt.Data.Context;
using MakersMarkt.Data.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MakersMarkt.Dashboards
{
    public sealed partial class AddCreditPage : Page
    {
        private User _currentAdmin;
        private AppDbContext _db = new AppDbContext();
        private List<User> _buyers = new();

        public AddCreditPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            _currentAdmin = e.Parameter as User;
            LoadBuyers();
        }

        private void LoadBuyers()
        {
            _buyers = _db.Users
                .Where(u => u.Role == "buyer")
                .OrderBy(u => u.DisplayName)
                .ToList();

            foreach (var buyer in _buyers)
                BuyerDropDown.Items.Add(new ComboBoxItem
                {
                    Content = $"{buyer.DisplayName} (@{buyer.Username})",
                    Tag = buyer.Id
                });
        }

        private void BuyerDropDown_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BuyerDropDown.SelectedItem is ComboBoxItem item && item.Tag is int buyerId)
            {
                var buyer = _buyers.FirstOrDefault(b => b.Id == buyerId);
                if (buyer != null)
                {
                    BuyerDisplayName.Text = buyer.DisplayName;
                    BuyerUsername.Text = $"@{buyer.Username}";
                    BuyerInfoPanel.Visibility = Visibility.Visible;
                }
            }
            else
            {
                BuyerInfoPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Validate buyer
            if (BuyerDropDown.SelectedItem is not ComboBoxItem selectedItem || selectedItem.Tag is not int buyerId)
            {
                ShowFeedback("Selecteer eerst een koper.", isError: true);
                return;
            }

            // Validate amount
            if (!decimal.TryParse(AmountTextBox.Text.Replace(",", "."),
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out decimal amount) || amount <= 0)
            {
                ShowFeedback("Voer een geldig bedrag in (groter dan 0).", isError: true);
                return;
            }

            SaveButton.IsEnabled = false;

            try
            {
                // Create a credit order for the buyer
                var order = new Order
                {
                    Status = "credit",
                    OrderDate = DateTime.Now,
                    BuyerUserId = buyerId
                };
                _db.Orders.Add(order);
                _db.SaveChanges();

                // Save transaction as credit type
                var transaction = new Transaction
                {
                    Amount = amount,
                    Type = "credit",
                    TransactionDate = DateTime.Now,
                    OrderId = order.Id
                };
                _db.Transactions.Add(transaction);

                // Notify the buyer
                var notification = new Notification
                {
                    Type = "credit_added",
                    IsRead = false,
                    UserId = buyerId
                };
                _db.Notifications.Add(notification);

                _db.SaveChanges();

                ShowFeedback($"✓ €{amount:F2} krediet toegevoegd!", isError: false);

                var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1.5) };
                timer.Tick += (s, args) =>
                {
                    timer.Stop();
                    if (Frame.CanGoBack) Frame.GoBack();
                };
                timer.Start();
            }
            catch (Exception ex)
            {
                ShowFeedback($"Fout: {ex.Message}", isError: true);
                SaveButton.IsEnabled = true;
            }
        }

        private void ShowFeedback(string message, bool isError)
        {
            FeedbackText.Text = message;
            FeedbackText.Foreground = isError
                ? new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 59, 48))
                : new SolidColorBrush(Windows.UI.Color.FromArgb(255, 52, 199, 89));
            FeedbackText.Visibility = Visibility.Visible;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack) Frame.GoBack();
        }
    }
}