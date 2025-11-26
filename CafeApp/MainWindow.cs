using Avalonia.Controls;
using CafeApp.Controls;
using CafeApp.Controls.Components.Sidebar;
using CafeApp.Controls.Components.List;
using System.Collections.ObjectModel;
using CafeApp.Database;
using CafeApp.Models;
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
            formInputControl.LoginResult += OnLoginResult;
            
            var formEmployeeControl = this.FindControl<FormEmployee>("FormEmployeeControl");
            formEmployeeControl.SaveButtonClicked += (s, e) => LoadEmployeesData();
            
            var sidebarControl = this.FindControl<Sidebar>("SidebarControl");
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
            listControl.Items = _employees;
            listControl.Title = "Сотрудники";
        }

        private void LoadOrdersData()
        {
            _orders.Clear();
            List<ListItem> data;
            if (GetCurrentRole() == "официант")
            { data = _databaseService.GetCurrentShiftOrdersList(); }
            else
            { data = _databaseService.GetOrdersList(); }
           
            foreach (var item in data)
                _orders.Add(item);
            
            var listControl = this.FindControl<List>("OrdersListControl");
            listControl.Items = _orders;
            listControl.Title = "Список заказов";
            
        }

        private void LoadShiftsData()
        {
            _shifts.Clear();
            var data = _databaseService.GetShiftsList();
            
            foreach (var item in data)
                _shifts.Add(item);
            
            var listControl = this.FindControl<List>("ShiftsListControl");
            listControl.Items = _shifts;
            listControl.Title = "Список смен";
            
        }

        private void OnLoginResult(object? sender, string role)
        {
            if (!string.IsNullOrEmpty(role))
            {
             //   string userInfo = CurrentUser.GetUserInfo();
                
             //   string logPath = @"A:\Инженерно-техническая поддержка сопровождения ИС\debug.log";
           //     File.AppendAllText(logPath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - MAIN WINDOW: {userInfo}\n");
        
               // UpdateUserInterface();
        
                var sidebarControl = this.FindControl<Sidebar>("SidebarControl");
                sidebarControl.Role = CurrentUser.Role;
        
                var withSidebarPanel = this.FindControl<Grid>("WithSidebarPanel");
                var withoutSidebarPanel = this.FindControl<Grid>("WithoutSidebarPanel");
        
                withSidebarPanel.IsVisible = true;
                withoutSidebarPanel.IsVisible = false;
            }
        }
        
        private void UpdateUserInterface()
        {
            // Показываем информацию о пользователе
            var userInfoTextBlock = this.FindControl<TextBlock>("UserInfoTextBlock");
            userInfoTextBlock.Text = $"{CurrentUser.FullName} ({CurrentUser.Role})";
            
    
            // Настраиваем видимость элементов по роли
            var adminPanel = this.FindControl<StackPanel>("AdminPanel");
            adminPanel.IsVisible = CurrentUser.IsAdmin;
            
    
            var waiterPanel = this.FindControl<StackPanel>("WaiterPanel");
            waiterPanel.IsVisible = CurrentUser.IsWaiter;
            
    
            // Также можно обновить Sidebar
            var sidebarControl = this.FindControl<Sidebar>("SidebarControl");
              sidebarControl.Role = CurrentUser.Role;
            
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
                var orderControl = this.FindControl<CafeApp.Controls.Order>("OrderControl");
                if (orderControl != null)
                {
                    orderControl.Role = GetCurrentRole();
                    int orderId = clickedItem.Id;
                    orderControl.LoadOrderData(orderId, GetCurrentRole()); 
                    ShowControl(orderControl);
                }
            }
            else if (title.Contains("сотрудник"))
            {
                var formEmployee = this.FindControl<FormEmployee>("FormEmployeeControl");
                if (formEmployee != null)
                {
                    formEmployee.Title = "Редактирование сотрудника";
                    formEmployee.LoadEmployee(clickedItem.Id);
                    ShowControl(formEmployee);
                }
            }
            else if (title.Contains("сотрудник"))
            {
                var formEmployee = this.FindControl<FormEmployee>("FormEmployeeControl");
                if (formEmployee != null)
                {
                    formEmployee.Title = "Редактирование сотрудника";
                    ShowControl(formEmployee);
                    string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - Loading employee ID: {clickedItem.Id}\n";
                    File.AppendAllText("A:/Инженерно-техническая поддержка сопровождения ИС/debug.log", logMessage);
                }
            }
            else if (title.Contains("смен"))
            {
                string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - Loading shift ID: {clickedItem.Id}\n";
                File.AppendAllText("A:/Инженерно-техническая поддержка сопровождения ИС/debug.log", logMessage);
            }
        }
        private void OnListAddButtonClicked(object sender, EventArgs e)
        {
            var listControl = sender as List;
            var title = listControl.Title?.ToLower() ?? "";
            var currentRole = GetCurrentRole();

            if (title.Contains("заказ") && currentRole != "повар" ) //&& currentRole != "администратор"
            {
                var orderControl = this.FindControl<CafeApp.Controls.Order>("OrderControl");
                if (orderControl != null)
                {
                    orderControl.ClearForm();
                    orderControl.Title = "Новый заказ";
                    orderControl.Role = currentRole;
                    orderControl.OrderId = -1;
                    orderControl.HideComponentsOrder();
                    ShowControl(orderControl);
                }
            }
            else if (title.Contains("сотрудник"))
            {
                var formEmployee = this.FindControl<FormEmployee>("FormEmployeeControl");
                if (formEmployee != null)
                {
                    formEmployee.ClearForm();
                    formEmployee.Title = "Регистрация сотрудника";
                    ShowControl(formEmployee);
                }
            }
        }

        private void ShowControl(Control control)
        {    
            HideAllControls();
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
        
            var orderControl = this.FindControl<CafeApp.Controls.Order>("OrderControl");
            if (orderControl != null)
                orderControl.IsVisible = false;
        }
    }

    public class ListItem
    {
        public int Id { get; set; }
        public string DisplayText { get; set; } = "";
        
        public override string ToString()
        { return DisplayText; }
    }
}