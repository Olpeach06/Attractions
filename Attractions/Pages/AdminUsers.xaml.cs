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
    public partial class AdminUsers : Page
    {
        public System.Collections.Generic.List<Roles> Roles { get; set; }

        public AdminUsers()
        {
            InitializeComponent();
            LoadData();
            DataContext = this;
        }

        private void LoadData()
        {
            // Загрузка пользователей
            var users = AppConnect.modelDB.Users.ToList();
            lvUsers.ItemsSource = users;

            // Загрузка ролей
            Roles = AppConnect.modelDB.Roles.ToList();
        }

        private void RoleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.RemovedItems.Count > 0)
            {
                var comboBox = (ComboBox)sender;
                int userId = (int)comboBox.Tag;
                int newRoleId = ((Roles)e.AddedItems[0]).RoleId;

                var user = AppConnect.modelDB.Users.FirstOrDefault(u => u.UserId == userId);
                if (user != null)
                {
                    user.RoleId = newRoleId;
                    try
                    {
                        AppConnect.modelDB.SaveChanges();
                        MessageBox.Show("Роль пользователя успешно изменена", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (System.Exception ex)
                    {
                        MessageBox.Show($"Ошибка при изменении роли: {ex.Message}", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (lvUsers.SelectedItem is Users selectedUser)
            {
                var result = MessageBox.Show($"Вы уверены, что хотите удалить пользователя {selectedUser.LastName} {selectedUser.FirstName}?",
                    "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        // Удаляем связанные записи
                        var cart = AppConnect.modelDB.Cart.FirstOrDefault(c => c.UserId == selectedUser.UserId);
                        if (cart != null) AppConnect.modelDB.Cart.Remove(cart);

                        var favorites = AppConnect.modelDB.Favorites.Where(f => f.UserId == selectedUser.UserId).ToList();
                        AppConnect.modelDB.Favorites.RemoveRange(favorites);

                        // Удаляем пользователя
                        AppConnect.modelDB.Users.Remove(selectedUser);
                        AppConnect.modelDB.SaveChanges();

                        // Обновляем список
                        LoadData();

                        MessageBox.Show("Пользователь успешно удален", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (System.Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении пользователя: {ex.Message}", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void BtnAttractions_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AdminMain());
        }

        private void BtnOrders_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AdminOrders());
        }

        private void BtnUsers_Click(object sender, RoutedEventArgs e)
        {
            // Уже на странице пользователей
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            CurrentUser.Users = null;
            NavigationService?.Navigate(new Authorization());
        }

        private void BtnSessions_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AdminSchedule());
        }
    }
}