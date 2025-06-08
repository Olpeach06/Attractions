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
    public partial class CartPage : Page
    {
        public CartPage()
        {
            InitializeComponent();
            LoadCartItems();
            DataContext = this;
        }

        private void LoadCartItems()
        {
            lvCart.ItemsSource = CurrentUser.Cart.Select(c => new CartItemViewModel
            {
                Schedule = c.Schedule,
                Entertainment = c.Entertainment,
                IsSelected = true
            }).ToList();

            UpdateTotal();
        }

        private void UpdateTotal()
        {
            TotalPrice = CurrentUser.Cart
                .Where(c => c.IsSelected)
                .Sum(c => c.Entertainment.Price);

            HasSelectedItems = CurrentUser.Cart.Any(c => c.IsSelected);
        }

        public decimal TotalPrice { get; set; }
        public bool HasSelectedItems { get; set; }

        private void CheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            UpdateTotal();
        }

        private void CbSelectAll_Checked(object sender, RoutedEventArgs e)
        {
            foreach (var item in lvCart.Items)
            {
                if (item is CartItemViewModel vm)
                {
                    vm.IsSelected = true;
                }
            }
            UpdateTotal();
        }

        private void CbSelectAll_Unchecked(object sender, RoutedEventArgs e)
        {
            foreach (var item in lvCart.Items)
            {
                if (item is CartItemViewModel vm)
                {
                    vm.IsSelected = false;
                }
            }
            UpdateTotal();
        }

        private void BtnRemove_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int scheduleId)
            {
                var itemToRemove = CurrentUser.Cart.FirstOrDefault(c => c.Schedule.ScheduleId == scheduleId);
                if (itemToRemove != null)
                {
                    CurrentUser.Cart.Remove(itemToRemove);
                    LoadCartItems();
                    MessageBox.Show("Товар удален из корзины", "Успешно",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void BtnAddToFavorites_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int entertainmentId)
            {
                var attraction = AppConnect.modelDB.Entertainment.FirstOrDefault(a => a.EntertainmentId == entertainmentId);

                if (attraction == null) return;

                if (CurrentUser.Favorites.Any(f => f.EntertainmentId == entertainmentId))
                {
                    MessageBox.Show("Этот аттракцион уже в вашем избранном",
                        "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    CurrentUser.Favorites.Add(attraction);
                    MessageBox.Show("Аттракцион добавлен в избранное", "Успешно",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void BtnCheckout_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = CurrentUser.Cart
                .Where(c => c.IsSelected)
                .ToList();

            if (!selectedItems.Any())
            {
                MessageBox.Show("Выберите хотя бы один товар для оформления",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            NavigationService.Navigate(new CheckoutPage(selectedItems));
        }
        private void BtnMain_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new UserMain());
        }

        private void BtnCart_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new CartPage());
        }

        private void BtnFavorites_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new FavoritesPage());
        }

        private void BtnAccount_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AccountPage());
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Authorization());
        }
    }

    public class CartItemViewModel
    {
        public Schedule Schedule { get; set; }
        public Entertainment Entertainment { get; set; }
        public bool IsSelected { get; set; }

        // Добавляем вычисляемые свойства для Status и StatusColor
        public string Status => Schedule.IsBooked ? "Забронирован" : "Доступен";
        public string StatusColor => Schedule.IsBooked ? "#FFFF0000" : "#FF00FF00";
    }
}