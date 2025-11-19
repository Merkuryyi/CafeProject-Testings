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
            this.AttachedToVisualTree += (s, e) => SelectFirstAvailableItem();
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
                SelectFirstAvailableItem();
            }
        }

        private void SelectFirstAvailableItem()
        {
            string roleLower = Role?.ToLower() ?? "";
            
            if (roleLower == "администратор")
            {
                SelectItem("RegistrationText");
            }
            else if (roleLower == "повар")
            {
                    SelectItem("OrdersText");
            }
            else if (roleLower == "официант")
            {
                SelectItem("OrdersText");
            }
        }

        private bool IsItemVisible(string itemName)
        {
            var textBlock = this.FindControl<TextBlock>(itemName);
            return textBlock != null && textBlock.IsVisible;
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

            bool isAdmin = Role == "Администратор" || Role == "администратор";
            bool isCook = Role == "Повар" || Role == "повар";
            bool isWaiter = Role == "Официант" || Role == "официант";

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
                    if (_activeBorder != null && _activeTextBlock != null)
                    {
                        _activeBorder.Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.Transparent);
                        _activeTextBlock.Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#194E84"));
                    }
                    _activeBorder = clickedBorder;
                    _activeTextBlock = clickedTextBlock;
                    
                    _activeBorder.Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#194E84"));
                    _activeTextBlock.Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.White);

                    ItemSelected?.Invoke(this, clickedTextBlock.Name);
                }
            }
        }


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
        public void SelectItem(string itemName)
        {
            var textBlock = this.FindControl<TextBlock>(itemName);
            if (textBlock != null && textBlock.IsVisible)
            {
                TextBlock_PointerPressed(textBlock, null);
            }
        }
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