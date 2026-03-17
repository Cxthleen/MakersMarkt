using MakersMarkt.Data.Context;
using MakersMarkt.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Linq;

namespace MakersMarkt.Dashboards
{
    public sealed partial class ProductDetailPage : Page
    {
        private Product _product;
        private User _currentUser;
        private AppDbContext _db = new AppDbContext();

        public ProductDetailPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is (Product product, User user))
            {
                _product = product;
                _currentUser = user;
                LoadProduct();
            }
        }

        private void LoadProduct()
        {
            // Load full product with all relations
            _product = _db.Products
                .Include(p => p.Category)
                .Include(p => p.Seller)
                .Include(p => p.Reviews)
                    .ThenInclude(r => r.Buyer)
                .FirstOrDefault(p => p.Id == _product.Id);

            if (_product == null) return;

            // Header
            ProductName.Text = _product.Name;
            CategoryName.Text = _product.Category.Name;
            SellerName.Text = $"By {_product.Seller.DisplayName}";

            // Description
            ProductDescription.Text = _product.Description;

            // Specs
            MaterialText.Text = _product.MaterialUsage;
            ProductionTimeText.Text = _product.ProductionTime;
            ComplexityText.Text = _product.Complexity;
            DurabilityText.Text = _product.Durability;

            // Reviews
            var reviews = _product.Reviews.ToList();
            if (reviews.Any())
            {
                var avg = reviews.Average(r => r.Rating);
                AverageRating.Text = $"★ {avg:F1}";
                ReviewCount.Text = $"({reviews.Count} review{(reviews.Count > 1 ? "s" : "")})";
                ReviewsListView.ItemsSource = reviews;
                NoReviewsText.Visibility = Visibility.Collapsed;
            }
            else
            {
                AverageRating.Text = "★ —";
                ReviewCount.Text = "(0 reviews)";
                NoReviewsText.Visibility = Visibility.Visible;
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
                Frame.GoBack();
        }

        private async void OrderButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ContentDialog
            {
                Title = "Place Order",
                Content = $"Are you sure you want to order \"{_product.Name}\"?",
                PrimaryButtonText = "Order",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();
            if (result != ContentDialogResult.Primary) return;

            try
            {
                // Create order
                var order = new Order
                {
                    Status = "pending",
                    OrderDate = DateTime.Now,
                    BuyerUserId = _currentUser.Id
                };
                _db.Orders.Add(order);
                _db.SaveChanges();

                // Link product to order
                var orderProduct = new OrderProduct
                {
                    OrderId = order.Id,
                    ProductId = _product.Id,
                    Quantity = 1
                };
                _db.OrderProducts.Add(orderProduct);

                // Create transaction
                var transaction = new Transaction
                {
                    Amount = 0,
                    Type = "payment",
                    TransactionDate = DateTime.Now,
                    OrderId = order.Id
                };
                _db.Transactions.Add(transaction);

                // Notify the seller
                var notification = new Notification
                {
                    Type = "new_order",
                    IsRead = false,
                    UserId = _product.SellerUserId
                };
                _db.Notifications.Add(notification);

                _db.SaveChanges();

                OrderButton.IsEnabled = false;
                OrderFeedback.Text = "✓ Order placed successfully!";
                OrderFeedback.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                var errorDialog = new ContentDialog
                {
                    Title = "Error",
                    Content = $"Something went wrong: {ex.Message}",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await errorDialog.ShowAsync();
            }
        }
    }
}