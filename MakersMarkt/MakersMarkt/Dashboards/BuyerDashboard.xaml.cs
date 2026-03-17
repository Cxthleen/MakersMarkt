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
    public sealed partial class BuyerDashboard : Page
    {
        private User _currentUser;
        private AppDbContext _db = new AppDbContext();
        private List<Product> _allProducts = new();

        public BuyerDashboard()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            _currentUser = e.Parameter as User;
            base.OnNavigatedTo(e);

            WelcomeText.Text = $"Welcome, {_currentUser.DisplayName}";
            (App.MainAppWindow as MainWindow)?.Header.SetLoggedIn(true);

            LoadProducts();
        }

        private void LoadProducts()
        {
            _allProducts = _db.Products
                .Include(p => p.Category)
                .Include(p => p.Seller)
                .ToList();

            // Populate category filter dynamically
            CategoryFilter.SelectionChanged -= Filter_Changed;
            CategoryFilter.Items.Clear();
            CategoryFilter.Items.Add(new ComboBoxItem { Content = "All" });
            foreach (var cat in _allProducts.Select(p => p.Category.Name).Distinct().OrderBy(n => n))
                CategoryFilter.Items.Add(new ComboBoxItem { Content = cat });
            CategoryFilter.SelectedIndex = 0;
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

            var selectedCategory = (CategoryFilter.SelectedItem as ComboBoxItem)?.Content?.ToString();
            if (selectedCategory != null && selectedCategory != "All")
                filtered = filtered.Where(p => p.Category.Name == selectedCategory);

            var selectedComplexity = (ComplexityFilter.SelectedItem as ComboBoxItem)?.Content?.ToString();
            if (selectedComplexity != null && selectedComplexity != "All")
                filtered = filtered.Where(p => p.Complexity == selectedComplexity);

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

        private void View_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var selectedProduct = (Product)button.DataContext;
            Frame.Navigate(typeof(ProductDetailPage), (selectedProduct, _currentUser));
        }
    }
}