// Controls/Components/Table/Table.axaml.cs
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Collections;
using System.Collections.Generic;

namespace CafeApp.Controls.Components.Table
{
    public partial class Table : UserControl
    {
        public static readonly StyledProperty<string> TitleProperty =
            AvaloniaProperty.Register<Table, string>(nameof(Title), "Таблица");

        public static readonly StyledProperty<IEnumerable> ColumnsProperty =
            AvaloniaProperty.Register<Table, IEnumerable>(nameof(Columns));

        public static readonly StyledProperty<IEnumerable> ItemsSourceProperty =
            AvaloniaProperty.Register<Table, IEnumerable>(nameof(ItemsSource));

        public static readonly StyledProperty<double> RowHeightProperty =
            AvaloniaProperty.Register<Table, double>(nameof(RowHeight), 40);

        public static readonly StyledProperty<double> HeaderHeightProperty =
            AvaloniaProperty.Register<Table, double>(nameof(HeaderHeight), 50);

        public string Title
        {
            get => GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public IEnumerable Columns
        {
            get => GetValue(ColumnsProperty);
            set => SetValue(ColumnsProperty, value);
        }

        public IEnumerable ItemsSource
        {
            get => GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public double RowHeight
        {
            get => GetValue(RowHeightProperty);
            set => SetValue(RowHeightProperty, value);
        }

        public double HeaderHeight
        {
            get => GetValue(HeaderHeightProperty);
            set => SetValue(HeaderHeightProperty, value);
        }

        public Table()
        {
            InitializeComponent();
        }
    }
}