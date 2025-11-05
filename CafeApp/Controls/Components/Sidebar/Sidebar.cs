using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using System;

namespace CafeApp.Controls.Components.Sidebar
{
    public partial class Sidebar : UserControl
    {
        public static readonly StyledProperty<string> TextProperty =
            AvaloniaProperty.Register<Sidebar, string>(nameof(Text), "Роль:");

        public static readonly StyledProperty<string> RoleProperty =
            AvaloniaProperty.Register<Sidebar, string>(nameof(Role), "Администратор");

        private Border _activeBorder;
        private TextBlock _activeTextBlock;

        public Sidebar()
        {
            InitializeComponent();
            UpdateVisibility();
            
            // Делаем "Регистрацию" активной после полной загрузки
            this.AttachedToVisualTree += (s, e) => SelectRegistration();
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
                // При изменении роли также выбираем регистрацию, если она доступна
                SelectRegistration();
            }
        }

        private void SelectRegistration()
        {
            // Если роль администратор и регистрация доступна, выбираем её
            if (Role == "Администратор")
            {
                    SelectItem("RegistrationText");
            }
        }

        private void UpdateVisibility()
        {
            var registrationBorder = this.FindControl<Border>("RegistrationBorder");
            var employeesBorder = this.FindControl<Border>("EmployeesBorder");
            var ordersBorder = this.FindControl<Border>("OrdersBorder");
            var shiftsBorder = this.FindControl<Border>("ShiftsBorder");
            var reportsBorder = this.FindControl<Border>("ReportsBorder");
            var orderBorder = this.FindControl<Border>("OrderBorder");

            if (registrationBorder == null) return;

            bool isAdmin = Role == "Администратор";
            bool isCook = Role == "Повар";
            bool isWaiter = Role == "Официант";

            registrationBorder.IsVisible = isAdmin;
            employeesBorder.IsVisible = isAdmin;
            ordersBorder.IsVisible = isWaiter || isAdmin || isCook;
            reportsBorder.IsVisible = isWaiter || isAdmin;
            shiftsBorder.IsVisible = isAdmin;
            orderBorder.IsVisible = isWaiter || isCook;
        }

        private void TextBlock_PointerPressed(object sender, PointerPressedEventArgs e)
        {
            if (sender is TextBlock clickedTextBlock)
            {
                var clickedBorder = clickedTextBlock.Parent as Border;
                
                if (clickedBorder != null)
                {
                    // Сбрасываем стиль у предыдущего активного элемента
                    if (_activeBorder != null && _activeTextBlock != null)
                    {
                        _activeBorder.Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.Transparent);
                        _activeTextBlock.Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#194E84"));
                    }

                    // Устанавливаем новый активный элемент
                    _activeBorder = clickedBorder;
                    _activeTextBlock = clickedTextBlock;
                    
                    _activeBorder.Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#194E84"));
                    _activeTextBlock.Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.White);

                    // Выводим в консоль информацию о нажатом элементе
                    Console.WriteLine($"Нажата кнопка: {clickedTextBlock.Text}");

                    // Можно также использовать switch для разных действий
                    switch (clickedTextBlock.Name)
                    {
                        case "RegistrationText":
                            Console.WriteLine("Выполняется действие: Регистрация");
                            break;
                        case "EmployeesText":
                            Console.WriteLine("Выполняется действие: Сотрудники");
                            break;
                        case "OrdersText":
                            Console.WriteLine("Выполняется действие: Заказы");
                            break;
                        case "ShiftsText":
                            Console.WriteLine("Выполняется действие: Смены");
                            break;
                        case "ReportsText":
                            Console.WriteLine("Выполняется действие: Отчеты");
                            break;
                        case "OrderText":
                            Console.WriteLine("Выполняется действие: Ордер");
                            break;
                    }

                    // Вызываем событие выбора элемента
                    ItemSelected?.Invoke(this, clickedTextBlock.Name);
                }
            }
        }

        // Метод для сброса выделения (если понадобится извне)
        public void ResetSelection()
        {
            if (_activeBorder != null && _activeTextBlock != null)
            {
                _activeBorder.Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.Transparent);
                _activeTextBlock.Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#194E84"));
                _activeBorder = null;
                _activeTextBlock = null;
            }
        }

        // Метод для программного выбора элемента
        public void SelectItem(string itemName)
        {
            var textBlock = this.FindControl<TextBlock>(itemName);
            if (textBlock != null && textBlock.IsVisible)
            {
                TextBlock_PointerPressed(textBlock, null);
            }
        }

        // Событие для уведомления о выборе элемента
        public event EventHandler<string> ItemSelected;

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