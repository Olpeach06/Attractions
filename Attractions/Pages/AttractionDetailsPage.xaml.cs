using Attractions.AppData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Collections.Specialized.BitVector32;
namespace Attractions.Pages
{
    public partial class AttractionDetailsPage : Page
    {
        private readonly int _attractionId;
        private readonly int _currentUserId; // Assuming you have a way to get current user ID

        public AttractionDetailsPage(int attractionId)
        {
            InitializeComponent();
            _attractionId = attractionId;
            _currentUserId = GetCurrentUserId(); // Implement this method based on your auth system
            LoadAttractionDetails();
            LoadSchedule();
            LoadReviews();
            CheckIfInFavorites();
        }

        private int GetCurrentUserId()
        {
            // Implement based on your authentication system
            // For example, if you have a static class with current user info:
            return CurrentUser.Users.UserId;

            // Or if you're passing it from previous page:
            // return somePassedUserId;
        }

        private void LoadAttractionDetails()
        {
            var attraction = AppConnect.modelDB.Entertainment
                .FirstOrDefault(a => a.EntertainmentId == _attractionId);

            if (attraction != null)
            {
                this.DataContext = attraction;
                //tbTitle.Text = attraction.Name;
            }
        }

        private void LoadSchedule()
        {
            var availableSessions = AppConnect.modelDB.Schedule
                .Where(s => s.EntertainmentId == _attractionId &&
                           s.StartTime > DateTime.Now &&
                           s.CurrentParticipants < s.MaxParticipants)
                .OrderBy(s => s.StartTime)
                .ToList();

            MessageBox.Show($"Найдено расписаний: {availableSessions.Count}");
            lvSchedule.ItemsSource = availableSessions;
        }

        private void LoadReviews()
        {
            var reviews = AppConnect.modelDB.Reviews
                .Where(r => r.EntertainmentId == _attractionId)
                .OrderByDescending(r => r.ReviewDate)
                .ToList();

            lvReviews.ItemsSource = reviews;
        }

        private void CheckIfInFavorites()
        {
            bool isFavorite = AppConnect.modelDB.Favorites
                .Any(f => f.UserId == _currentUserId && f.EntertainmentId == _attractionId);

            btnAddToFavorites.Content = isFavorite ? "В избранном" : "Добавить в избранное";
            btnAddToFavorites.Tag = isFavorite;
        }

        private void btnAddToFavorites_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)btnAddToFavorites.Tag) 
            {
                MessageBox.Show("Это развлечение уже добавлено в избранное.", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                var newFavorite = new Favorites
                {
                    UserId = _currentUserId,
                    EntertainmentId = _attractionId
                };

                AppConnect.modelDB.Favorites.Add(newFavorite);
                AppConnect.modelDB.SaveChanges();

                btnAddToFavorites.Content = "В избранном";
                btnAddToFavorites.Tag = true;
                MessageBox.Show("Развлечение успешно добавлено в избранное!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении в избранное: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnAddToCart_Click(object sender, RoutedEventArgs e)
        {
            if (lvSchedule.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите дату и время!", "Предупреждение",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedSchedule = (Schedule)lvSchedule.SelectedItem;

            try
            {
                // Check if user has a cart, create if not
                var userCart = AppConnect.modelDB.Cart.FirstOrDefault(c => c.UserId == _currentUserId);
                if (userCart == null)
                {
                    userCart = new Cart { UserId = _currentUserId };
                    AppConnect.modelDB.Cart.Add(userCart);
                    AppConnect.modelDB.SaveChanges();
                }

                // Check if this schedule item already exists in cart
                var existingItem = AppConnect.modelDB.CartItems
                    .FirstOrDefault(ci => ci.CartId == userCart.CartId && ci.ScheduleId == selectedSchedule.ScheduleId);

                if (existingItem != null)
                {
                    MessageBox.Show("Этот сеанс уже находится в корзине!", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Add new item to cart
                var newCartItem = new CartItems
                {
                    CartId = userCart.CartId,
                    ScheduleId = selectedSchedule.ScheduleId,
                    Quantity = 1
                };

                AppConnect.modelDB.CartItems.Add(newCartItem);
                AppConnect.modelDB.SaveChanges();

                MessageBox.Show("Развлечение успешно добавлено в корзинуy!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                // Refresh schedule to show updated availability
                LoadSchedule();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении в корзину: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnBook_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            int scheduleId = (int)button.Tag;

            // Find and select the corresponding item in the ListView
            var scheduleItem = lvSchedule.Items.OfType<Schedule>()
                .FirstOrDefault(s => s.ScheduleId == scheduleId);

            if (scheduleItem != null)
            {
                lvSchedule.SelectedItem = scheduleItem;
                btnAddToCart_Click(sender, e);
            }
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}
