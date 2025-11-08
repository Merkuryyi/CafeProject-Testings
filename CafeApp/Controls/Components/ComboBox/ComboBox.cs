using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Collections;

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

        public ComboBox()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
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
    }
}