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
        private readonly ObservableCollection<string> _employees = new();
        private readonly ObservableCollection<string> _orders = new();
        private readonly ObservableCollection<string> _shifts = new();
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
            var data = _databaseService.GetOrdersSimpleInfo();
            
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

        private void OnListItemClicked(object sender, object clickedItem)
        {
            var listControl = sender as List;
            if (listControl == null) return;
    
            var title = listControl.Title?.ToLower() ?? "";
    
            if (title.Contains("заказ"))
            {
                var orderControl = this.FindControl<Order>("OrderControl");
                orderControl.Title = "Редактирование заказа";
                orderControl.Role = GetCurrentRole();
                string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - Form data saved:\n" +
                                    $"rolуe: '{orderControl.Role}'\n";
                File.AppendAllText("A:/Инженерно-техническая поддержка сопровождения ИС/debug.log", logMessage);
                ShowControl(orderControl);
            }
        }

        private void OnListAddButtonClicked(object sender, EventArgs e)
        {
            var listControl = sender as List;
            if (listControl == null) return;
    
            var title = listControl.Title?.ToLower() ?? "";
    
            if (title.Contains("заказ") && GetCurrentRole() != "повар")
            {
                var orderControl = this.FindControl<Order>("OrderControl");
                orderControl.Title = "Заказ";
                orderControl.Role = GetCurrentRole();
                ShowControl(orderControl);
            }
            else if (title.Contains("сотрудник"))
            {
                // Показываем форму регистрации сотрудника
                var formEmployee = this.FindControl<FormEmployee>("FormEmployeeControl");
                ShowControl(formEmployee);
            }
            else if (title.Contains("смен"))
            {
                // Показываем форму создания смены
              //  ShowControl();
            }
        }

        private void ShowControl(Control control)
        {    
            // Скрываем все контролы
            HideAllControls();
    
            // Показываем контрол Orde
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
           // var editOrderControl = this.FindControl<EditOrder>("EditOrderControl");
           // if (editOrderControl != null)
              //  editOrderControl.IsVisible = false;
            
           
        }
    }
}