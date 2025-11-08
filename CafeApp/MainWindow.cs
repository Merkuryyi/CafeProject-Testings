using Avalonia.Controls;
using CafeApp.Controls;
using CafeApp.Controls.Components.Sidebar;
using System;

namespace CafeApp
{
    public partial class MainWindow : Window
    {
        private Sidebar? _sidebarControl;
        private FormEmployee? _formEmployeeControl;
        private FormInput? _formInputControl;

        public MainWindow()
        {
            InitializeComponent();
            this.Opened += OnMainWindowOpened;
        }

        private void OnMainWindowOpened(object? sender, EventArgs e)
        {
            SubscribeToAuthEvents();
            SubscribeToSidebarEvents();
        }

        private void SubscribeToAuthEvents()
        {
            _formInputControl = this.FindControl<FormInput>("FormInputControl");
            _formInputControl.LoginResult += OnLoginResult;
        }

        private void SubscribeToSidebarEvents()
        {
            _sidebarControl = this.FindControl<Sidebar>("SidebarControl");
            _sidebarControl.ItemSelected += OnSidebarItemSelected;
        }

        // Теперь получаем роль вместо boolean
        private void OnLoginResult(object? sender, string role)
        {
            if (!string.IsNullOrEmpty(role))
            {
                ShowWithSidebar(role); // Передаем роль в сайдбар
            }
            else
            {
                Console.WriteLine("Ошибка входа!");
            }
        }

        // Метод для показа с сайдбаром с указанной ролью
        public void ShowWithSidebar(string role)
        {
            var withSidebarPanel = this.FindControl<Grid>("WithSidebarPanel");
            var withoutSidebarPanel = this.FindControl<Grid>("WithoutSidebarPanel");
            
            if (withSidebarPanel != null && withoutSidebarPanel != null)
            {
                // Устанавливаем роль в сайдбар
                if (_sidebarControl != null)
                {
                    _sidebarControl.Role = role;
                }
                
                withSidebarPanel.IsVisible = true;
                withoutSidebarPanel.IsVisible = false;
            }
        }

        // Остальной код без изменений...
        private void OnSidebarItemSelected(object? sender, string itemName)
        {
            _formEmployeeControl = this.FindControl<FormEmployee>("FormEmployeeControl");

            if (itemName == "RegistrationText")
            {
                _formEmployeeControl.IsVisible = true;
                Console.WriteLine("Показана форма сотрудника");
            }
            else
            {
                _formEmployeeControl.IsVisible = false;
                Console.WriteLine($"Скрыта форма сотрудника, выбран: {itemName}");
            }
        }

        public void ShowWithoutSidebar()
        {
            var withSidebarPanel = this.FindControl<Grid>("WithSidebarPanel");
            var withoutSidebarPanel = this.FindControl<Grid>("WithoutSidebarPanel");
            if (withSidebarPanel != null && withoutSidebarPanel != null)
            {
                withSidebarPanel.IsVisible = false;
                withoutSidebarPanel.IsVisible = true;
            }
        }
    }
}