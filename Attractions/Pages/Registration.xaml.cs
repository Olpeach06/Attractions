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
    public partial class Registration : Page
    {
        public Registration()
        {
            InitializeComponent();
            dpBirthDate.DisplayDateEnd = DateTime.Now; // Ограничение даты - не позже сегодня
            Loaded += (s, e) => tbFirstName.Focus(); // Фокус на первом поле при загрузке
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                tbError.Visibility = Visibility.Collapsed;

                // Валидация данных
                if (string.IsNullOrWhiteSpace(tbFirstName.Text))
                    throw new Exception("Введите имя");
                if (string.IsNullOrWhiteSpace(tbLastName.Text))
                    throw new Exception("Введите фамилию");
                if (dpBirthDate.SelectedDate == null)
                    throw new Exception("Укажите дату рождения");
                if (string.IsNullOrWhiteSpace(tbEmail.Text))
                    throw new Exception("Введите email");
                if (pbPassword.Password.Length < 6)
                    throw new Exception("Пароль должен содержать минимум 6 символов");
                if (pbPassword.Password != pbConfirmPassword.Password)
                    throw new Exception("Пароли не совпадают");

                // Проверка возраста (18+)
                var age = DateTime.Now.Year - dpBirthDate.SelectedDate.Value.Year;
                if (dpBirthDate.SelectedDate.Value > DateTime.Now.AddYears(-age)) age--;
                if (age < 18)
                    throw new Exception("Регистрация доступна только с 18 лет");

                // Проверка уникальности email
                if (AppConnect.modelDB.Users.Any(u => u.Email == tbEmail.Text))
                    throw new Exception("Пользователь с таким email уже существует");

                // Создание нового пользователя
                var newUser = new Users()
                {
                    FirstName = tbFirstName.Text,
                    LastName = tbLastName.Text,
                    MiddleName = tbMiddleName.Text,
                    BirthDate = dpBirthDate.SelectedDate.Value,
                    Email = tbEmail.Text,
                    Password = pbPassword.Password,
                    RoleId = 2 // Автоматически назначаем роль "Пользователь"
                };

                // Сохранение в БД
                AppConnect.modelDB.Users.Add(newUser);
                AppConnect.modelDB.SaveChanges();

                MessageBox.Show("Регистрация прошла успешно!", "Успех",
                              MessageBoxButton.OK, MessageBoxImage.Information);

                // Возврат на страницу авторизации
                NavigationService.Navigate(new Authorization());
            }
            catch (Exception ex)
            {
                tbError.Text = ex.Message;
                tbError.Visibility = Visibility.Visible;
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void CbShowPassword_Checked(object sender, RoutedEventArgs e)
        {
            pbPassword.Visibility = Visibility.Collapsed;
            pbConfirmPassword.Visibility = Visibility.Collapsed;

            var tbPassword = new TextBox { Text = pbPassword.Password, Tag = "Пароль" };
            var tbConfirmPassword = new TextBox { Text = pbConfirmPassword.Password, Tag = "Подтвердите пароль" };

            //// Применяем стили
            //tbPassword.Style = (Style)FindResource("MaterialDesignFloatingHintTextBox");
            //tbConfirmPassword.Style = (Style)FindResource("MaterialDesignFloatingHintTextBox");

            // Заменяем PasswordBox на TextBox
            var stackPanel = (StackPanel)pbPassword.Parent;
            var passwordIndex = stackPanel.Children.IndexOf(pbPassword);
            stackPanel.Children.Insert(passwordIndex, tbPassword);
            stackPanel.Children.Remove(pbPassword);

            var confirmIndex = stackPanel.Children.IndexOf(pbConfirmPassword);
            stackPanel.Children.Insert(confirmIndex, tbConfirmPassword);
            stackPanel.Children.Remove(pbConfirmPassword);
        }

        private void CbShowPassword_Unchecked(object sender, RoutedEventArgs e)
        {
            // Возвращаем PasswordBox обратно
            var stackPanel = (StackPanel)((CheckBox)sender).Parent;

            foreach (var child in stackPanel.Children.OfType<TextBox>().ToList())
            {
                if (child.Tag.ToString() == "Пароль")
                {
                    pbPassword.Password = child.Text;
                    stackPanel.Children.Insert(stackPanel.Children.IndexOf(child), pbPassword);
                    stackPanel.Children.Remove(child);
                }
                else if (child.Tag.ToString() == "Подтвердите пароль")
                {
                    pbConfirmPassword.Password = child.Text;
                    stackPanel.Children.Insert(stackPanel.Children.IndexOf(child), pbConfirmPassword);
                    stackPanel.Children.Remove(child);
                }
            }

            pbPassword.Visibility = Visibility.Visible;
            pbConfirmPassword.Visibility = Visibility.Visible;
        }
    }
}