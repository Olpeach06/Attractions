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
    public partial class Authorization : Page
    {
        public Authorization()
        {
            InitializeComponent();
            // Установка фокуса на поле ввода email при загрузке
            Loaded += (s, e) => tbLogin.Focus();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            // Сброс сообщения об ошибке
            tbError.Visibility = Visibility.Collapsed;

            // Проверка заполнения полей
            if (string.IsNullOrWhiteSpace(tbLogin.Text) || pbPassword.Password.Length == 0)
            {
                ShowError("Введите email и пароль");
                return;
            }

            // Поиск пользователя в БД
            var user = AppConnect.modelDB.Users
                .FirstOrDefault(u => u.Email == tbLogin.Text && u.Password == pbPassword.Password);

            if (user == null)
            {
                ShowError("Неверный email или пароль");
                return;
            }

            // Сохранение текущего пользователя
            CurrentUser.Users = user;

            // Переход на соответствующую страницу
            if (user.RoleId == 1) // Администратор
            {
                NavigationService.Navigate(new AdminMain());
            }
            else // Пользователь
            {
                NavigationService.Navigate(new UserMain());
            }
        }

        private void ShowError(string message)
        {
            tbError.Text = message;
            tbError.Visibility = Visibility.Visible;
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Registration());
        }
    }
}
