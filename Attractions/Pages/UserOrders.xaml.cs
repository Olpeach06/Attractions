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
using Microsoft.Win32;
using System.IO;

namespace Attractions.Pages
{
    public partial class UserOrders : Page
    {
        public UserOrders()
        {
            InitializeComponent();
            LoadOrders();
        }

        private void LoadOrders()
        {
            var userId = CurrentUser.Users.UserId;

            var ordersFromDb = AppConnect.modelDB.Orders
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            var formattedOrders = ordersFromDb.Select(o => new
            {
                o.OrderId,
                o.OrderDate,
                o.TotalAmount,
                Status = o.Statuses.Name, // Изменено здесь - берем название статуса
                OrderItems = o.OrderItems.Select(oi => new
                {
                    EntertainmentName = oi.Schedule.Entertainment.Name,
                    ScheduleTime = oi.Schedule.StartTime.ToString("dd.MM.yyyy HH:mm"),
                    oi.Quantity,
                    oi.Price
                }).ToList()
            }).ToList();

            if (formattedOrders.Any())
            {
                OrdersItemsControl.ItemsSource = formattedOrders;
            }
            else
            {
                TxtNoOrders.Visibility = Visibility.Visible;
            }
        }


        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Authorization());
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

        private void BtnUserOrders_Click(object sender, RoutedEventArgs e)
        {
            
        }
        private void LeaveReviewClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int orderId)
            {
                // Получаем первый аттракцион из заказа
                var order = AppConnect.modelDB.Orders.FirstOrDefault(o => o.OrderId == orderId);
                if (order != null && order.OrderItems.Any())
                {
                    int entertainmentId = order.OrderItems.First().Schedule.EntertainmentId;
                    NavigationService.Navigate(new AddReview(entertainmentId));
                }
                else
                {
                    MessageBox.Show("Не удалось найти информацию о заказе", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}