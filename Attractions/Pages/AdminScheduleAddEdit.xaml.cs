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
    public partial class AdminScheduleAddEdit : Page
    {
        private Schedule _currentSchedule;

        public string Title => _currentSchedule.ScheduleId == 0 ? "Добавление сеанса" : "Редактирование сеанса";

        public AdminScheduleAddEdit(Schedule schedule)
        {
            InitializeComponent();
            _currentSchedule = schedule ?? new Schedule();
            DataContext = this;
            LoadEntertainment();
            LoadData();
        }

        private void LoadEntertainment()
        {
            cbEntertainment.ItemsSource = AppConnect.modelDB.Entertainment.ToList();
        }

        private void LoadData()
        {
            if (_currentSchedule.ScheduleId != 0)
            {
                cbEntertainment.SelectedValue = _currentSchedule.EntertainmentId;
                dpStartDate.SelectedDate = _currentSchedule.StartTime.Date;
                tbStartTime.Text = _currentSchedule.StartTime.ToString("HH:mm");
                tbDuration.Text = _currentSchedule.EndTime.ToString();
                tbMaxParticipants.Text = _currentSchedule.MaxParticipants.ToString();
            }
            else
            {
                // Значения по умолчанию для нового сеанса
                dpStartDate.SelectedDate = DateTime.Today;
                tbDuration.Text = "60";
                tbMaxParticipants.Text = "10";
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput()) return;

            try
            {
                // Парсим время начала
                var timeParts = tbStartTime.Text.Split(':');
                var hours = int.Parse(timeParts[0]);
                var minutes = int.Parse(timeParts[1]);

                // Создаем DateTime
                var startDate = dpStartDate.SelectedDate.Value;
                var startTime = new DateTime(
                    startDate.Year, startDate.Month, startDate.Day,
                    hours, minutes, 0);

                // Заполняем данные
                _currentSchedule.EntertainmentId = (int)cbEntertainment.SelectedValue;
                _currentSchedule.StartTime = startTime;
                //_currentSchedule.EndTime = int.Parse(tbDuration.Text);
                _currentSchedule.EndTime = startTime.AddMinutes(int.Parse(tbDuration.Text));
                _currentSchedule.MaxParticipants = int.Parse(tbMaxParticipants.Text);
                _currentSchedule.CurrentParticipants = 0;

                // Сохраняем в БД
                if (_currentSchedule.ScheduleId == 0)
                {
                    AppConnect.modelDB.Schedule.Add(_currentSchedule);
                }

                AppConnect.modelDB.SaveChanges();
                MessageBox.Show("Сеанс успешно сохранен", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                NavigationService?.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении сеанса: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidateInput()
        {
            if (cbEntertainment.SelectedItem == null)
            {
                MessageBox.Show("Выберите развлечение", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!dpStartDate.SelectedDate.HasValue)
            {
                MessageBox.Show("Укажите дату начала", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!TimeSpan.TryParse(tbStartTime.Text, out _))
            {
                MessageBox.Show("Укажите корректное время начала (HH:mm)", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!int.TryParse(tbDuration.Text, out int duration) || duration <= 0)
            {
                MessageBox.Show("Укажите корректную длительность (положительное число)", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!int.TryParse(tbMaxParticipants.Text, out int max) || max <= 0)
            {
                MessageBox.Show("Укажите корректное количество участников (положительное число)", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }
    }
}
