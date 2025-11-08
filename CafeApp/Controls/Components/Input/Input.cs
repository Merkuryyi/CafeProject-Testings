using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Markup.Xaml;

namespace CafeApp.Controls.Components.Input
{
    public partial class Input : UserControl
    {
        // Существующие свойства...
        public static readonly StyledProperty<string> TextProperty =
            AvaloniaProperty.Register<Input, string>(nameof(Text), "Название");

        public static readonly StyledProperty<string> ValueProperty =
            AvaloniaProperty.Register<Input, string>(nameof(Value));

        public static readonly StyledProperty<bool> IsPasswordProperty =
            AvaloniaProperty.Register<Input, bool>(nameof(IsPassword), false);

        public static readonly StyledProperty<double> InputWidthProperty =
            AvaloniaProperty.Register<Input, double>(nameof(InputWidth), defaultValue: 400);

        // Новое свойство для цвета текста
        public static readonly StyledProperty<IBrush> TextColorProperty =
            AvaloniaProperty.Register<Input, IBrush>(nameof(TextColor), 
                SolidColorBrush.Parse("#194E84")); // Значение по умолчанию

        public Input()
        {
            InitializeComponent();
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

        public double InputWidth
        {
            get => GetValue(InputWidthProperty);
            set => SetValue(InputWidthProperty, value);
        }

        // Новое свойство для цвета текста
        public IBrush TextColor
        {
            get => GetValue(TextColorProperty);
            set => SetValue(TextColorProperty, value);
        }
    }
}