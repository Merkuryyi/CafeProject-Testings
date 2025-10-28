using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace CafeApp.Controls
{
    public partial class CustomButton : UserControl
    {
        public string ButtonText
        {
            get => MainButton.Content?.ToString() ?? "";
            set => MainButton.Content = value;
        }

        public event RoutedEventHandler Click;

        public CustomButton()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void MainButton_Click(object sender, RoutedEventArgs e)
        {
            Click?.Invoke(this, e);
        }
    }
}