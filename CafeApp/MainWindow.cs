using Avalonia.Controls;
using CafeApp.Controls;
using CafeApp.Controls.Components.Sidebar;
using CafeApp.Controls.Components.List;
using System.Collections.ObjectModel;
using CafeApp.Database;
using System;
using System.Collections.Generic;
using System.IO;

namespace CafeApp
{
    public partial class MainWindow : Window
    {
        private readonly ObservableCollection<ListItem> _employees = new();
        private readonly ObservableCollection<ListItem> _orders = new();
        private readonly ObservableCollection<ListItem> _shifts = new();
        private readonly DatabaseService _databaseService = new();

        public MainWindow()
        {
            InitializeComponent();
            this.Opened += OnMainWindowOpened;
        }

        private void OnMainWindowOpened(object? sender, System.EventArgs e)
        {
            SubscribeToEvents();
            LoadEmployeesData();
        }

        private void SubscribeToEvents()
        {
            var formInputControl = this.FindControl<FormInput>("FormInputControl");
            if (formInputControl != null)
                formInputControl.LoginResult += OnLoginResult;
            
            var formEmployeeControl = this.FindControl<FormEmployee>("FormEmployeeControl");
            if (formEmployeeControl != null)
                formEmployeeControl.SaveButtonClicked += (s, e) => LoadEmployeesData();
            
            var sidebarControl = this.FindControl<Sidebar>("SidebarControl");
            if (sidebarControl != null)
                sidebarControl.ItemSelected += OnSidebarItemSelected;

            SubscribeToListEvents();
        }

        private void LoadEmployeesData()
        {
            _employees.Clear();
            var data = _databaseService.GetEmployeesList();
            
            foreach (var item in data)
                _employees.Add(item);
            
            var listControl = this.FindControl<List>("EmployeesListControl");
            if (listControl != null)
            {
                listControl.Items = _employees;
                listControl.Title = "Сотрудники";
            }
        }

        private void LoadOrdersData()
        {
            _orders.Clear();
            var data = _databaseService.GetOrdersList();
            
            foreach (var item in data)
                _orders.Add(item);
            
            var listControl = this.FindControl<List>("OrdersListControl");
            if (listControl != null)
            {
                listControl.Items = _orders;
                listControl.Title = "Список заказов";
            }
        }

        private void LoadShiftsData()
        {
            _shifts.Clear();
            var data = _databaseService.GetShiftsList();
            
            foreach (var item in data)
                _shifts.Add(item);
            
            var listControl = this.FindControl<List>("ShiftsListControl");
            if (listControl != null)
            {
                listControl.Items = _shifts;
                listControl.Title = "Список смен";
            }
        }

        private void OnLoginResult(object? sender, string role)
        {
            if (!string.IsNullOrEmpty(role))
            {
                var sidebarControl = this.FindControl<Sidebar>("SidebarControl");
                if (sidebarControl != null)
                    sidebarControl.Role = role;
                
                var withSidebarPanel = this.FindControl<Grid>("WithSidebarPanel");
                var withoutSidebarPanel = this.FindControl<Grid>("WithoutSidebarPanel");
                
                if (withSidebarPanel != null && withoutSidebarPanel != null)
                {
                    withSidebarPanel.IsVisible = true;
                    withoutSidebarPanel.IsVisible = false;
                }
            }
        }

        private void OnSidebarItemSelected(object? sender, string itemName)
        {
            HideAllControls();
            switch (itemName)
            {
                case "RegistrationText":
                    var formEmployee = this.FindControl<FormEmployee>("FormEmployeeControl");
                    if (formEmployee != null)
                        formEmployee.IsVisible = true;
                    break;
                    
                case "EmployeesText":
                    LoadEmployeesData();
                    var employeesList = this.FindControl<List>("EmployeesListControl");
                    if (employeesList != null)
                        employeesList.IsVisible = true;
                    break;
                    
                case "OrdersText":
                    LoadOrdersData();
                    var ordersList = this.FindControl<List>("OrdersListControl");
                    if (ordersList != null)
                        ordersList.IsVisible = true;
                    break;
                    
                case "ShiftsText":
                    LoadShiftsData();
                    var shiftsList = this.FindControl<List>("ShiftsListControl");
                    if (shiftsList != null)
                        shiftsList.IsVisible = true;
                    break;
            }
        }

        private void SubscribeToListEvents()
        {
            var employeesList = this.FindControl<List>("EmployeesListControl");
            if (employeesList != null)
            {
                employeesList.ItemClicked += OnListItemClicked;
                employeesList.AddButtonClicked += OnListAddButtonClicked;
            }
        
            var ordersList = this.FindControl<List>("OrdersListControl");
            if (ordersList != null)
            {
                ordersList.ItemClicked += OnListItemClicked;
                ordersList.AddButtonClicked += OnListAddButtonClicked;
            }
        
            var shiftsList = this.FindControl<List>("ShiftsListControl");
            if (shiftsList != null)
            {
                shiftsList.ItemClicked += OnListItemClicked;
                shiftsList.AddButtonClicked += OnListAddButtonClicked;
            }
        }

        private string GetCurrentRole()
        {
            var sidebarControl = this.FindControl<Sidebar>("SidebarControl");
            return sidebarControl?.Role ?? "";
        }

