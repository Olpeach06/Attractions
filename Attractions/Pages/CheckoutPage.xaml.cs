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
    public partial class CheckoutPage : Page
    {
        public CheckoutPage()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                if (CurrentUser.Users == null) return;

                // Баланс
                txtBalance.Text = $"Баланс: {CurrentUser.Users.Balance:C}";

                // Загрузка элементов корзины
                var cart = AppConnect.modelDB.Cart
                    .Include("CartItems.Schedule.Entertainment")
                    .FirstOrDefault(c => c.UserId == CurrentUser.Users.UserId);

                if (cart != null)
                {
                    lvOrderItems.ItemsSource = cart.CartItems.ToList();
                    txtTotal.Text = $"Итого: {cart.CartItems.Sum(ci => ci.Quantity * ci.Schedule.Entertainment.Price):C}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentUser.Users == null) return;

            try
            {
                var cart = AppConnect.modelDB.Cart
                    .Include("CartItems.Schedule.Entertainment")
                    .FirstOrDefault(c => c.UserId == CurrentUser.Users.UserId);

                if (cart == null || cart.CartItems.Count == 0)
                {
                    MessageBox.Show("Корзина пуста", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                decimal total = cart.CartItems.Sum(ci => ci.Quantity * ci.Schedule.Entertainment.Price);

                // Проверка баланса
                if (CurrentUser.Users.Balance < total)
                {
                    MessageBox.Show("Недостаточно средств на балансе.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Списание средств
                CurrentUser.Users.Balance -= total;

                // Создание заказа
                var order = new Orders
                {
                    UserId = CurrentUser.Users.UserId,
                    TotalAmount = total,
                    StatusId = 1,
                    PaymentMethodId = 1, // если в БД обязательно, можно оставить фиксированное значение
                    OrderDate = DateTime.Now
                };

                AppConnect.modelDB.Orders.Add(order);
                AppConnect.modelDB.SaveChanges();

                // Добавление элементов заказа
                foreach (var item in cart.CartItems)
                {
                    var orderItem = new OrderItems
                    {
                        OrderId = order.OrderId,
                        ScheduleId = item.ScheduleId,
                        Quantity = item.Quantity,
                        Price = item.Schedule.Entertainment.Price
                    };
                    AppConnect.modelDB.OrderItems.Add(orderItem);
                }

                // Очистка корзины
                AppConnect.modelDB.CartItems.RemoveRange(cart.CartItems);
                AppConnect.modelDB.SaveChanges();

                MessageBox.Show("Заказ успешно оформлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                NavigationService.Navigate(new UserMain());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при оформлении заказа: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnMain_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new UserMain());
        private void BtnCart_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new CartPage());
        private void BtnFavorites_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new FavoritesPage());
        private void BtnAccount_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new AccountPage());
        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            CurrentUser.Users = null;
            NavigationService.Navigate(new Authorization());
        }
    }
}