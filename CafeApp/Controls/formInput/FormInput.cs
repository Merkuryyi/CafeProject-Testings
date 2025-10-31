using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace CafeApp.Controls
{
    public partial class FormInput : UserControl
    {
        public static readonly StyledProperty<string> TitleProperty =
            AvaloniaProperty.Register<FormInput, string>(nameof(Title), "Название");

        public FormInput()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public string Title
        {
            get => GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }
    }
}