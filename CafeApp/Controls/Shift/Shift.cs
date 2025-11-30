using Avalonia;
using Avalonia.Controls;
using CafeApp.Controls.Components.Input;
using System.Collections.ObjectModel;
using System.Linq;
using CafeApp.Database;
using System;
using Avalonia.Interactivity;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using Avalonia.Input;

namespace CafeApp.Controls
{
    public partial class Shift : UserControl
    {
        private readonly DatabaseService _databaseService;
        public ObservableCollection<string> AllEmployees { get; set; }
        public ObservableCollection<ShiftEmployeeData> ShiftEmployees { get; set; }
        
        public static readonly StyledProperty<string> TitleProperty =
            AvaloniaProperty.Register<Shift, string>(nameof(Title), "Новая смена");
        
        public static readonly StyledProperty<int> EmployeeCountProperty =
            AvaloniaProperty.Register<Shift, int>(nameof(EmployeeCount), 0);

        private int _currentShiftId = -1;
        public int CurrentShiftId 
        { 
            get => _currentShiftId; 
            set
            {
                _currentShiftId = value;
                UpdateTitle();
            }
        }

        public string Title
        {
            get => GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public int EmployeeCount
        {
            get => GetValue(EmployeeCountProperty);
            set => SetValue(EmployeeCountProperty, value);
        }

        public event EventHandler? SaveButtonClicked;

        public Shift()
        {
            InitializeComponent();
            
            this.DataContext = this;
            _databaseService = new DatabaseService();
            AllEmployees = new ObservableCollection<string>();
            ShiftEmployees = new ObservableCollection<ShiftEmployeeData>();
            
            for (int i = 0; i < 4; i++)
            { 
                var newEmployee = new ShiftEmployeeData();
                newEmployee.PropertyChanged += OnEmployeePropertyChanged;
                ShiftEmployees.Add(newEmployee);
            }
            EmployeeCount = ShiftEmployees.Count;
            InitializeUI();
            LoadAllEmployees();
        }

        public void LoadShiftData(int shiftId)
        {
            try
            {
                CurrentShiftId = shiftId;
                var shiftInfo = _databaseService.GetShiftById(shiftId);
                
                if (shiftInfo == null || shiftInfo.ShiftId == 0)
                { return; }
                
                foreach (var employee in ShiftEmployees)
                { employee.PropertyChanged -= OnEmployeePropertyChanged; }
                ShiftEmployees.Clear();
                
                var dateInput = this.FindControl<Input>("DateInput");
                var startTimeInput = this.FindControl<Input>("StartTimeInput");
                var endTimeInput = this.FindControl<Input>("EndTimeInput");

                if (dateInput != null) 
                    dateInput.Value = shiftInfo.ShiftDate.ToString("yyyy-MM-dd");
                if (startTimeInput != null) 
                    startTimeInput.Value = shiftInfo.StartTime.ToString(@"hh\:mm");
                if (endTimeInput != null) 
                    endTimeInput.Value = shiftInfo.EndTime.ToString(@"hh\:mm");
                
                foreach (var employee in shiftInfo.Employees)
                {
                    var employeeData = new ShiftEmployeeData();
                    employeeData.PropertyChanged += OnEmployeePropertyChanged;
                    
                    string fullName = $"{employee.Surname} {employee.Name} {employee.Patronymic}".Trim();
                    employeeData.SelectedEmployeeName = fullName;
                    employeeData.SetEmployeeRole(employee.Role);
                    employeeData.TableNumber = employee.TableNumber ?? "";

                    ShiftEmployees.Add(employeeData);
                }
                
                EmployeeCount = ShiftEmployees.Count;
                UpdateEmployeeCountText();
            }
            catch (Exception ex)
            {
                File.AppendAllText(@"A:\debug.log", 
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - ERROR in LoadShiftData: {ex.Message}\n{ex.StackTrace}\n");
            }
        }

        private void UpdateTitle()
        {
            var titleText = this.FindControl<TextBlock>("TitleText");
            if (titleText != null) titleText.Text = this.Title;
        }

        private void InitializeUI()
        {
            var titleText = this.FindControl<TextBlock>("TitleText");
            if (titleText != null) titleText.Text = Title;

            UpdateEmployeeCountText();

            var addButton = this.FindControl<TextBlock>("AddEmployeeButton");
            var removeButton = this.FindControl<TextBlock>("RemoveEmployeeButton");
            var saveButton = this.FindControl<global::CafeApp.Controls.Components.Button.Button>("SaveButton");

            if (addButton != null) addButton.PointerPressed += OnAddEmployeeClicked;
            if (removeButton != null) removeButton.PointerPressed += OnRemoveEmployeeClicked;
            if (saveButton != null) saveButton.PointerPressed += SaveButton_Click;

            var employeesItemsControl = this.FindControl<ItemsControl>("EmployeesItemsControl");
            if (employeesItemsControl != null)
            { employeesItemsControl.ItemsSource = ShiftEmployees; }
        }

        private void OnAddEmployeeClicked(object sender, PointerPressedEventArgs e)
        {
            if (EmployeeCount < 7)
            {
                var newEmployee = new ShiftEmployeeData();
                newEmployee.PropertyChanged += OnEmployeePropertyChanged;
                ShiftEmployees.Add(newEmployee);
                EmployeeCount = ShiftEmployees.Count;
                UpdateEmployeeCountText();
            }
        }

        private void OnRemoveEmployeeClicked(object sender, PointerPressedEventArgs e)
        {
            if (EmployeeCount > 4)
            {
                var lastEmployee = ShiftEmployees.Last();
                lastEmployee.PropertyChanged -= OnEmployeePropertyChanged;
                ShiftEmployees.RemoveAt(ShiftEmployees.Count - 1);
                EmployeeCount = ShiftEmployees.Count;
                UpdateEmployeeCountText();
            }
        }

        private void LoadAllEmployees()
        {
            try
            {
                var employees = _databaseService.GetAllEmployeesExceptAdmins();
                AllEmployees.Clear();
                foreach (var employee in employees)
                {
                    string fullName = $"{employee.Surname} {employee.Name} {employee.Patronymic}".Trim();
                    AllEmployees.Add(fullName);
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText(@"A:\debug.log", 
                    DateTime.Now.ToString() + " - ERROR loading employees: " + ex.Message + "\n");
            }
        }

        private string GetEmployeeRoleByFullName(string fullName)
        {
            try
            {
                var employees = _databaseService.GetAllEmployeesExceptAdmins();
                var employee = employees.FirstOrDefault(e => 
                    $"{e.Surname} {e.Name} {e.Patronymic}".Trim() == fullName);
        
                return employee?.Role ?? "";
            }
            catch (Exception ex)
            { return ""; }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dateInput = this.FindControl<Input>("DateInput");
                var startTimeInput = this.FindControl<Input>("StartTimeInput");
                var endTimeInput = this.FindControl<Input>("EndTimeInput");

                string date = dateInput?.Value?.ToString() ?? dateInput?.Text ?? "";
                string startTime = startTimeInput?.Value?.ToString() ?? startTimeInput?.Text ?? "";
                string endTime = endTimeInput?.Value?.ToString() ?? endTimeInput?.Text ?? "";
                
                foreach (var employee in ShiftEmployees)
                {
                    if (string.IsNullOrEmpty(employee.SelectedEmployeeName))
                    { return; }
                }
                
                if (string.IsNullOrEmpty(date) || string.IsNullOrEmpty(startTime) || string.IsNullOrEmpty(endTime))
                { return; }

                bool success;
                
                if (CurrentShiftId > 0)
                { success = UpdateShiftInDatabase(CurrentShiftId, date, startTime, endTime); }
                else
                { success = CreateShiftInDatabase(date, startTime, endTime); }
                if (success)
                {
                    ClearForm();
                    SaveButtonClicked?.Invoke(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText(@"A:\Инженерно-техническая поддержка сопровождения ИС\debug.log", 
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - err: {ex.Message}\n{ex.StackTrace}\n");
            }
        }

        private bool CreateShiftInDatabase(string date, string startTime, string endTime)
        {
            try
            {
                DateTime shiftDate = DateTime.Parse(date);
                TimeSpan start = TimeSpan.Parse(startTime);
                TimeSpan end = TimeSpan.Parse(endTime);

                bool shiftExists = _databaseService.IsShiftExists(shiftDate, start, end);
                File.AppendAllText(@"A:\debug.log", 
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - Shift exists check: {shiftExists}\n");

                if (shiftExists)
                {
                    File.AppendAllText(@"A:\debug.log", 
                        DateTime.Now.ToString() + " - ERROR: Смена на указанную дату и время уже существует\n");
                    return false;
                }
                
                int shiftId = _databaseService.CreateShift(shiftDate, start, end);
                if (shiftId == -1)
                { return false; }
                
            
                foreach (var employee in ShiftEmployees)
                {
                    if (!string.IsNullOrEmpty(employee.SelectedEmployeeName))
                    {
                        int employeeId = _databaseService.GetEmployeeIdByName(employee.SelectedEmployeeName);
                        if (employeeId != -1)
                        {
                            bool added = _databaseService.AddEmployeeToShift(shiftId, employeeId);
                           
                            if (added && employee.IsWaiter && !string.IsNullOrEmpty(employee.TableNumber))
                            {
                                File.AppendAllText(@"A:\debug.log", 
                                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - Employee is waiter with table: {employee.TableNumber}\n");

                                if (int.TryParse(employee.TableNumber, out int tableNumber))
                                {
                                    bool tableAssigned = _databaseService.AssignTableToWaiter(shiftId, employeeId, tableNumber, shiftDate);
                                }
                                else
                                {
                                    File.AppendAllText(@"A:\debug.log", 
                                        DateTime.Now.ToString() + " - ERROR: Неверный формат номера столика: " + 
                                        employee.TableNumber + "\n");
                                }
                            }
                        }
                        else
                        {
                            File.AppendAllText(@"A:\debug.log", 
                                DateTime.Now.ToString() + " - ERROR: Не найден ID сотрудника: " + 
                                employee.SelectedEmployeeName + "\n");
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            { return false; }
        }

        private bool UpdateShiftInDatabase(int shiftId, string date, string startTime, string endTime)
        {
            try
            {
                DateTime shiftDate = DateTime.Parse(date);
                TimeSpan start = TimeSpan.Parse(startTime);
                TimeSpan end = TimeSpan.Parse(endTime);
                bool shiftUpdated = _databaseService.UpdateShift(shiftId, shiftDate, start, end);
                File.AppendAllText(@"A:\debug.log", 
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - UpdateShift result: {shiftUpdated}\n");

                if (!shiftUpdated)
                { return false; }
                
                bool employeesCleared = _databaseService.ClearShiftEmployees(shiftId);
                
                int successCount = 0;
                foreach (var employee in ShiftEmployees)
                {
                    if (!string.IsNullOrEmpty(employee.SelectedEmployeeName))
                    {
                        int employeeId = _databaseService.GetEmployeeIdByName(employee.SelectedEmployeeName);
                        if (employeeId != -1)
                        {
                            bool added = _databaseService.AddEmployeeToShift(shiftId, employeeId);
                            if (added)
                            {
                                successCount++;
                                
                                if (employee.IsWaiter && !string.IsNullOrEmpty(employee.TableNumber))
                                {
                                    if (int.TryParse(employee.TableNumber, out int tableNumber))
                                    {
                                        _databaseService.ClearWaiterTableAssignments(shiftId, employeeId);
                                        bool tableAssigned = _databaseService.AssignTableToWaiter(shiftId, employeeId, tableNumber, shiftDate);
                                    }
                                }
                            }
                        }
                    }
                }
                return successCount > 0;
            }
            catch (Exception ex)
            { return false; }
        }

        private void UpdateEmployeeCountText()
        {
            var employeeCountText = this.FindControl<TextBlock>("EmployeeCountText");
            if (employeeCountText != null)
                employeeCountText.Text = EmployeeCount.ToString() + "/7";
        }

        public void ClearForm()
        {
            CurrentShiftId = -1;
            foreach (var employee in ShiftEmployees)
            { employee.PropertyChanged -= OnEmployeePropertyChanged; }
            
            ShiftEmployees.Clear();
            
            for (int i = 0; i < 4; i++)
            {
                var newEmployee = new ShiftEmployeeData();
                newEmployee.PropertyChanged += OnEmployeePropertyChanged;
                ShiftEmployees.Add(newEmployee);
            }
            
            var dateInput = this.FindControl<Input>("DateInput");
            var startTimeInput = this.FindControl<Input>("StartTimeInput");
            var endTimeInput = this.FindControl<Input>("EndTimeInput");

            if (dateInput != null) dateInput.Value = "";
            if (startTimeInput != null) startTimeInput.Value = "";
            if (endTimeInput != null) endTimeInput.Value = "";

            EmployeeCount = ShiftEmployees.Count;
            UpdateEmployeeCountText();
            UpdateTitle();
        }

        private void OnEmployeePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ShiftEmployeeData.SelectedEmployeeName))
            {
                var employeeData = (ShiftEmployeeData)sender;
                if (!string.IsNullOrEmpty(employeeData.SelectedEmployeeName))
                {
                    string role = GetEmployeeRoleByFullName(employeeData.SelectedEmployeeName);
                    employeeData.SetEmployeeRole(role);
                }
                else
                { employeeData.SetEmployeeRole(""); }
            }
        }
    }
    
    public class ShiftEmployeeData : INotifyPropertyChanged
    {
        private string _selectedEmployeeName = "";
        private string _employeeRole = "";
        private string _tableNumber = "";

        public string SelectedEmployeeName
        {
            get => _selectedEmployeeName;
            set
            {
                if (_selectedEmployeeName != value)
                {
                    _selectedEmployeeName = value;
                    OnPropertyChanged(nameof(SelectedEmployeeName));
                }
            }
        }

        public string EmployeeRole
        {
            get => _employeeRole;
            set
            {
                if (_employeeRole != value)
                {
                    _employeeRole = value;
                    OnPropertyChanged(nameof(EmployeeRole));
                    OnPropertyChanged(nameof(IsWaiter));
                }
            }
        }

        public bool IsWaiter => EmployeeRole?.ToLower() == "официант";

        public string TableNumber
        {
            get => _tableNumber;
            set
            {
                if (_tableNumber != value)
                {
                    _tableNumber = value;
                    OnPropertyChanged(nameof(TableNumber));
                }
            }
        }
        
        public void SetEmployeeRole(string role)
        { EmployeeRole = role; }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
    }
}