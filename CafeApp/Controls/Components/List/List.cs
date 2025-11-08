using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CafeApp.Controls.Components.List
{
    public partial class List : UserControl
    {
        public static readonly StyledProperty<string> TextProperty =
            AvaloniaProperty.Register<List, string>(nameof(Text), "Сотрудники");

        public static readonly StyledProperty<ObservableCollection<string>> ItemsProperty =
            AvaloniaProperty.Register<List, ObservableCollection<string>>(nameof(Items));

        public List()
        {
            InitializeComponent();
            // Инициализация пустым списком
            Items = new ObservableCollection<string>();
        }

        public string Text
        {
            get => GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public ObservableCollection<string> Items
        {
            get => GetValue(ItemsProperty);
            set => SetValue(ItemsProperty, value);
        }

        // Метод для обновления данных
        public void UpdateItems(IEnumerable<string> newItems)
        {
            Items.Clear();
            foreach (var item in newItems)
            {
                Items.Add(item);
            }
        }
    }
}