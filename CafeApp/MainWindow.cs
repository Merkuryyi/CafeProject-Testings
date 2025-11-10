using Avalonia.Controls;
using CafeApp.Controls;
using CafeApp.Controls.Components.Sidebar;
using System;
using CafeApp.Controls.Components.List;

namespace CafeApp
{
    public partial class MainWindow : Window
    {
        private Sidebar? _sidebarControl;
        private FormEmployee? _formEmployeeControl;
        private FormInput? _formInputControl;
        private List? _listControl;

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
    
            _listControl = this.FindControl<List>("ListControl");
    
            // Устанавливаем параметры для List контрола
            if (_listControl != null)
            {
                _listControl.Title = "Список сотрудников";
                _listControl.ListHeight = 400;
            }
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
            _listControl = this.FindControl<List>("ListControl");

            // Сначала скрываем все контролы
            if (_formEmployeeControl != null)
                _formEmployeeControl.IsVisible = false;
        
            if (_listControl != null)
                _listControl.IsVisible = false;

            // Показываем нужный контрол в зависимости от выбора
            switch (itemName)
            {
                case "RegistrationText":
                    if (_formEmployeeControl != null)
                    {
                        _formEmployeeControl.IsVisible = true;
                        Console.WriteLine("Показана форма сотрудника");
                    }
                    break;
            
                case "EmployeesText":
                    if (_listControl != null)
                    {
                        // Обновляем параметры
                        _listControl.Title = "Сотрудники";
                        _listControl.ListHeight = 450;
                        _listControl.IsVisible = true;
                        Console.WriteLine("Показан список сотрудников");
                    }
                    break;
            
                default:
                    Console.WriteLine($"Выбран пункт: {itemName}");
                    break;
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