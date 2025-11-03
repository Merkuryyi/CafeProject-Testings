using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace CafeApp.Controls.Components.Sidebar
{
    public partial class Sidebar : UserControl
    {
        public static readonly StyledProperty<string> TextProperty =
            AvaloniaProperty.Register<Sidebar, string>(nameof(Text), "Роль:");

        public static readonly StyledProperty<string> RoleProperty =
            AvaloniaProperty.Register<Sidebar, string>(nameof(Role), "Администратор");

        public Sidebar()
        {
            InitializeComponent();
            
            // Вызываем когда контрол полностью инициализирован
            UpdateVisibility();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == RoleProperty)
            {
                UpdateVisibility();
            }
        }

        private void UpdateVisibility()
        {
            var registrationText = this.FindControl<TextBlock>("RegistrationText");
            var employeesText = this.FindControl<TextBlock>("EmployeesText");
            var ordersText = this.FindControl<TextBlock>("OrdersText");
            var shiftsText = this.FindControl<TextBlock>("ShiftsText");
            var reportsText = this.FindControl<TextBlock>("ReportsText");
            var orderText = this.FindControl<TextBlock>("OrderText");

            if (registrationText == null) return;

            bool isAdmin = Role == "Администратор";
            bool isCook = Role == "Повар";
            bool isWaiter = Role == "Официант";

            registrationText.IsVisible = isAdmin;
            employeesText.IsVisible = isAdmin;
            ordersText.IsVisible = isWaiter || isAdmin || isCook;
            reportsText.IsVisible = isWaiter || isAdmin;
            shiftsText.IsVisible = isAdmin;
            orderText.IsVisible = isWaiter || isCook;
        }

        public string Text
        {
            get => GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public string Role
        {
            get => GetValue(RoleProperty);
            set => SetValue(RoleProperty, value);
        }
    }
}