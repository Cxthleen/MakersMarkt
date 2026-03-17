using MakersMarkt.Data.Context;
using MakersMarkt.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System.Collections.Generic;
using System.Linq;

namespace MakersMarkt.Dashboards
{
    public sealed partial class SellerOrdersPage : Page
    {
        private User _currentUser;
        private AppDbContext _db = new AppDbContext();

        public SellerOrdersPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            _currentUser = e.Parameter as User;
            WelcomeText.Text = $"Welcome, {_currentUser.DisplayName}";
            LoadOrders();
        }

        private void LoadOrders()
        {
            var sellerProductIds = _db.Products
                .Where(p => p.SellerUserId == _currentUser.Id)
                .Select(p => p.Id)
                .ToList();

            var orders = _db.Orders
                .Include(o => o.Buyer)
                .Include(o => o.OrderProducts)
                    .ThenInclude(op => op.Product)
                .Where(o => o.OrderProducts.Any(op => sellerProductIds.Contains(op.ProductId)))
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            var viewModels = orders.Select(o => new SellerOrderViewModel
            {
                OrderId = o.Id,
                OrderTitle = $"Order #{o.Id}",
                BuyerName = $"Buyer: {o.Buyer?.DisplayName ?? "Unknown"}",
                OrderDate = o.OrderDate.ToString("dd MMM yyyy"),
                StatusDisplay = o.Status,
                StatusColor = GetStatusColor(o.Status),
                ProductsSummary = string.Join(", ", o.OrderProducts
                    .Where(op => sellerProductIds.Contains(op.ProductId))
                    .Select(op => $"{op.Product.Name} x{op.Quantity}")),
                Order = o
            }).ToList();

            OrdersListView.ItemsSource = viewModels;
        }

        private string GetStatusColor(string status) => status switch
        {
            "completed" => "#27ae60",
            "pending" => "#f39c12",
            "in_productie" => "#0c5adb",
            "verzonden" => "#8e44ad",
            "geweigerd" => "#e74c3c",
            "credit" => "#16a085",
            _ => "#999999"
        };

        private void Manage_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var vm = (SellerOrderViewModel)button.DataContext;
            Frame.Navigate(typeof(ManageOrderPage), (vm.Order, _currentUser));
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack) Frame.GoBack();
        }
    }

    public class SellerOrderViewModel
    {
        public int OrderId { get; set; }
        public string OrderTitle { get; set; }
        public string BuyerName { get; set; }
        public string OrderDate { get; set; }
        public string StatusDisplay { get; set; }
        public string StatusColor { get; set; }
        public string ProductsSummary { get; set; }
        public Order Order { get; set; }
    }
}