        private void OnListItemClicked(object sender, ListItem clickedItem)
        {
            var listControl = sender as List;
            if (listControl == null) return;

            var title = listControl.Title?.ToLower() ?? "";

            if (title.Contains("заказ"))
            {
                   var orderControl = this.FindControl<Order>("OrderControl");
               
                    orderControl.Title = "Редактирование заказа";
                    orderControl.Role = GetCurrentRole();
                    // Загружаем данные заказа по ID

                    int orderId = clickedItem.Id;
                 
                    
                    string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - Loading order ID: {orderId}\n" +
                                       $"Role: {GetCurrentRole()}\n";
                    File.AppendAllText("A:/Инженерно-техническая поддержка сопровождения ИС/debug.log", logMessage);
                    orderControl.LoadOrderData(orderId, GetCurrentRole());
                    ShowControl(orderControl);
                
            }
            else if (title.Contains("сотрудник"))
            {
               
                var formEmployee = this.FindControl<FormEmployee>("FormEmployeeControl");
                if (formEmployee != null)
                {
                    // TODO: Реализовать метод загрузки данных сотрудника по ID
                    // formEmployee.LoadEmployeeData(clickedItem.Id);
                    formEmployee.Title = "Редактирование сотрудника";
                    ShowControl(formEmployee);
                    
                    string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - Loading employee ID: {clickedItem.Id}\n";
                    File.AppendAllText("A:/Инженерно-техническая поддержка сопровождения ИС/debug.log", logMessage);
                }
            }
            else if (title.Contains("смен"))
            {
                // Обработка клика на смене
                // TODO: Реализовать форму редактирования смены
                string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - Loading shift ID: {clickedItem.Id}\n";
                File.AppendAllText("A:/Инженерно-техническая поддержка сопровождения ИС/debug.log", logMessage);
                
                // Показываем сообщение, что функционал в разработке
                var dialog = new Window()
                {
                    Title = "Информация",
                    Content = new TextBlock { Text = "Редактирование смен в разработке" },
                    SizeToContent = SizeToContent.WidthAndHeight,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };
                dialog.ShowDialog(this);
            }
        }

        private void OnListAddButtonClicked(object sender, EventArgs e)
        {
            var listControl = sender as List;
            if (listControl == null) return;

            var title = listControl.Title?.ToLower() ?? "";
            var currentRole = GetCurrentRole();

            if (title.Contains("заказ") && currentRole != "повар")
            {
                var orderControl = this.FindControl<Order>("OrderControl");
                if (orderControl != null)
                {
                    orderControl.ClearForm();
                    orderControl.Title = "Новый заказ";
                    orderControl.Role = currentRole;
                    orderControl.OrderId = -1; // Сбрасываем ID для нового заказа
                    ShowControl(orderControl);
                    
                    string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - Creating new order\n" +
                                       $"Role: {currentRole}\n";
                    File.AppendAllText("A:/Инженерно-техническая поддержка сопровождения ИС/debug.log", logMessage);
                }
            }
            else if (title.Contains("сотрудник") && currentRole == "администратор")
            {
                // Показываем форму регистрации сотрудника
                var formEmployee = this.FindControl<FormEmployee>("FormEmployeeControl");
                if (formEmployee != null)
                {
                    formEmployee.ClearForm();
                    formEmployee.Title = "Регистрация сотрудника";
                    ShowControl(formEmployee);
                }
            }
            else if (title.Contains("смен") && currentRole == "администратор")
            {
                // TODO: Реализовать форму создания смены
                string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - Creating new shift\n";
                File.AppendAllText("A:/Инженерно-техническая поддержка сопровождения ИС/debug.log", logMessage);
                
                // Показываем сообщение, что функционал в разработке
                var dialog = new Window()
                {
                    Title = "Информация",
                    Content = new TextBlock { Text = "Создание смен в разработке" },
                    SizeToContent = SizeToContent.WidthAndHeight,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };
                dialog.ShowDialog(this);
            }
            else
            {
                // Сообщение о недостаточных правах
                string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - Access denied for role: {currentRole}\n";
                File.AppendAllText("A:/Инженерно-техническая поддержка сопровождения ИС/debug.log", logMessage);
                
                var dialog = new Window()
                {
                    Title = "Ошибка доступа",
                    Content = new TextBlock { Text = "Недостаточно прав для выполнения этой операции" },
                    SizeToContent = SizeToContent.WidthAndHeight,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };
                dialog.ShowDialog(this);
            }
        }

        private void ShowControl(Control control)
        {    
            if (control == null) return;
            
            // Скрываем все контролы
            HideAllControls();
    
            // Показываем нужный контрол
            control.IsVisible = true;
        }

        private void HideAllControls()
        {
            var formEmployee = this.FindControl<FormEmployee>("FormEmployeeControl");
            if (formEmployee != null)
                formEmployee.IsVisible = false;
        
            var employeesList = this.FindControl<List>("EmployeesListControl");
            if (employeesList != null)
                employeesList.IsVisible = false;
        
            var ordersList = this.FindControl<List>("OrdersListControl");
            if (ordersList != null)
                ordersList.IsVisible = false;
        
            var shiftsList = this.FindControl<List>("ShiftsListControl");
            if (shiftsList != null)
                shiftsList.IsVisible = false;
        
            var orderControl = this.FindControl<Order>("OrderControl");
            if (orderControl != null)
                orderControl.IsVisible = false;
        }
    }

    // Класс ListItem должен быть в том же namespace
    public class ListItem
    {
        public int Id { get; set; }
        public string DisplayText { get; set; } = "";
        
        public override string ToString()
        {
            return DisplayText;
        }
    }
}