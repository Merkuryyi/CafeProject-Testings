using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace CafeApp.Controls.Components.Input
{
    public partial class Input : UserControl
    {
        public static readonly StyledProperty<string> TextProperty =
            AvaloniaProperty.Register<Input, string>(nameof(Text), "Название");

        public static readonly StyledProperty<string> ValueProperty =
            AvaloniaProperty.Register<Input, string>(nameof(Value));

        public static readonly StyledProperty<bool> IsPasswordProperty =
            AvaloniaProperty.Register<Input, bool>(nameof(IsPassword), false);

        public Input()
        {
            InitializeComponent();
            
            // Подписываемся на событие после загрузки XAML
            this.AttachedToVisualTree += (s, e) => UpdatePasswordChar();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == IsPasswordProperty)
            {
                UpdatePasswordChar();
            }
        }

        private void UpdatePasswordChar()
        {
            var textBox = this.FindControl<TextBox>("MainInput");
            if (textBox != null)
            {
                textBox.PasswordChar = IsPassword ? '*' : '\0';
            }
        }

        public string Text
        {
            get => GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public string Value
        {
            get => GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public bool IsPassword
        {
            get => GetValue(IsPasswordProperty);
            set => SetValue(IsPasswordProperty, value);
        }
    }
}