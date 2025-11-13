using Avalonia.Controls;
using CafeApp.Controls;
using CafeApp.Controls.Components.Sidebar;
using System;
using CafeApp.Controls.Components.List;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CafeApp.Database;

namespace CafeApp
{
    public partial class MainWindow : Window
    {
        private Sidebar? _sidebarControl;
        private FormEmployee? _formEmployeeControl;
        private FormInput? _formInputControl;
        private List? _employeesListControl;
        private List? _ordersListControl;
        private ObservableCollection<string> _employees;
        private ObservableCollection<string> _orders;
		private DatabaseService _databaseService;

        public MainWindow()
        {
            InitializeComponent();
            _employees = new ObservableCollection<string>();
            _orders = new ObservableCollection<string>();
			_databaseService = new DatabaseService();
            this.Opened += OnMainWindowOpened;
        }

        private void OnMainWindowOpened(object? sender, EventArgs e)
        {
            SubscribeToAuthEvents();
            SubscribeToSidebarEvents();
            LoadEmployeesData();
        }

        private void LoadEmployeesData()
		{
    		try
    		{
				_employees.Clear();
        		var employeesFromDb = _databaseService.GetEmployeesList();
        
       			foreach (var employee in employeesFromDb)
       		 	{
            		_employees.Add(employee);
        		}
       			if (_employeesListControl != null)
        		{
            		_employeesListControl.Items = _employees;
        		}
    		}
    		catch (Exception ex)
    		{
        		Console.WriteLine($"Ошибка загрузки сотрудников: {ex.Message}");
    		}
		}

        private void LoadOrdersData()
        {
            try
            {
                _orders.Clear();
                var ordersFromDb = _databaseService.GetOrdersSimpleInfo();
                
                foreach (var order in ordersFromDb)
                {
                    _orders.Add(order);
                }
                
                if (_ordersListControl != null)
                {
                    _ordersListControl.Items = _orders;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки заказов: {ex.Message}");
            }
        }

        private void SubscribeToAuthEvents()
        {
            _formInputControl = this.FindControl<FormInput>("FormInputControl");
            _formInputControl.LoginResult += OnLoginResult;
            
            _formEmployeeControl = this.FindControl<FormEmployee>("FormEmployeeControl");
            if (_formEmployeeControl != null)
            {
                _formEmployeeControl.SaveButtonClicked += OnEmployeeSaved;
            }
        }

        private void SubscribeToSidebarEvents()
        {
            _sidebarControl = this.FindControl<Sidebar>("SidebarControl");
            _sidebarControl.ItemSelected += OnSidebarItemSelected;
    
            _employeesListControl = this.FindControl<List>("EmployeesListControl");
            _ordersListControl = this.FindControl<List>("OrdersListControl");

            if (_employeesListControl != null)
            {
                _employeesListControl.Title = "Список сотрудников";
                _employeesListControl.ListHeight = 400;
                _employeesListControl.Items = _employees; 
            }

            if (_ordersListControl != null)
            {
                _ordersListControl.Title = "Список заказов";
                _ordersListControl.ListHeight = 400;
                _ordersListControl.Items = _orders;
            }
        }

        // Обработчик сохранения сотрудника
        private void OnEmployeeSaved(object? sender, EventArgs e)
        {
            LoadEmployeesData();
        }

        // Теперь получаем роль вместо boolean
        private void OnLoginResult(object? sender, string role)
        {
            if (!string.IsNullOrEmpty(role))
            {
                ShowWithSidebar(role);
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

        private void OnSidebarItemSelected(object? sender, string itemName)
        {
            _formEmployeeControl = this.FindControl<FormEmployee>("FormEmployeeControl");
            _employeesListControl = this.FindControl<List>("EmployeesListControl");
            _ordersListControl = this.FindControl<List>("OrdersListControl");

            // Сначала скрываем все контролы
            if (_formEmployeeControl != null)
                _formEmployeeControl.IsVisible = false;
        
            if (_employeesListControl != null)
                _employeesListControl.IsVisible = false;
            
            if (_ordersListControl != null)
                _ordersListControl.IsVisible = false;

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
                    if (_employeesListControl != null)
                    {
                        // Обновляем параметры
                        _employeesListControl.Title = "Сотрудники";
                        _employeesListControl.ListHeight = 450;
                        _employeesListControl.Items = _employees;
                        _employeesListControl.IsVisible = true;
                        Console.WriteLine("Показан список сотрудников");
                    }
                    break;
            
                case "OrdersText":
                    if (_ordersListControl != null)
                    {
                        // Загружаем актуальные данные заказов
                        LoadOrdersData();
                        
                        // Обновляем параметры
                        _ordersListControl.Title = "Список заказов";
                        _ordersListControl.ListHeight = 450;
                        _ordersListControl.Items = _orders;
                        _ordersListControl.IsVisible = true;
                        Console.WriteLine("Показан список заказов");
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