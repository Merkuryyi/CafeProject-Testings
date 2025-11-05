using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace CafeApp.Controls
{
    public partial class FormEmployee : UserControl
    {
        public static readonly StyledProperty<string> TitleProperty =
            AvaloniaProperty.Register<FormEmployee, string>(nameof(Title), "Название");

        public FormEmployee()
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