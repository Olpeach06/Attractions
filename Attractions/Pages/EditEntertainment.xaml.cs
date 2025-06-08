using Attractions.AppData;
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

namespace Attractions.Pages
{
    public partial class EditEntertainment : Page
    {
        private Entertainment _currentEntertainment;
        private bool _isNew;

        public EditEntertainment(int? entertainmentId)
        {
            InitializeComponent();
            _currentEntertainment = new Entertainment();
            _isNew = true;
            LoadData();
        }

        public EditEntertainment(Entertainment entertainment)
        {
            InitializeComponent();
            _currentEntertainment = entertainment;
            _isNew = false;
            LoadData();
        }

        private void LoadData()
        {
            DataContext = this;
            cbCategory.ItemsSource = AppConnect.modelDB.Categories.ToList();
            cbFilter.ItemsSource = AppConnect.modelDB.Filters.ToList();
        }

        public string Title => _isNew ? "Добавление нового развлечения" : "Редактирование развлечения";
        public Entertainment CurrentEntertainment => _currentEntertainment;

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Валидация данных
                if (string.IsNullOrWhiteSpace(_currentEntertainment.Name))
                {
                    MessageBox.Show("Введите название развлечения", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (_currentEntertainment.CategoryId == 0)
                {
                    MessageBox.Show("Выберите категорию", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (_isNew)
                {
                    AppConnect.modelDB.Entertainment.Add(_currentEntertainment);
                }

                AppConnect.modelDB.SaveChanges();
                MessageBox.Show("Данные успешно сохранены", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                NavigationService.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (_isNew || _currentEntertainment.EntertainmentId == 0)
            {
                NavigationService.GoBack();
                return;
            }

            var result = MessageBox.Show("Отменить изменения?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Откатываем изменения
                AppConnect.modelDB.Entry(_currentEntertainment).Reload();
                NavigationService.GoBack();
            }
        }
    }
}
