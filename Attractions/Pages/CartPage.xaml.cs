using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Collections.ObjectModel;

namespace Attractions.Pages
{
    public partial class CartPage : Page
    {
        public decimal TotalAmount { get; set; }

        public CartPage()
        {
            InitializeComponent();
            LoadCartItems();
            DataContext = this;
        }

        private void LoadCartItems()
        {
            try
            {
                if (CurrentUser.Users == null) return;

                var cart = AppConnect.modelDB.Cart
                    .Include("CartItems.Schedule.Entertainment")
                    .FirstOrDefault(c => c.UserId == CurrentUser.Users.UserId);

                if (cart != null)
                {
                    lvCartItems.ItemsSource = cart.CartItems.ToList();
                    TotalAmount = cart.CartItems.Sum(ci => ci.Quantity * ci.Schedule.Entertainment.Price);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке корзины: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnRemove_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int cartItemId)
            {
                try
                {
                    var item = AppConnect.modelDB.CartItems.Find(cartItemId);
                    if (item != null)
                    {
                        AppConnect.modelDB.CartItems.Remove(item);
                        AppConnect.modelDB.SaveChanges();
                        LoadCartItems();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnCheckout_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentUser.Users == null)
            {
                MessageBox.Show("Для оформления заказа необходимо авторизоваться", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (lvCartItems.Items.Count == 0)
            {
                MessageBox.Show("Корзина пуста", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            NavigationService.Navigate(new CheckoutPage());
        }

        private void BtnMain_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new UserMain());
        private void BtnCart_Click(object sender, RoutedEventArgs e) { /* Уже на этой странице */ }
        private void BtnFavorites_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new FavoritesPage());
        private void BtnAccount_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new AccountPage());
        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            CurrentUser.Users = null;
            NavigationService.Navigate(new Authorization());
        }

        private void BtnUserOrders_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
