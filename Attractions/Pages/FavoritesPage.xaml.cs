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
using Attractions.AppData;

namespace Attractions.Pages
{
    public partial class FavoritesPage : Page
    {
        public FavoritesPage()
        {
            InitializeComponent();
            LoadFavorites();
        }

        private void LoadFavorites()
        {
            if (CurrentUser.Users == null) return;

            // Получаем избранные развлечения пользователя
            var favorites = AppConnect.modelDB.Favorites
                .Where(f => f.UserId == CurrentUser.Users.UserId)
                .Join(AppConnect.modelDB.Entertainment,
                    f => f.EntertainmentId,
                    e => e.EntertainmentId,
                    (f, e) => e)
                .ToList();

            FavoritesItemsControl.ItemsSource = favorites;

            // Показываем сообщение, если избранное пусто
            TxtNoFavorites.Visibility = favorites.Any() ? Visibility.Collapsed : Visibility.Visible;
        }

        private void BtnRemoveFromFavorites_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentUser.Users == null) return;

            var button = (Button)sender;
            int entertainmentId = (int)button.CommandParameter;

            // Находим запись в избранном
            var favorite = AppConnect.modelDB.Favorites
                .FirstOrDefault(f => f.UserId == CurrentUser.Users.UserId &&
                                   f.EntertainmentId == entertainmentId);

            if (favorite != null)
            {
                try
                {
                    // Удаляем из избранного
                    AppConnect.modelDB.Favorites.Remove(favorite);
                    AppConnect.modelDB.SaveChanges();

                    // Обновляем список
                    LoadFavorites();

                    MessageBox.Show("Развлечение удалено из избранного", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении из избранного: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // Навигационные методы
        private void BtnMain_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new UserMain());
        }

        private void BtnCart_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new CartPage());
        }

        private void BtnFavorites_Click(object sender, RoutedEventArgs e)
        {
            // Уже на странице избранного
        }

        private void BtnAccount_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AccountPage());
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new Authorization());
        }

        private void BtnUserOrders_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new UserOrders());
        }
    }
}