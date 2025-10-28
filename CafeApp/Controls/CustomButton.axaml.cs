using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;

namespace CafeApp.Controls
{
    public partial class CustomButton : UserControl
    {
        public string ButtonText
        {
            get => MainButton.Content?.ToString() ?? "";
            set => MainButton.Content = value;
        }
        public event EventHandler Click;

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