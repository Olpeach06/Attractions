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
    public partial class AdminOrders : Page
    {
        public AdminOrders()
        {
            InitializeComponent();

            // Initialize date pickers with default values
            dpFromDate.SelectedDate = DateTime.Today.AddDays(-7);
            dpToDate.SelectedDate = DateTime.Today;

            // Load statuses for filter
            cbStatusFilter.ItemsSource = AppConnect.modelDB.Statuses.ToList();
            cbStatusFilter.DisplayMemberPath = "Name";
            cbStatusFilter.SelectedValuePath = "StatusId";
            cbStatusFilter.SelectedIndex = 0;

            // Загружаем статусы из базы данных и назначаем их в тег DataGrid
            var statuses = AppConnect.modelDB.Statuses.ToList();
            dgOrders.Tag = new { Statuses = statuses };

            LoadOrders();
        }
        // Загрузка заказов
        private void LoadOrders()
        {
            try
            {
                var ordersQuery = AppConnect.modelDB.Orders
                    .Include("Users")
                    .OrderByDescending(o => o.OrderDate)
                    .AsQueryable();

                // Если установлен фильтр по статусу
                if (cbStatusFilter.SelectedIndex > 0 && cbStatusFilter.SelectedItem is Statuses selectedStatus)
                {
                    ordersQuery = ordersQuery.Where(o => o.Statuses.StatusId == selectedStatus.StatusId);
                }

                // Если установлены фильтры по датам
                if (dpFromDate.SelectedDate != null)
                {
                    ordersQuery = ordersQuery.Where(o => o.OrderDate >= dpFromDate.SelectedDate);
                }

                if (dpToDate.SelectedDate != null)
                {
                    var endDate = dpToDate.SelectedDate.Value.AddDays(1);
                    ordersQuery = ordersQuery.Where(o => o.OrderDate < endDate);
                }

                // Присваиваем итоговый набор данных DataGrid
                dgOrders.ItemsSource = ordersQuery.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке заказов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Кнопка "Обновить"
        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadOrders();
        }

        // Кнопка "Удалить"
        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgOrders.SelectedItems.Count > 0)
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить выбранные заказы?", "Подтверждение удаления",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    foreach (var item in dgOrders.SelectedItems.Cast<Orders>())
                    {
                        AppConnect.modelDB.Orders.Remove(item);
                    }

                    try
                    {
                        AppConnect.modelDB.SaveChanges();
                        LoadOrders();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении заказа: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        // Обновление статуса заказа
        private void BtnUpdateStatus_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int orderId &&
                ((FrameworkElement)button.Parent).FindName("cbStatus") is ComboBox comboBox &&
                comboBox.SelectedItem is Statuses selectedStatus)
            {
                try
                {
                    var order = AppConnect.modelDB.Orders.FirstOrDefault(o => o.OrderId == orderId);
                    if (order != null)
                    {
                        order.Statuses = selectedStatus;
                        AppConnect.modelDB.SaveChanges();
                        LoadOrders();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при изменении статуса: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // Событие при смене фильтра по статусу
        private void CbStatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadOrders();
        }

        // Событие при применении фильтров по датам
        private void ApplyFilters_Click(object sender, RoutedEventArgs e)
        {
            LoadOrders();
        }

        // Событие сброса фильтров
        private void ResetFilters_Click(object sender, RoutedEventArgs e)
        {
            cbStatusFilter.SelectedIndex = 0;
            dpFromDate.SelectedDate = DateTime.Today.AddDays(-7);
            dpToDate.SelectedDate = DateTime.Today;
            LoadOrders();
        }

        // Переход на главную страницу
        private void BtnMain_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AdminMain());
        }

        // Переход на страницу развлечение
        private void BtnAttractions_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AdminMain());
        }

        // Переход на страницу сеансов
        private void BtnSessions_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AdminSchedule());
        }

        // Мы уже на странице заказов
        private void BtnOrders_Click(object sender, RoutedEventArgs e)
        {
            // Оставлено пустым, так как мы уже на странице заказов
        }

        // Переход на страницу пользователей
        private void BtnUsers_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AdminUsers());
        }

        // Выход из системы
        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            CurrentUser.Users = null;
            NavigationService.Navigate(new Authorization());
        }
    }
}