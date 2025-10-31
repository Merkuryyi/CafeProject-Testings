using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace CafeApp.Controls
{
    public partial class FormInput : UserControl
    {
        public static readonly StyledProperty<string> TitleProperty =
            AvaloniaProperty.Register<FormInput, string>(nameof(Title), "Название");

        public static readonly StyledProperty<bool> ShowRoleSelectorProperty =
            AvaloniaProperty.Register<FormInput, bool>(nameof(ShowRoleSelector), true);

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

        public bool ShowRoleSelector
        {
            get => GetValue(ShowRoleSelectorProperty);
            set => SetValue(ShowRoleSelectorProperty, value);
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == TitleProperty)
            {
                // Автоматически скрываем ComboBox когда Title = "Авторизация"
                ShowRoleSelector = change.NewValue?.ToString() != "Авторизация";
            }
        }
    }
}