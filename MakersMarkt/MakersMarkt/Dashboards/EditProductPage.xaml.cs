using MakersMarkt.Data.Context;
using MakersMarkt.Data.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Threading.Tasks;

namespace MakersMarkt.Dashboards
{
    public sealed partial class EditProductPage : Page
    {
        private Product _currentProduct;
        private string _selectedComplexity;
        private string _selectedDurability;

        public EditProductPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is Product product)
            {
                _currentProduct = product;
                LoadProductDetails();
            }
        }

        private void LoadProductDetails()
        {
            NameTextBox.Text = _currentProduct.Name;
            DescriptionTextBox.Text = _currentProduct.Description;
            MaterialTextBox.Text = _currentProduct.MaterialUsage;
            ProductionTimeTextBox.Text = _currentProduct.ProductionTime;

            _selectedComplexity = _currentProduct.Complexity;
            ComplexityDropDown.Content = _currentProduct.Complexity;

            _selectedDurability = _currentProduct.Durability;
            DurabilityDropDown.Content = _currentProduct.Durability;
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
                string.IsNullOrWhiteSpace(ProductionTimeTextBox.Text))
            {
                ShowDialog("Please fill in all fields.");
                return;
            }

            try
            {
                _currentProduct.Name = NameTextBox.Text;
                _currentProduct.Description = DescriptionTextBox.Text;
                _currentProduct.MaterialUsage = MaterialTextBox.Text;
                _currentProduct.ProductionTime = ProductionTimeTextBox.Text;
                _currentProduct.Complexity = _selectedComplexity;
                _currentProduct.Durability = _selectedDurability;

                using (var db = new AppDbContext())
                {
                    db.Products.Attach(_currentProduct);
                    db.Entry(_currentProduct).State =
                        Microsoft.EntityFrameworkCore.EntityState.Modified;
                    db.SaveChanges();
                }

                ShowDialog("Product updated successfully.");

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
                Title = "Edit Product",
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