using MakersMarkt.Data.Context;
using MakersMarkt.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MakersMarkt.Dashboards
{
    public sealed partial class AdminDashboard : Page
    {
        private User _currentUser;
        private AppDbContext _db;
        private ObservableCollection<ModerationLogViewModel> _logs = new();

        public AdminDashboard()
        {
            InitializeComponent();
            _db = new AppDbContext();
            AllLogsListView.ItemsSource = _logs;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            _currentUser = e.Parameter as User;
            base.OnNavigatedTo(e);

            if (_currentUser != null)
            {
                WelcomeText.Text = _currentUser.DisplayName ?? _currentUser.Username;
                AdminInitial.Text = WelcomeText.Text.Length > 0
                    ? WelcomeText.Text[0].ToString().ToUpper()
                    : "A";
            }

            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            LoadingOverlay.Visibility = Visibility.Visible;
            try
            {
                int totalProducts = await _db.Products.CountAsync();
                int openReports = await _db.Reports.CountAsync(r => r.Status == "open");
                int totalUsers = await _db.Users.CountAsync();
                int totalModActions = await _db.Moderations.CountAsync();

                TotalProductsText.Text = totalProducts.ToString();
                OpenReportsText.Text = openReports.ToString();
                TotalUsersText.Text = totalUsers.ToString();
                TotalModActionsText.Text = totalModActions.ToString();

                var mods = await _db.Moderations
                    .Include(m => m.Product)
                    .Include(m => m.User)
                    .OrderByDescending(m => m.Id)
                    .ToListAsync();

                _logs.Clear();
                foreach (var m in mods)
                    _logs.Add(new ModerationLogViewModel
                    {
                        ActionType = m.ActionType ?? "Actie",
                        ProductName = m.Product?.Name ?? $"Product #{m.ProductId}",
                        Note = string.IsNullOrWhiteSpace(m.Note) ? "Geen notitie" : m.Note,
                        ModeratorName = m.User?.DisplayName ?? m.User?.Username ?? "Onbekend"
                    });

                LogCountText.Text = $"{_logs.Count} acties";
            }
            catch (Exception ex)
            {
                LogCountText.Text = "Fout bij laden";
                _ = ex;
            }
            finally
            {
                LoadingOverlay.Visibility = Visibility.Collapsed;
            }
        }

        private void ModerationButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(ModerationPage), _currentUser);
        }

        private void AddCreditButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(AddCreditPage), _currentUser);
        }
    }
}