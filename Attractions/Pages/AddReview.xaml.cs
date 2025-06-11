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
    public partial class AddReview : Page
    {
        private int _entertainmentId;

        public AddReview(int entertainmentId)
        {
            InitializeComponent();
            _entertainmentId = entertainmentId;
        }

        private void SubmitReview_Click(object sender, RoutedEventArgs e)
        {
            if (RatingComboBox.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите оценку", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(CommentTextBox.Text))
            {
                MessageBox.Show("Пожалуйста, введите комментарий", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Получаем числовую часть рейтинга (первый символ в ComboBoxItem)
                var ratingText = ((ComboBoxItem)RatingComboBox.SelectedItem).Content.ToString();
                int rating = int.Parse(ratingText.Substring(0, 1));

                var newReview = new Reviews
                {
                    UserId = CurrentUser.Users.UserId,
                    EntertainmentId = _entertainmentId,
                    Rating = rating,
                    Comment = CommentTextBox.Text.Trim(),
                    ReviewDate = DateTime.Now
                };

                AppConnect.modelDB.Reviews.Add(newReview);
                AppConnect.modelDB.SaveChanges();

                MessageBox.Show("Спасибо за ваш отзыв!", "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);
                NavigationService.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении отзыва: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}