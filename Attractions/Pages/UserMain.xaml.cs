using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public partial class UserMain : Page
    {
        private ObservableCollection<Entertainment> _allAttractions;
        private ObservableCollection<Categories> _categories;
        private ObservableCollection<Filters> _filters;

        public UserMain()
        {
            InitializeComponent();
            Loaded += UserMain_Loaded;
        }

        private void UserMain_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Загрузка данных из БД
                _allAttractions = new ObservableCollection<Entertainment>(AppConnect.modelDB.Entertainment.ToList());
                lvAttractions.ItemsSource = _allAttractions;

                // Заполнение категорий (добавляем "Все категории" в начало)
                _categories = new ObservableCollection<Categories>(
                    new[] { new Categories { CategoryId = 0, Name = "Все категории" } }
                        .Concat(AppConnect.modelDB.Categories.ToList())
                );
                cbCategories.ItemsSource = _categories;
                cbCategories.DisplayMemberPath = "Name";
                cbCategories.SelectedIndex = 0; // Выбираем "Все категории" по умолчанию

                // Заполнение фильтров (аналогично)
                _filters = new ObservableCollection<Filters>(
                    new[] { new Filters { FilterId = 0, Name = "Все фильтры" } }
                        .Concat(AppConnect.modelDB.Filters.ToList())
                );
                cbFilters.ItemsSource = _filters;
                cbFilters.DisplayMemberPath = "Name";
                cbFilters.SelectedIndex = 0; // Выбираем "Все фильтры" по умолчанию

                // Заполнение сортировки
                cbSort.ItemsSource = new List<string>
                {
                    "По умолчанию",
                    "Сначала дешевле",
                    "Сначала дороже"
                };
                cbSort.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplyFilters()
        {
            if (_allAttractions == null) return;

            var filtered = _allAttractions.AsEnumerable();

            // Фильтрация по категории
            if (cbCategories.SelectedItem is Categories selectedCategory && selectedCategory.CategoryId != 0)
            {
                filtered = filtered.Where(a => a.CategoryId == selectedCategory.CategoryId);
            }

            // Фильтрация по фильтру
            if (cbFilters.SelectedItem is Filters selectedFilter && selectedFilter.FilterId != 0)
            {
                filtered = filtered.Where(a => a.FilterId == selectedFilter.FilterId);
            }

            // Фильтрация по цене
            if (decimal.TryParse(tbMinPrice.Text, out decimal minPrice) && minPrice > 0)
            {
                filtered = filtered.Where(a => a.Price >= minPrice);
            }

            if (decimal.TryParse(tbMaxPrice.Text, out decimal maxPrice) && maxPrice > 0)
            {
                filtered = filtered.Where(a => a.Price <= maxPrice);
            }

            // Поиск по названию
            if (!string.IsNullOrWhiteSpace(tbSearch.Text))
            {
                filtered = filtered.Where(a => a.Name.IndexOf(tbSearch.Text, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            // Сортировка
            switch (cbSort.SelectedItem?.ToString())
            {
                case "Сначала дешевле":
                    filtered = filtered.OrderBy(a => a.Price);
                    break;
                case "Сначала дороже":
                    filtered = filtered.OrderByDescending(a => a.Price);
                    break;
            }

            lvAttractions.ItemsSource = filtered.ToList(); // Обновляем ListView
        }

        private void CbCategories_SelectionChanged(object sender, SelectionChangedEventArgs e) => ApplyFilters();
        private void CbFilters_SelectionChanged(object sender, SelectionChangedEventArgs e) => ApplyFilters();
        private void CbSort_SelectionChanged(object sender, SelectionChangedEventArgs e) => ApplyFilters();
        private void PriceFilter_TextChanged(object sender, TextChangedEventArgs e) => ApplyFilters();
        private void TbSearch_TextChanged(object sender, TextChangedEventArgs e) => ApplyFilters();

        // Навигация
        private void BtnMain_Click(object sender, RoutedEventArgs e)
            => NavigationService.Navigate(new UserMain());

        private void BtnCart_Click(object sender, RoutedEventArgs e)
            => NavigationService.Navigate(new CartPage());
        private void BtnFavorites_Click(object sender, RoutedEventArgs e)
            => NavigationService.Navigate(new FavoritesPage());

        private void BtnAccount_Click(object sender, RoutedEventArgs e)
            => NavigationService.Navigate(new AccountPage());

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            CurrentUser.Users = null;
            NavigationService.Navigate(new Authorization());
        }

        private void BtnDetails_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int attractionId)
            {
                NavigationService.Navigate(new AttractionDetailsPage(attractionId));
            }
        }

        private void BtnAddToCart_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int attractionId)
            {
                try
                {
                    // Логика добавления в корзину
                    var attraction = _allAttractions.FirstOrDefault(a => a.EntertainmentId == attractionId);
                    if (attraction != null)
                    {
                        // Здесь добавьте логику добавления в корзину
                        MessageBox.Show($"{attraction.Name} добавлен в корзину", "Успех",
                                      MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}