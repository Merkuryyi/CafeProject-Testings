using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace CafeApp.Controls.Components.ComboBox
{
    public partial class ComboBox : UserControl
    {
        public static readonly StyledProperty<string> TextProperty =
            AvaloniaProperty.Register<ComboBox, string>(nameof(Text), "Роль:");

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
    }
}