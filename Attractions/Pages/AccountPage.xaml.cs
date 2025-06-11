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
using System.Text.RegularExpressions;
using Attractions.AppData;
using System.ComponentModel;
using System.Drawing;
using ZXing;
using System.IO;
using System.Net.NetworkInformation;

namespace Attractions.Pages
{
    public partial class AccountPage : Page, INotifyPropertyChanged
    {
        private Users _currentUser;

        public Users CurrentUserData
        {
            get => _currentUser;
            set
            {
                _currentUser = value;
                OnPropertyChanged(nameof(CurrentUserData));
            }
        }

        public AccountPage()
        {
            InitializeComponent();
            DataContext = this;
            LoadUserData();
        }

        private void LoadUserData()
        {
            // Получаем текущего пользователя из базы данных
            if (CurrentUser.Users != null)
            {
                CurrentUserData = AppConnect.modelDB.Users
                    .FirstOrDefault(u => u.UserId == CurrentUser.Users.UserId);

                // Обновляем привязки данных
                OnPropertyChanged(nameof(CurrentUserData));
            }
        }

        private void BtnSaveChanges_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentUserData == null) return;

            // Обновляем данные пользователя
            CurrentUserData.FirstName = txtFirstName.Text;
            CurrentUserData.LastName = txtLastName.Text;
            CurrentUserData.MiddleName = string.IsNullOrEmpty(txtMiddleName.Text) ? null : txtMiddleName.Text;
            CurrentUserData.Email = txtEmail.Text;

            // Если введен новый пароль, обновляем его
            if (!string.IsNullOrEmpty(txtPassword.Password))
            {
                CurrentUserData.Password = txtPassword.Password;
            }

            try
            {
                // Сохраняем изменения в базе данных
                AppConnect.modelDB.SaveChanges();

                // Обновляем текущего пользователя в CurrentUser
                CurrentUser.Users = CurrentUserData;

                MessageBox.Show("Изменения сохранены успешно!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnTopUp_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentUserData == null) return;

            if (decimal.TryParse(txtAmount.Text, out decimal amount) && amount > 0)
            {
                // Обновляем баланс
                CurrentUserData.Balance = (CurrentUserData.Balance ?? 0) + amount;

                try
                {
                    // Сохраняем изменения в базе данных
                    AppConnect.modelDB.SaveChanges();

                    // Обновляем отображение баланса
                    OnPropertyChanged(nameof(CurrentUserData));
                    txtAmount.Text = "0";

                    MessageBox.Show($"Баланс успешно пополнен на {amount:F2} руб.!", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при пополнении баланса: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Введите корректную сумму для пополнения!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Разрешаем вводить только цифры в поле суммы
        private void TxtAmount_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9,]+");
            e.Handled = regex.IsMatch(e.Text);
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
            NavigationService?.Navigate(new FavoritesPage());
        }

        private void BtnAccount_Click(object sender, RoutedEventArgs e)
        {
            // Уже на странице аккаунта
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            // Очищаем текущего пользователя
            CurrentUser.Users = null;
            CurrentUser.Cart.Clear();
            CurrentUser.Favorites.Clear();

            // Возвращаемся на страницу входа
            NavigationService?.Navigate(new Authorization());
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void BtnUserOrders_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new UserOrders());
        }


        private void ButtonQr(object sender, RoutedEventArgs e)
        {
            var writer = new BarcodeWriter
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new ZXing.Common.EncodingOptions
                {
                    Width = 300,
                    Height = 300
                }
            };
            var result = writer.Write(@"https://online.sberbank.ru/CSAFront/index.do");
            var bitmap = new BitmapImage();
            using (var memoryStream = new MemoryStream())
            {
                result.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                memoryStream.Position = 0;
                bitmap.BeginInit();
                bitmap.StreamSource = memoryStream;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();
            }
            imgQr.Source = bitmap;
        }
    }
}
