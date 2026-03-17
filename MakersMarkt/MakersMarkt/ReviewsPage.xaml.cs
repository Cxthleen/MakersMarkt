using MakersMarkt.Data.Context;
using MakersMarkt.Data.Models;
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
    public sealed partial class ReviewsPage : Page
    {
        private User _currentUser;

        public ReviewsPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            _currentUser = e.Parameter as User;
            LoadReviews();
        }

        private void LoadReviews()
        {
            using var db = new AppDbContext();
            var raw = db.Reviews
                .Where(r => r.BuyerUserId == _currentUser.Id)
                .Select(r => new
                {
                    r.Rating,
                    r.ReviewText,
                    ProductName = r.Product.Name
                })
                .ToList();

            var reviews = raw.Select(r => new
            {
                r.Rating,
                r.ReviewText,
                r.ProductName,
                Stars = new string('?', r.Rating) + new string('?', 5 - r.Rating)
            }).ToList();

            ReviewsList.ItemsSource = reviews;
        }
    }
}
