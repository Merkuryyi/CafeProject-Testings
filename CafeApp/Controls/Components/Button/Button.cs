using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;

namespace CafeApp.Controls.Components.Button
{
    public partial class Button : Avalonia.Controls.Button 
    {
        public static readonly StyledProperty<string> TextProperty =
            AvaloniaProperty.Register<Button, string>(nameof(Text), "Кнопка");

        public Button()
        { InitializeComponent(); }

        private void InitializeComponent()
        { AvaloniaXamlLoader.Load(this); }

        public string Text
        {
            get => GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }
    }
}