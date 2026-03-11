using MakersMarkt.Data;
using MakersMarkt.Data.Context;
using MakersMarkt.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.UI;
namespace MakersMarkt.Dashboards
{
    public class ProductModerationViewModel : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string CategoryName { get; set; }
        public int CategoryId { get; set; }
        public string Complexity { get; set; }
        public string MaterialUsage { get; set; }
        public string Durability { get; set; }
        public string AvgRatingDisplay { get; set; }
        public int ReportCount { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
    public class CategoryStatViewModel
    {
        public string CategoryName { get; set; }
        public int ProductCount { get; set; }
        public double BarWidth { get; set; }
    }
    public class RatingStatViewModel
    {
        public string CategoryName { get; set; }
        public double AverageRating { get; set; }
        public string AverageRatingDisplay => AverageRating.ToString("F1");
    }
    public class PopularTypeViewModel
    {
        public string Rank { get; set; }
        public string TypeName { get; set; }
        public int Count { get; set; }
        public string CountDisplay => $"{Count} producten";
        public string BadgeColor { get; set; }
    }
    public class ModerationLogViewModel
    {
        public string ActionType { get; set; }
        public string ProductName { get; set; }
        public string Note { get; set; }
        public string ModeratorName { get; set; }
        public string ActionColor => ActionType switch
        {
            "Goedgekeurd" => "#34C759",
            "Afgewezen" => "#FF9500",
            "Verwijderd" => "#FF3B30",
            "Categorie" => "#007AFF",
            _ => "#8E8E93"
        };
    }
    public class SearchResultViewModel
    {
        public int ProductId { get; set; }
        public string ResultType { get; set; }
        public string Title { get; set; }
        public string Snippet { get; set; }
        public string ContextInfo { get; set; }
        public string TypeColor => ResultType switch
        {
            "Product" => "#007AFF",
            "Recensie" => "#FF9500",
            "Gebruiker" => "#8E8E93",
            _ => "#C7C7CC"
        };
    }
    public class ReportModerationViewModel : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string Reason { get; set; }
        public string Status { get; set; }
        public string StatusDisplay => Status switch
        {
            "open" => "Open",
            "pending" => "In behandeling",
            "resolved" => "Opgelost",
            _ => Status
        };
        public string ReporterName { get; set; }
        public string FlagName { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
    public class FlagViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Pattern { get; set; }
    }
    public sealed partial class ModerationPage : Page
    {
        private User _currentUser;
        private AppDbContext _db;

        private ObservableCollection<ProductModerationViewModel> _allProducts = new();
        private ObservableCollection<ProductModerationViewModel> _filteredProducts = new();
        private ObservableCollection<ReportModerationViewModel> _allReports = new();
        private ObservableCollection<ReportModerationViewModel> _filteredReports = new();
        private ObservableCollection<SearchResultViewModel> _searchResults = new();
        private ObservableCollection<CategoryStatViewModel> _categoryStats = new();
        private ObservableCollection<RatingStatViewModel> _ratingStats = new();
        private ObservableCollection<PopularTypeViewModel> _popularTypes = new();
        private ObservableCollection<ModerationLogViewModel> _recentActions = new();
        private ObservableCollection<FlagViewModel> _flags = new();
        private List<Category> _categories = new();
        private int _pendingProductId;
        private string _pendingAction;
        public ModerationPage()
        {
            InitializeComponent();
            _db = new AppDbContext();
            ProductListView.ItemsSource = _filteredProducts;
            ReportsListView.ItemsSource = _filteredReports;
            SearchResultsListView.ItemsSource = _searchResults;
            CategoryStatsControl.ItemsSource = _categoryStats;
            RatingStatsControl.ItemsSource = _ratingStats;
            PopularTypesControl.ItemsSource = _popularTypes;
            RecentActionsListView.ItemsSource = _recentActions;
            FlagButtonsControl.ItemsSource = _flags;
        }
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            _currentUser = e.Parameter as User;
            base.OnNavigatedTo(e);
            if (_currentUser != null)
            {
                ModeratorName.Text = _currentUser.DisplayName ?? _currentUser.Username;
                ModeratorInitial.Text = (ModeratorName.Text.Length > 0
                    ? ModeratorName.Text[0].ToString().ToUpper()
                    : "M");
            }
            await LoadAllDataAsync();
        }
        private async Task LoadAllDataAsync()
        {
            ShowLoading(true);
            try
            {
                await LoadCategoriesAsync();
                await LoadProductsAsync();
                await LoadReportsAsync();
                await LoadStatsAsync();
                await LoadFlagsAsync();
                await LoadRecentActionsAsync();
                UpdateBadges();
            }
            catch (Exception ex)
            {
                ShowStatus($"Fout bij laden: {ex.Message}", isError: true);
            }
            finally
            {
                ShowLoading(false);
            }
        }
        private async Task LoadCategoriesAsync()
        {
            _categories = await _db.Categories.OrderBy(c => c.Name).ToListAsync();
            CategoryFilterComboBox.Items.Clear();
            CategoryFilterComboBox.Items.Add(new ComboBoxItem { Content = "Alle categorieën", Tag = 0 });
            foreach (var cat in _categories)
                CategoryFilterComboBox.Items.Add(new ComboBoxItem { Content = cat.Name, Tag = cat.Id });
            CategoryAssignComboBox.Items.Clear();
            foreach (var cat in _categories)
                CategoryAssignComboBox.Items.Add(new ComboBoxItem { Content = cat.Name, Tag = cat.Id });
            TotalCategoriesCount.Text = _categories.Count.ToString();
        }
        private async Task LoadProductsAsync()
        {
            var products = await _db.Products
                .Include(p => p.Category)
                .Include(p => p.Reviews)
                .Include(p => p.Reports)
                .OrderBy(p => p.Name)
                .ToListAsync();

            _allProducts.Clear();
            _filteredProducts.Clear();

            foreach (var p in products)
            {
                double avg = p.Reviews.Any() ? p.Reviews.Average(r => r.Rating) : 0;
                var vm = new ProductModerationViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description ?? string.Empty,
                    CategoryName = p.Category?.Name ?? "Geen categorie",
                    CategoryId = p.CategoryId,
                    Complexity = p.Complexity ?? "—",
                    MaterialUsage = p.MaterialUsage ?? "—",
                    Durability = p.Durability ?? "—",
                    AvgRatingDisplay = avg > 0 ? avg.ToString("F1") : "—",
                    ReportCount = p.Reports.Count
                };
                _allProducts.Add(vm);
                _filteredProducts.Add(vm);
            }
            TotalProductsCount.Text = products.Count.ToString();
            UpdateProductCountLabel();
        }
        private async Task LoadReportsAsync()
        {
            var reports = await _db.Reports
                .Include(r => r.Product)
                .Include(r => r.User)
                .Include(r => r.Flag)
                .OrderByDescending(r => r.Id)
                .ToListAsync();
            _allReports.Clear();
            _filteredReports.Clear();
            int openCount = 0;
            foreach (var r in reports)
            {
                var vm = new ReportModerationViewModel
                {
                    Id = r.Id,
                    ProductId = r.ProductId,
                    ProductName = r.Product?.Name ?? $"Product #{r.ProductId}",
                    Reason = r.Reason ?? "Geen reden opgegeven",
                    Status = r.Status ?? "open",
                    ReporterName = r.User?.DisplayName ?? r.User?.Username ?? "Anoniem",
                    FlagName = r.Flag?.Name ?? "—"
                };
                _allReports.Add(vm);
                _filteredReports.Add(vm);
                if (vm.Status == "open") openCount++;
            }
            TotalReportsCount.Text = openCount.ToString();
            UpdateReportCountLabel();
            if (ReportsEmptyState != null)
                ReportsEmptyState.Visibility = _filteredReports.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            if (ReportsListView != null)
                ReportsListView.Visibility = _filteredReports.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        }
        private async Task LoadStatsAsync()
        {
            var catGroups = await _db.Products
                .Include(p => p.Category)
                .GroupBy(p => p.Category.Name)
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .ToListAsync();
            _categoryStats.Clear();
            int maxCount = catGroups.FirstOrDefault()?.Count ?? 1;
            foreach (var g in catGroups)
            {
                _categoryStats.Add(new CategoryStatViewModel
                {
                    CategoryName = g.Name ?? "Onbekend",
                    ProductCount = g.Count,
                    BarWidth = maxCount > 0 ? (g.Count / (double)maxCount) * 300 : 0
                });
            }
            var ratingGroups = await _db.Reviews
                .Include(r => r.Product).ThenInclude(p => p.Category)
                .GroupBy(r => r.Product.Category.Name)
                .Select(g => new { Name = g.Key, Avg = g.Average(r => r.Rating) })
                .OrderByDescending(g => g.Avg)
                .ToListAsync();
            _ratingStats.Clear();
            foreach (var g in ratingGroups)
            {
                _ratingStats.Add(new RatingStatViewModel
                {
                    CategoryName = g.Name ?? "Onbekend",
                    AverageRating = g.Avg
                });
            }
            bool hasReviews = await _db.Reviews.AnyAsync();
            if (hasReviews)
            {
                double platformAvg = await _db.Reviews.AverageAsync(r => r.Rating);
                AvgRatingDisplay.Text = platformAvg.ToString("F1");
            }
            else
            {
                AvgRatingDisplay.Text = "—";
            }
            var typeGroups = await _db.Products
                .Where(p => p.Complexity != null)
                .GroupBy(p => p.Complexity)
                .Select(g => new { Type = g.Key, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .Take(5)
                .ToListAsync();
            _popularTypes.Clear();
            string[] rankBadges = { "1", "2", "3", "4", "5" };
            string[] badgeColors = { "#007AFF", "#34C759", "#FF9500", "#8E8E93", "#C7C7CC" };
            for (int i = 0; i < typeGroups.Count; i++)
            {
                _popularTypes.Add(new PopularTypeViewModel
                {
                    Rank = rankBadges[i],
                    TypeName = typeGroups[i].Type,
                    Count = typeGroups[i].Count,
                    BadgeColor = badgeColors[i]
                });
            }
        }
        private async Task LoadFlagsAsync()
        {
            var flags = await _db.Flags
                .Where(f => f.IsActive)
                .OrderBy(f => f.Name)
                .ToListAsync();

            _flags.Clear();
            foreach (var f in flags)
                _flags.Add(new FlagViewModel { Id = f.Id, Name = f.Name, Pattern = f.Pattern });
        }
        private async Task LoadRecentActionsAsync()
        {
            var mods = await _db.Moderations
                .Include(m => m.Product)
                .Include(m => m.User)
                .OrderByDescending(m => m.Id)
                .Take(20)
                .ToListAsync();
            _recentActions.Clear();
            foreach (var m in mods)
            {
                _recentActions.Add(new ModerationLogViewModel
                {
                    ActionType = m.ActionType ?? "Actie",
                    ProductName = m.Product?.Name ?? $"Product #{m.ProductId}",
                    Note = string.IsNullOrWhiteSpace(m.Note) ? "Geen notitie" : m.Note,
                    ModeratorName = m.User?.DisplayName ?? m.User?.Username ?? "Onbekend"
                });
            }
        }
        private void UpdateBadges()
        {
            int openReports = _allReports.Count(r => r.Status == "open");
            PendingReportsBadge.Visibility = openReports > 0 ? Visibility.Visible : Visibility.Collapsed;
            PendingReportsCount.Text = $"{openReports} open";
        }
        private void UpdateProductCountLabel()
        {
            if (ProductCountLabel != null)
                ProductCountLabel.Text = $"{_filteredProducts.Count} producten";
        }

        private void UpdateReportCountLabel()
        {
            if (ReportCountLabel != null)
                ReportCountLabel.Text = $"{_filteredReports.Count} meldingen";
        }

        private void ShowLoading(bool show)
            => LoadingOverlay.Visibility = show ? Visibility.Visible : Visibility.Collapsed;

        private void ShowStatus(string message, bool isError = false)
        {
            StatusInfoBar.Severity = isError ? InfoBarSeverity.Error : InfoBarSeverity.Success;
            StatusInfoBar.Message = message;
            StatusInfoBar.IsOpen = true;
        }
        private async Task LogModerationActionAsync(int productId, string actionType, string note)
        {
            var entry = new Moderation
            {
                ProductId = productId,
                UserId = _currentUser?.Id ?? 0,
                ActionType = actionType,
                Note = note
            };
            _db.Moderations.Add(entry);
            await _db.SaveChangesAsync();
            await LoadRecentActionsAsync();
        }
        private void ProductSearchBox_TextChanged(object sender, TextChangedEventArgs e)
            => ApplyProductFilters();

        private void CategoryFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
            => ApplyProductFilters();

        private void StatusFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
            => ApplyProductFilters();

        private void ApplyProductFilters()
        {
            if (ProductSearchBox == null || CategoryFilterComboBox == null || StatusFilterComboBox == null) return;

            string search = ProductSearchBox.Text?.ToLower() ?? string.Empty;
            int catId = (CategoryFilterComboBox.SelectedItem as ComboBoxItem)?.Tag is int c ? c : 0;
            string statusTag = (StatusFilterComboBox.SelectedItem as ComboBoxItem)?.Tag as string ?? "all";

            var filtered = _allProducts.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(search))
                filtered = filtered.Where(p =>
                    p.Name.ToLower().Contains(search) ||
                    p.Description.ToLower().Contains(search) ||
                    p.CategoryName.ToLower().Contains(search));

            if (catId > 0)
                filtered = filtered.Where(p => p.CategoryId == catId);
            if (statusTag == "pending")
                filtered = filtered.Where(p => p.ReportCount > 0);

            _filteredProducts.Clear();
            foreach (var p in filtered)
                _filteredProducts.Add(p);

            UpdateProductCountLabel();
        }

        private void ProductListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //detail panel?
        }
        private async void ApproveProduct_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is int id)
                await OpenModerationDialog(id, "approve");
        }
        private async void RejectProduct_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is int id)
                await OpenModerationDialog(id, "reject");
        }
        private async void DeleteProduct_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is int id)
                await OpenModerationDialog(id, "delete");
        }
        private async Task OpenModerationDialog(int productId, string action)
        {
            _pendingProductId = productId;
            _pendingAction = action;

            var product = _allProducts.FirstOrDefault(p => p.Id == productId);
            string productName = product?.Name ?? $"Product #{productId}";

            ModerationActionDescription.Text = action switch
            {
                "approve" => $"Weet je zeker dat je '{productName}' wilt goedkeuren?",
                "reject" => $"Weet je zeker dat je '{productName}' wilt afwijzen?",
                "delete" => $"⚠️ Weet je zeker dat je '{productName}' permanent wilt verwijderen? Dit kan niet ongedaan worden gemaakt.",
                _ => string.Empty
            };
            ModerationNoteBox.Text = string.Empty;
            ModerationActionDialog.Title = action switch
            {
                "approve" => "Product goedkeuren",
                "reject" => "Product afwijzen",
                "delete" => "Product verwijderen",
                _ => "Bevestigen"
            };
            await ModerationActionDialog.ShowAsync();
        }
        private async void ModerationActionDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            string note = ModerationNoteBox.Text?.Trim() ?? string.Empty;
            await ExecuteModerationAction(_pendingProductId, _pendingAction, note);
        }
        private async Task ExecuteModerationAction(int productId, string action, string note)
        {
            ShowLoading(true);
            try
            {
                switch (action)
                {
                    case "approve":
                        await LogModerationActionAsync(productId, "Goedgekeurd", note);
                        ShowStatus($"Product #{productId} is goedgekeurd.");
                        break;

                    case "reject":
                        await LogModerationActionAsync(productId, "Afgewezen", note);
                        ShowStatus($"Product #{productId} is afgewezen.");
                        break;

                    case "delete":
                        var product = await _db.Products.FindAsync(productId);
                        if (product != null)
                        {
                            await LogModerationActionAsync(productId, "Verwijderd", note);
                            _db.Products.Remove(product);
                            await _db.SaveChangesAsync();
                            await LoadProductsAsync();
                            await LoadStatsAsync();
                        }
                        ShowStatus($"Product is verwijderd.", isError: false);
                        break;
                }
                if (action != "delete")
                    await CreateNotificationForProductOwnerAsync(productId, action);
            }
            catch (Exception ex)
            {
                ShowStatus($"Fout: {ex.Message}", isError: true);
            }
            finally
            {
                ShowLoading(false);
            }
        }
        private async Task CreateNotificationForProductOwnerAsync(int productId, string action)
        {
            try
            {
                var buyerIds = await _db.OrderProducts
                    .Where(op => op.ProductId == productId)
                    .Select(op => op.Order.BuyerUserId)
                    .Distinct()
                    .ToListAsync();
                foreach (var uid in buyerIds)
                {
                    var notification = new Notification
                    {
                        UserId = uid,
                        Type = action == "approve" ? "product_approved" : "product_rejected",
                        IsRead = false
                    };
                    _db.Notifications.Add(notification);
                }
                if (buyerIds.Any())
                    await _db.SaveChangesAsync();
            }
            catch
            {
            }
        }
        private async void AssignCategory_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is int id)
                await OpenCategoryDialog(id);
        }
        private async Task OpenCategoryDialog(int productId)
        {
            _pendingProductId = productId;
            var product = _allProducts.FirstOrDefault(p => p.Id == productId);
            CategoryDialogProductName.Text = product?.Name ?? $"Product #{productId}";
            for (int i = 0; i < CategoryAssignComboBox.Items.Count; i++)
            {
                if ((CategoryAssignComboBox.Items[i] as ComboBoxItem)?.Tag is int cid
                    && cid == product?.CategoryId)
                {
                    CategoryAssignComboBox.SelectedIndex = i;
                    break;
                }
            }
            CategoryAssignNote.Text = string.Empty;
            await CategoryAssignDialog.ShowAsync();
        }
        private async void CategoryAssignDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            int? newCategoryId = (CategoryAssignComboBox.SelectedItem as ComboBoxItem)?.Tag as int?;
            if (newCategoryId == null) return;

            ShowLoading(true);
            try
            {
                var product = await _db.Products.FindAsync(_pendingProductId);
                if (product != null)
                {
                    product.CategoryId = newCategoryId.Value;
                    await _db.SaveChangesAsync();
                    await LogModerationActionAsync(
                        _pendingProductId,
                        "Categorie",
                        $"Categorie gewijzigd naar {_categories.FirstOrDefault(c => c.Id == newCategoryId)?.Name}. {CategoryAssignNote.Text}".Trim()
                    );
                    await LoadProductsAsync();
                    await LoadStatsAsync();
                    ShowStatus("Categorie succesvol bijgewerkt.");
                }
            }
            catch (Exception ex)
            {
                ShowStatus($"Fout bij opslaan: {ex.Message}", isError: true);
            }
            finally
            {
                ShowLoading(false);
            }
        }
        private void ContentSearchBox_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
                ExecuteContentSearch();
        }

        private void ContentSearch_Click(object sender, RoutedEventArgs e)
            => ExecuteContentSearch();

        private void FlagButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is string pattern)
            {
                ContentSearchBox.Text = pattern;
                ExecuteContentSearch();
            }
        }

        private async void ExecuteContentSearch()
        {
            string query = ContentSearchBox.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(query)) return;

            ShowLoading(true);
            _searchResults.Clear();
            SearchResultsListView.Visibility = Visibility.Collapsed;
            SearchEmptyState.Visibility = Visibility.Collapsed;

            try
            {
                string scope = (SearchScopeComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Alles";
                string q = query.ToLower();
                int count = 0;
                if (scope is "Alles" or "Productnamen" or "Beschrijvingen")
                {
                    var products = await _db.Products
                        .Include(p => p.Category)
                        .ToListAsync();

                    foreach (var p in products)
                    {
                        bool nameMatch = scope != "Beschrijvingen" && (p.Name?.ToLower().Contains(q) ?? false);
                        bool descMatch = scope != "Productnamen" && (p.Description?.ToLower().Contains(q) ?? false);
                        if (!nameMatch && !descMatch) continue;

                        _searchResults.Add(new SearchResultViewModel
                        {
                            ProductId = p.Id,
                            ResultType = nameMatch ? "Product" : "Beschrijving",
                            Title = p.Name,
                            Snippet = TruncateAroundKeyword(nameMatch ? p.Name : p.Description, query, 120),
                            ContextInfo = $"Categorie: {p.Category?.Name ?? "—"} · ID: {p.Id}"
                        });
                        count++;
                    }
                }

                if (scope is "Alles" or "Recensies")
                {
                    var reviews = await _db.Reviews
                        .Include(r => r.Product)
                        .Include(r => r.Buyer)
                        .Where(r => r.ReviewText != null)
                        .ToListAsync();

                    foreach (var r in reviews.Where(r => r.ReviewText.ToLower().Contains(q)))
                    {
                        _searchResults.Add(new SearchResultViewModel
                        {
                            ProductId = r.ProductId,
                            ResultType = "Recensie",
                            Title = $"Recensie voor '{r.Product?.Name ?? "—"}'",
                            Snippet = TruncateAroundKeyword(r.ReviewText, query, 120),
                            ContextInfo = $"Door: {r.Buyer?.DisplayName ?? "—"} · Beoordeling: {r.Rating}/5"
                        });
                        count++;
                    }
                }
                if (scope is "Alles" or "Gebruikersnamen")
                {
                    var users = await _db.Users.ToListAsync();
                    foreach (var u in users.Where(u =>
                        (u.Username?.ToLower().Contains(q) ?? false) ||
                        (u.DisplayName?.ToLower().Contains(q) ?? false) ||
                        (u.Biography?.ToLower().Contains(q) ?? false)))
                    {
                        _searchResults.Add(new SearchResultViewModel
                        {
                            ProductId = 0,
                            ResultType = "Gebruiker",
                            Title = u.DisplayName ?? u.Username,
                            Snippet = TruncateAroundKeyword(u.Biography ?? u.Username, query, 120),
                            ContextInfo = $"Gebruikersnaam: {u.Username} · Rol: {u.Role}"
                        });
                        count++;
                    }
                }

                // Update UI
                if (count > 0)
                {
                    SearchResultsListView.Visibility = Visibility.Visible;
                    SearchSummaryBar.Visibility = Visibility.Visible;
                    SearchResultsSummaryText.Text = $"{count} resultaat{(count != 1 ? "en" : "")} gevonden voor '{query}'";
                }
                else
                {
                    SearchEmptyState.Visibility = Visibility.Visible;
                    SearchSummaryBar.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                ShowStatus($"Zoekfout: {ex.Message}", isError: true);
            }
            finally
            {
                ShowLoading(false);
            }
        }

        private static string TruncateAroundKeyword(string text, string keyword, int maxLength)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            int idx = text.ToLower().IndexOf(keyword.ToLower(), StringComparison.OrdinalIgnoreCase);
            if (idx < 0) return text.Length > maxLength ? text[..maxLength] + "…" : text;
            int start = Math.Max(0, idx - 40);
            int end = Math.Min(text.Length, idx + keyword.Length + 60);
            string snippet = text[start..end];
            if (start > 0) snippet = "…" + snippet;
            if (end < text.Length) snippet += "…";
            return snippet;
        }

        private void ViewProductFromSearch_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is int productId && productId > 0)
            {
                MainTabView.SelectedIndex = 1;
                ProductSearchBox.Text = _allProducts.FirstOrDefault(p => p.Id == productId)?.Name ?? string.Empty;
            }
        }
        private void ReportStatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
            => ApplyReportFilters();

        private void ReportSearch_TextChanged(object sender, TextChangedEventArgs e)
            => ApplyReportFilters();

        private void ApplyReportFilters()
        {
            if (ReportSearchBox == null || ReportStatusFilter == null) return;

            string search = ReportSearchBox.Text?.ToLower() ?? string.Empty;
            string statusTag = (ReportStatusFilter.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Alle meldingen";

            var filtered = _allReports.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(search))
                filtered = filtered.Where(r =>
                    r.ProductName.ToLower().Contains(search) ||
                    r.Reason.ToLower().Contains(search));

            filtered = statusTag switch
            {
                "Open" => filtered.Where(r => r.Status == "open"),
                "In behandeling" => filtered.Where(r => r.Status == "pending"),
                "Opgelost" => filtered.Where(r => r.Status == "resolved"),
                _ => filtered
            };

            _filteredReports.Clear();
            foreach (var r in filtered)
                _filteredReports.Add(r);

            UpdateReportCountLabel();
            if (ReportsEmptyState != null)
                ReportsEmptyState.Visibility = _filteredReports.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            if (ReportsListView != null)
                ReportsListView.Visibility = _filteredReports.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private async void ReportCategoryCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox cb
                && cb.Tag is int productId
                && cb.SelectedItem is ComboBoxItem item
                && item.Tag is int categoryId)
            {
                ShowLoading(true);
                try
                {
                    var product = await _db.Products.FindAsync(productId);
                    if (product != null)
                    {
                        product.CategoryId = categoryId;
                        await _db.SaveChangesAsync();
                        await LogModerationActionAsync(productId, "Categorie",
                            $"Categorie ingesteld via meldingswachtrij: {_categories.FirstOrDefault(c => c.Id == categoryId)?.Name}");
                        ShowStatus("Categorie bijgewerkt.");
                    }
                }
                catch (Exception ex)
                {
                    ShowStatus($"Fout: {ex.Message}", isError: true);
                }
                finally
                {
                    ShowLoading(false);
                }
            }
        }

        private async void ApproveFromReport_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is int reportId)
                await SetReportStatus(reportId, "resolved", productAction: "approve");
        }

        private async void DeleteFromReport_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is int productId)
                await OpenModerationDialog(productId, "delete");
        }

        private async void CloseReport_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is int reportId)
                await SetReportStatus(reportId, "resolved", productAction: null);
        }

        private async Task SetReportStatus(int reportId, string status, string productAction)
        {
            ShowLoading(true);
            try
            {
                var report = await _db.Reports.FindAsync(reportId);
                if (report != null)
                {
                    report.Status = status;
                    await _db.SaveChangesAsync();

                    if (productAction == "approve")
                        await LogModerationActionAsync(report.ProductId, "Goedgekeurd",
                            "Goedgekeurd via meldingswachtrij");

                    var notif = new Notification
                    {
                        UserId = report.UserId,
                        Type = "report_resolved",
                        IsRead = false
                    };
                    _db.Notifications.Add(notif);
                    await _db.SaveChangesAsync();

                    await LoadReportsAsync();
                    UpdateBadges();
                    ShowStatus("Melding succesvol afgehandeld. De gebruiker is op de hoogte gesteld.");
                }
            }
            catch (Exception ex)
            {
                ShowStatus($"Fout: {ex.Message}", isError: true);
            }
            finally
            {
                ShowLoading(false);
            }
        }
        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadAllDataAsync();
            ShowStatus("Dashboard vernieuwd.");
        }
    }
}
