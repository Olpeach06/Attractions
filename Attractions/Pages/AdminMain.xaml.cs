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
using System.Data.Entity;
using Attractions.AppData;
using System.Collections.ObjectModel;
using Microsoft.Win32;
using System.IO;

namespace Attractions.Pages
{
    public partial class AdminMain : Page
    {
        public ObservableCollection<Entertainment> EntertainmentList { get; set; } = new ObservableCollection<Entertainment>();
        public AdminMain()
        {
            InitializeComponent();
            lvEntertainment.ItemsSource = EntertainmentList;
            LoadEntertainment();
        }

        private void LoadEntertainment()
        {
            try
            {
                var list = AppConnect.modelDB.Entertainment
                    .Include(e => e.Categories)
                    .Include(e => e.Filters)
                    .ToList();

                EntertainmentList.Clear();
                foreach (var item in list)
                    EntertainmentList.Add(item);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Навигация
        private void BtnAttractions_Click(object sender, RoutedEventArgs e)
        {
            // Уже на этой странице
        }

        private void BtnOrders_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AdminOrders());
        }

        private void BtnUsers_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AdminUsers());
        }

        // Управление развлечениями
        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new EditEntertainment((int?)null));
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (lvEntertainment.SelectedItem is Entertainment selected)
            {
                NavigationService.Navigate(new EditEntertainment(selected));
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (lvEntertainment.SelectedItem is Entertainment selected)
            {
                var result = MessageBox.Show($"Вы уверены, что хотите удалить развлечение '{selected.Name}'?",
                    "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        // Удаляем связанные записи из расписания
                        var schedules = AppConnect.modelDB.Schedule
                            .Where(s => s.EntertainmentId == selected.EntertainmentId)
                            .ToList();

                        AppConnect.modelDB.Schedule.RemoveRange(schedules);
                        AppConnect.modelDB.Entertainment.Remove(selected);
                        AppConnect.modelDB.SaveChanges();

                        LoadEntertainment();
                        MessageBox.Show("Развлечение успешно удалено", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Authorization());
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                lvEntertainment.ItemsSource = AppConnect.modelDB.Entertainment
                    .Include(ent => ent.Categories)
                    .Include(ent => ent.Filters)
                    .ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении списка: {ex.Message}", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnSessions_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AdminSchedule());
        }

        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            if (EntertainmentList == null || EntertainmentList.Count == 0)
            {
                MessageBox.Show("Нет данных для экспорта", "Экспорт", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            ExportToCsv(EntertainmentList.ToList());
        }

        private void ExportToCsv(List<Entertainment> items)
        {
            try
            {
                SaveFileDialog saveDialog = new SaveFileDialog
                {
                    Filter = "CSV файлы (*.csv)|*.csv",
                    FileName = "Развлечения.csv"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    var sb = new StringBuilder();

                    // Заголовки столбцов, разделённые точкой с запятой
                    sb.AppendLine("ID;Название;Описание;Категория;Фильтр;Цена;Длительность;Мин. возраст;Макс. участников");

                    foreach (var item in items)
                    {
                        string categoryName = item.Categories != null ? item.Categories.Name : "";
                        string filterName = item.Filters != null ? item.Filters.Name : "";

                        sb.AppendLine($"{item.EntertainmentId};{item.Name};{item.Description};{categoryName};{filterName};{item.Price};{item.Duration};{item.MinAge};{item.MaxParticipants}");
                    }

                    System.IO.File.WriteAllText(saveDialog.FileName, sb.ToString(), Encoding.UTF8);

                    MessageBox.Show("Файл успешно сохранён!", "Экспорт", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при экспорте: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}