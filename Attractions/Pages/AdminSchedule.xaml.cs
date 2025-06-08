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
    public partial class AdminSchedule : Page
    {
        public AdminSchedule()
        {
            InitializeComponent();
            LoadSchedule();
        }

        private void LoadSchedule()
        {
            var schedule = AppConnect.modelDB.Schedule
                .Include("Entertainment")
                .OrderBy(s => s.StartTime)
                .ToList();
            lvSchedule.ItemsSource = schedule;
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AdminScheduleAddEdit(new Schedule()));
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (lvSchedule.SelectedItem is Schedule selectedSchedule)
            {
                NavigationService?.Navigate(new AdminScheduleAddEdit(selectedSchedule));
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (lvSchedule.SelectedItem is Schedule selectedSchedule)
            {
                var result = MessageBox.Show($"Удалить сеанс {selectedSchedule.Entertainment.Name} на {selectedSchedule.StartTime:dd.MM.yyyy HH:mm}?",
                    "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        AppConnect.modelDB.Schedule.Remove(selectedSchedule);
                        AppConnect.modelDB.SaveChanges();
                        LoadSchedule();
                        MessageBox.Show("Сеанс успешно удален", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (System.Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении сеанса: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadSchedule();
        }

        private void BtnAttractions_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AdminMain());
        }

        private void BtnSchedule_Click(object sender, RoutedEventArgs e)
        {
            // Уже на странице сеансов
        }

        private void BtnOrders_Click(object sender, RoutedEventArgs e)
        {
            // NavigationService?.Navigate(new AdminOrders());
        }

        private void BtnUsers_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AdminUsers());
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            CurrentUser.Users = null;
            NavigationService?.Navigate(new Authorization());
        }
    }
}