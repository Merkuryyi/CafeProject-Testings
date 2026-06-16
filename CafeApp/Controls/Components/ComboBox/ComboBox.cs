using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Collections;
using System;

namespace CafeApp.Controls.Components.ComboBox
{
    public partial class ComboBox : UserControl
    {
        public static readonly StyledProperty<string> TextProperty =
            AvaloniaProperty.Register<ComboBox, string>(nameof(Text), "Роль:");
        
        public static readonly StyledProperty<double> ComboBoxWidthProperty =
            AvaloniaProperty.Register<ComboBox, double>(nameof(ComboBoxWidth), defaultValue: 400);

        public static readonly StyledProperty<IEnumerable> ItemsSourceProperty =
            AvaloniaProperty.Register<ComboBox, IEnumerable>(nameof(ItemsSource));

        public static readonly StyledProperty<object> SelectedItemProperty =
            AvaloniaProperty.Register<ComboBox, object>(nameof(SelectedItem));

        public static readonly StyledProperty<int> SelectedIndexProperty =
            AvaloniaProperty.Register<ComboBox, int>(nameof(SelectedIndex), -1);

        public event EventHandler<SelectionChangedEventArgs>? SelectionChanged;

        public ComboBox()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            var innerComboBox = this.FindControl<Avalonia.Controls.ComboBox>("MainComboBox");
            if (innerComboBox != null)
            {
                innerComboBox.SelectionChanged += InnerComboBox_SelectionChanged;
            }
        }

        private void InnerComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            SelectionChanged?.Invoke(this, e);
        }

        public string Text
        {
            get => GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public double ComboBoxWidth
        {
            get => GetValue(ComboBoxWidthProperty);
            set => SetValue(ComboBoxWidthProperty, value);
        }

        public IEnumerable ItemsSource
        {
            get => GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public object SelectedItem
        {
            get => GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        public int SelectedIndex
        {
            get => GetValue(SelectedIndexProperty);
            set => SetValue(SelectedIndexProperty, value);
        }
    }
}