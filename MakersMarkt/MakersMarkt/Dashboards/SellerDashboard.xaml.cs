using MakersMarkt.Data.Context;
using MakersMarkt.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace MakersMarkt.Dashboards
{
    public sealed partial class SellerDashboard : Page
    {
        private User _currentUser;
        private AppDbContext _db = new AppDbContext();
        private List<Product> _allProducts = new();

        public SellerDashboard()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            _currentUser = e.Parameter as User;
            WelcomeText.Text = $"Welcome, {_currentUser.DisplayName}";
            (App.MainAppWindow as MainWindow)?.Header.SetLoggedIn(true);
            LoadProducts();
        }

        private void LoadProducts()
        {
            _allProducts = _db.Products
                .Include(p => p.Category)
                .Where(p => p.SellerUserId == _currentUser.Id)
                .ToList();

            // Populate category filter dynamically
            var currentSelected = (CategoryFilter.SelectedItem as ComboBoxItem)?.Content?.ToString();
            CategoryFilter.SelectionChanged -= Filter_Changed;
            CategoryFilter.Items.Clear();
            CategoryFilter.Items.Add(new ComboBoxItem { Content = "All" });
            foreach (var cat in _allProducts.Select(p => p.Category.Name).Distinct().OrderBy(n => n))
                CategoryFilter.Items.Add(new ComboBoxItem { Content = cat });

            // Restore or default to "All"
            CategoryFilter.SelectedIndex = 0;
            foreach (ComboBoxItem item in CategoryFilter.Items)
                if (item.Content.ToString() == currentSelected)
                    CategoryFilter.SelectedItem = item;

            CategoryFilter.SelectionChanged += Filter_Changed;

            ApplyFilterAndSort();
        }

        private void Filter_Changed(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilterAndSort();
        }

        private void ApplyFilterAndSort()
        {
            if (_allProducts == null || !_allProducts.Any())
                return;

            var filtered = _allProducts.AsEnumerable();

            // Filter by category
            var selectedCategory = (CategoryFilter.SelectedItem as ComboBoxItem)?.Content?.ToString();
            if (selectedCategory != null && selectedCategory != "All")
                filtered = filtered.Where(p => p.Category.Name == selectedCategory);

            // Filter by complexity
            var selectedComplexity = (ComplexityFilter.SelectedItem as ComboBoxItem)?.Content?.ToString();
            if (selectedComplexity != null && selectedComplexity != "All")
                filtered = filtered.Where(p => p.Complexity == selectedComplexity);

            // Sort
            var selectedSort = (SortBy.SelectedItem as ComboBoxItem)?.Content?.ToString();
            filtered = selectedSort switch
            {
                "Name (Z → A)" => filtered.OrderByDescending(p => p.Name),
                "Complexity" => filtered.OrderBy(p => p.Complexity),
                "Durability" => filtered.OrderBy(p => p.Durability),
                _ => filtered.OrderBy(p => p.Name)
            };

            ProductsListView.ItemsSource = filtered.ToList();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(AddProductPage), _currentUser);
        }
        private void OrdersButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SellerOrdersPage), _currentUser);
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var selectedProduct = (Product)button.DataContext;
            if (this.Frame != null)
                Frame.Navigate(typeof(EditProductPage), selectedProduct);
        }

        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var selectedProduct = (Product)button.DataContext;

            var orderCount = _db.OrderProducts.Count(op => op.ProductId == selectedProduct.Id);
            var reviewCount = _db.Reviews.Count(r => r.ProductId == selectedProduct.Id);
            var modCount = _db.Moderations.Count(m => m.ProductId == selectedProduct.Id);
            var reportCount = _db.Reports.Count(r => r.ProductId == selectedProduct.Id);
            var totalLinks = orderCount + reviewCount + modCount + reportCount;

            ContentDialog dialog;

            if (totalLinks > 0)
            {
                var lines = "";
                if (orderCount > 0) lines += $"\n• {orderCount} order line(s)";
                if (reviewCount > 0) lines += $"\n• {reviewCount} review(s)";
                if (modCount > 0) lines += $"\n• {modCount} moderation(s)";
                if (reportCount > 0) lines += $"\n• {reportCount} report(s)";

                dialog = new ContentDialog
                {
                    Title = "⚠️ Linked records found",
                    Content = $"\"{selectedProduct.Name}\" is linked to:{lines}\n\nDeleting this product will also remove all of the above. Are you sure?",
                    CloseButtonText = "Cancel",
                    PrimaryButtonText = "Delete everything",
                    DefaultButton = ContentDialogButton.Close,
                    XamlRoot = this.XamlRoot ?? button.XamlRoot
                };
            }
            else
            {
                dialog = new ContentDialog
                {
                    Title = "Delete Product",
                    Content = $"Are you sure you want to delete \"{selectedProduct.Name}\"?",
                    CloseButtonText = "Cancel",
                    PrimaryButtonText = "Delete",
                    DefaultButton = ContentDialogButton.Close,
                    XamlRoot = this.XamlRoot ?? button.XamlRoot
                };
            }

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                _db.Reports.RemoveRange(_db.Reports.Where(r => r.ProductId == selectedProduct.Id));
                _db.Moderations.RemoveRange(_db.Moderations.Where(m => m.ProductId == selectedProduct.Id));
                _db.Reviews.RemoveRange(_db.Reviews.Where(r => r.ProductId == selectedProduct.Id));
                _db.OrderProducts.RemoveRange(_db.OrderProducts.Where(op => op.ProductId == selectedProduct.Id));

                var toDelete = _db.Products.Find(selectedProduct.Id);
                if (toDelete != null)
                    _db.Products.Remove(toDelete);

                _db.SaveChanges();
                LoadProducts();
            }
        }
    }
}