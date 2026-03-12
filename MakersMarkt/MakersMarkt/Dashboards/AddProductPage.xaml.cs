using MakersMarkt.Data.Context;
using MakersMarkt.Data.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MakersMarkt.Dashboards
{
    public sealed partial class AddProductPage : Page
    {
        private User _currentUser;
        private string _selectedComplexity;
        private string _selectedDurability;

        public AddProductPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            _currentUser = e.Parameter as User;
            LoadCategories();
        }

        private void LoadCategories()
        {
            using var db = new AppDbContext();
            var categories = db.Categories.ToList();
            foreach (var cat in categories)
                CategoryDropDown.Items.Add(new ComboBoxItem { Content = cat.Name, Tag = cat.Id });
        }

        private void Complexity_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem item)
            {
                _selectedComplexity = item.Text;
                ComplexityDropDown.Content = item.Text;
            }
        }

        private void Durability_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem item)
            {
                _selectedDurability = item.Text;
                DurabilityDropDown.Content = item.Text;
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
                Frame.GoBack();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameTextBox.Text) ||
                string.IsNullOrWhiteSpace(DescriptionTextBox.Text) ||
                string.IsNullOrWhiteSpace(MaterialTextBox.Text) ||
                string.IsNullOrWhiteSpace(ProductionTimeTextBox.Text) ||
                CategoryDropDown.SelectedItem == null ||
                _selectedComplexity == null ||
                _selectedDurability == null)
            {
                ShowDialog("Please fill in all fields.");
                return;
            }

            try
            {
                var selectedCategory = (ComboBoxItem)CategoryDropDown.SelectedItem;

                var newProduct = new Product
                {
                    Name = NameTextBox.Text,
                    Description = DescriptionTextBox.Text,
                    CategoryId = (int)selectedCategory.Tag,
                    SellerUserId = _currentUser.Id,
                    MaterialUsage = MaterialTextBox.Text,
                    ProductionTime = ProductionTimeTextBox.Text,
                    Complexity = _selectedComplexity,
                    Durability = _selectedDurability
                };

                using (var db = new AppDbContext())
                {
                    db.Products.Add(newProduct);
                    db.SaveChanges();
                }

                ShowDialog("Product added successfully.");

                var timer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(1.5)
                };
                timer.Tick += (s, args) =>
                {
                    timer.Stop();
                    if (Frame.CanGoBack)
                        Frame.GoBack();
                };
                timer.Start();
            }
            catch (Exception ex)
            {
                ShowDialog($"Something went wrong: {ex.Message}");
            }
        }

        private async void ShowDialog(string message)
        {
            var dialog = new ContentDialog
            {
                Title = "Add Product",
                Content = message,
                XamlRoot = Content.XamlRoot
            };

            var task = dialog.ShowAsync();
            await Task.Delay(1000);
            dialog.Hide();
            _ = await task;
        }
    }
}