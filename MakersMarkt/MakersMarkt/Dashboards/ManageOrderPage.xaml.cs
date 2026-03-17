using MakersMarkt.Data.Context;
using MakersMarkt.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Linq;

namespace MakersMarkt.Dashboards
{
    public sealed partial class ManageOrderPage : Page
    {
        private Order _order;
        private User _currentUser;
        private AppDbContext _db = new AppDbContext();
        private string _selectedStatus;

        public ManageOrderPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is (Order order, User user))
            {
                _currentUser = user;
                _order = _db.Orders
                    .Include(o => o.Buyer)
                    .Include(o => o.OrderProducts)
                        .ThenInclude(op => op.Product)
                    .Include(o => o.Transactions)
                    .FirstOrDefault(o => o.Id == order.Id);

                LoadOrderDetails();
            }
        }

        private void LoadOrderDetails()
        {
            if (_order == null) return;

            OrderTitle.Text = $"Order #{_order.Id}";
            BuyerName.Text = $"Buyer: {_order.Buyer?.DisplayName ?? "Unknown"}";
            OrderDate.Text = $"Ordered on: {_order.OrderDate:dd MMM yyyy}";
            CurrentStatus.Text = _order.Status;

            var sellerProductIds = _db.Products
                .Where(p => p.SellerUserId == _currentUser.Id)
                .Select(p => p.Id)
                .ToList();

            var productLines = _order.OrderProducts
                .Where(op => sellerProductIds.Contains(op.ProductId))
                .Select(op => $"• {op.Product.Name}  x{op.Quantity}")
                .ToList();

            ProductsListView.ItemsSource = productLines;
        }

        private void Status_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem item)
            {
                _selectedStatus = item.Text;
                StatusDropDown.Content = item.Text;
                RefundWarning.Visibility = _selectedStatus == "geweigerd"
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedStatus == null)
            {
                ShowFeedback("Please select a status.", isError: true);
                return;
            }

            if (string.IsNullOrWhiteSpace(DescriptionTextBox.Text))
            {
                ShowFeedback("Please add a description for the buyer.", isError: true);
                return;
            }

            SaveButton.IsEnabled = false;

            try
            {
                var orderToUpdate = _db.Orders
                    .Include(o => o.Transactions)
                    .FirstOrDefault(o => o.Id == _order.Id);

                if (orderToUpdate == null) return;

                orderToUpdate.Status = _selectedStatus;

                // Notify buyer of status change
                _db.Notifications.Add(new Notification
                {
                    Type = $"order_{_selectedStatus}",
                    IsRead = false,
                    UserId = _order.BuyerUserId
                });

                // If geweigerd — create refund transaction
                if (_selectedStatus == "geweigerd")
                {
                    var originalAmount = orderToUpdate.Transactions
                        .Where(t => t.Type == "payment")
                        .Sum(t => t.Amount);

                    _db.Transactions.Add(new Transaction
                    {
                        Amount = originalAmount > 0 ? originalAmount : 0,
                        Type = "refund",
                        TransactionDate = DateTime.Now,
                        OrderId = _order.Id
                    });

                    // Extra notification for refund
                    _db.Notifications.Add(new Notification
                    {
                        Type = "refund_issued",
                        IsRead = false,
                        UserId = _order.BuyerUserId
                    });
                }

                _db.SaveChanges();

                CurrentStatus.Text = _selectedStatus;
                ShowFeedback("✓ Status updated successfully!", isError: false);

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
                ShowFeedback($"Something went wrong: {ex.Message}", isError: true);
                SaveButton.IsEnabled = true;
            }
        }

        private void ShowFeedback(string message, bool isError)
        {
            FeedbackText.Text = message;
            FeedbackText.Foreground = isError
                ? new SolidColorBrush(Windows.UI.Color.FromArgb(255, 231, 76, 60))
                : new SolidColorBrush(Windows.UI.Color.FromArgb(255, 39, 174, 96));
            FeedbackText.Visibility = Visibility.Visible;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack) Frame.GoBack();
        }
    }
}