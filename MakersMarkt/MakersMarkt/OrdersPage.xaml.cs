using MakersMarkt.Data.Context;
using MakersMarkt.Data.Models;
using Microsoft.EntityFrameworkCore;
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
    public sealed partial class OrdersPage : Page
    {
        private User _currentUser;

        public class OrderDisplay
        {
            public int OrderId { get; set; }
            public string Status { get; set; }
            public string DateFormatted { get; set; }
            public string ProductCount { get; set; }
            public bool HasReviewed { get; set; }
            public Visibility CanReview => (Status == "verzonden" || Status == "completed") && !HasReviewed
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        public OrdersPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            _currentUser = e.Parameter as User;
            LoadOrders();
        }

        private void LoadOrders()
        {
            using var db = new AppDbContext();
            var orders = db.Orders
                .Include(o => o.OrderProducts)
                .Where(o => o.BuyerUserId == _currentUser.Id)
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            var reviewedProductIds = db.Reviews
                .Where(r => r.BuyerUserId == _currentUser.Id)
                .Select(r => r.ProductId)
                .ToHashSet();

            var displayOrders = orders.Select(o => new OrderDisplay
            {
                OrderId = o.Id,
                Status = o.Status,
                DateFormatted = $"Ordered on: {o.OrderDate:dd MMM yyyy}",
                ProductCount = $"Products: {o.OrderProducts?.Count ?? 0}",
                HasReviewed = o.OrderProducts?.Any(op => reviewedProductIds.Contains(op.ProductId)) ?? false
            }).ToList();

            OrdersList.ItemsSource = displayOrders;
        }

        private async void WriteReview_Click(object sender, RoutedEventArgs e)
        {
            var order = (sender as FrameworkElement).DataContext as OrderDisplay;

            var dialog = new ContentDialog
            {
                Title = "Write a Review",
                PrimaryButtonText = "Submit",
                CloseButtonText = "Cancel",
                XamlRoot = this.XamlRoot
            };

            var panel = new StackPanel { Spacing = 10 };

            var stars = new RatingControl { MaxRating = 5, Value = 0 };
            var text = new TextBox { AcceptsReturn = true, Height = 80, PlaceholderText = "Write your review..." };

            panel.Children.Add(stars);
            panel.Children.Add(text);

            dialog.Content = panel;

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                using var db = new AppDbContext();

                var productId = db.OrderProducts
                    .Where(op => op.OrderId == order.OrderId)
                    .Select(op => op.ProductId)
                    .FirstOrDefault();

                var review = new Review
                {
                    Rating = (int)stars.Value,
                    ReviewText = text.Text,
                    BuyerUserId = _currentUser.Id,
                    ProductId = productId
                };

                db.Reviews.Add(review);
                db.SaveChanges();
                LoadOrders();
            }
        }
    }
}
