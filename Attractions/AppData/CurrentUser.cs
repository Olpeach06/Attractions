using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace Attractions.AppData
{
    public static class CurrentUser
    {
        public static Users Users { get; set; }
        public static List<Entertainment> Favorites { get; } = new List<Entertainment>();
        public static BindingList<CartItem> Cart { get; } = new BindingList<CartItem>();
    }

    public class CartItem : INotifyPropertyChanged
    {
        public Schedule Schedule { get; set; }
        public Entertainment Entertainment { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}