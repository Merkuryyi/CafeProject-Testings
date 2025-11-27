using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using System.Collections.ObjectModel;
using System.Linq;
using CafeApp.Database;
using System;
using Avalonia.Interactivity;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;

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
            
            _databaseService = new DatabaseService();
            this.DataContext = this;
            
            // Инициализация данных
            AllEmployees = new ObservableCollection<string>();
            ShiftEmployees = new ObservableCollection<ShiftEmployeeData> { new ShiftEmployeeData() };
            for (int i = 0; i < 3; i++)
            { 
                ShiftEmployees.Add(new ShiftEmployeeData()); 
            }
            EmployeeCount = ShiftEmployees.Count;
            
            // Инициализация UI
            InitializeUI();
            
            // Загружаем сотрудников из базы
            LoadAllEmployees();
        }

        private void InitializeUI()
        {
            var titleText = this.FindControl<TextBlock>("TitleText");
            if (titleText != null)
                titleText.Text = Title;

            UpdateEmployeeCountText();

            var addButton = this.FindControl<TextBlock>("AddEmployeeButton");
            var removeButton = this.FindControl<TextBlock>("RemoveEmployeeButton");
            var saveButton = this.FindControl<global::CafeApp.Controls.Components.Button.Button>("SaveButton");

            if (addButton != null) addButton.PointerPressed += OnAddEmployeeClicked;
            
            if (removeButton != null) removeButton.PointerPressed += OnRemoveEmployeeClicked;
            
            if (saveButton != null) saveButton.PointerPressed += SaveButton_Click;

            // Устанавливаем ItemsSource
            var employeesItemsControl = this.FindControl<ItemsControl>("EmployeesItemsControl");
            if (employeesItemsControl != null)
            {
                employeesItemsControl.ItemsSource = ShiftEmployees;
            }
        }

        private void OnAddEmployeeClicked(object sender, PointerPressedEventArgs e)
        {
            if (EmployeeCount < 7)
            {
                ShiftEmployees.Add(new ShiftEmployeeData());
                EmployeeCount = ShiftEmployees.Count;
                UpdateEmployeeCountText();
            }
        }

        private void OnRemoveEmployeeClicked(object sender, PointerPressedEventArgs e)
        {
            if (EmployeeCount > 4)
            {
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
                    // Форматируем строку как "Фамилия Имя Отчество - Роль"
                    string fullNameWithRole = $"{employee.Surname} {employee.Name} {employee.Patronymic}".Trim() + 
                                             $" - {employee.Role}";
                    AllEmployees.Add(fullNameWithRole);
                }

                File.AppendAllText(@"A:\debug.log", 
                    DateTime.Now.ToString() + " - Loaded " + AllEmployees.Count.ToString() + " employees with roles\n");
            }
            catch (Exception ex)
            {
                File.AppendAllText(@"A:\debug.log", 
                    DateTime.Now.ToString() + " - ERROR loading employees: " + ex.Message + "\n");
            }
        }

        // Метод для извлечения ФИО из строки "Фамилия Имя Отчество - Роль"
        private string ExtractFullName(string fullNameWithRole)
        {
            if (string.IsNullOrEmpty(fullNameWithRole))
                return "";
            
            // Убираем часть с ролью (все что после последнего "-")
            int lastDashIndex = fullNameWithRole.LastIndexOf(" - ");
            if (lastDashIndex > 0)
            {
                return fullNameWithRole.Substring(0, lastDashIndex).Trim();
            }
            
            return fullNameWithRole.Trim();
        }

        // Метод для извлечения роли из строки "Фамилия Имя Отчество - Роль"
        private string ExtractRole(string fullNameWithRole)
        {
            if (string.IsNullOrEmpty(fullNameWithRole))
                return "";
            
            int lastDashIndex = fullNameWithRole.LastIndexOf(" - ");
            if (lastDashIndex > 0)
            {
                return fullNameWithRole.Substring(lastDashIndex + 3).Trim();
            }
            
            return "";
        }

        private string GetEmployeeRole(string fullName)
        {
            try
            {
                var employees = _databaseService.GetAllEmployeesExceptAdmins();
                var employee = employees.FirstOrDefault(e => 
                    $"{e.Surname} {e.Name} {e.Patronymic}".Trim() == fullName);
                
                return employee?.Role ?? "не указана";
            }
            catch (Exception ex)
            {
                File.AppendAllText(@"A:\debug.log", 
                    DateTime.Now.ToString() + " - ERROR getting employee role: " + ex.Message + "\n");
                return "не указана";
            }
        }

        private void OnEmployeeComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender is ComboBox comboBox)
                {
                    // Находим родительский StackPanel и данные
                    var stackPanel = FindParent<StackPanel>(comboBox);
                    if (stackPanel?.DataContext is ShiftEmployeeData employeeData)
                    {
                        string selectedEmployeeWithRole = comboBox.SelectedItem?.ToString() ?? "";
                        
                        if (!string.IsNullOrEmpty(selectedEmployeeWithRole))
                        {
                            // Извлекаем ФИО и роль из выбранной строки
                            string selectedEmployee = ExtractFullName(selectedEmployeeWithRole);
                            string role = ExtractRole(selectedEmployeeWithRole);
                            
                            // Сохраняем выбранного сотрудника в Tag
                            comboBox.Tag = selectedEmployeeWithRole;
                            
                            // Обновляем данные в модели
                            employeeData.EmployeeRole = role;
                            employeeData.SelectedEmployeeName = selectedEmployee;
                            
                            // Обновляем UI
                            var roleTextBlock = stackPanel.FindControl<TextBlock>("RoleTextBlock");
                            var tableInput = stackPanel.FindControl<global::CafeApp.Controls.Components.Input.Input>("TableInput");
                            
                            if (roleTextBlock != null)
                                roleTextBlock.Text = role;
                            
                            if (tableInput != null)
                            {
                                tableInput.IsVisible = employeeData.IsWaiter;
                                if (!employeeData.IsWaiter)
                                {
                                    tableInput.Text = "";
                                    employeeData.TableNumber = "";
                                }
                            }
                            
                            File.AppendAllText(@"A:\debug.log", 
                                DateTime.Now.ToString() + " - Employee selected: " + selectedEmployee + 
                                ", Role: " + role + ", IsWaiter: " + employeeData.IsWaiter.ToString() + "\n");
                        }
                        else
                        {
                            // Сбрасываем значения при отмене выбора
                            comboBox.Tag = null;
                            employeeData.EmployeeRole = "";
                            employeeData.SelectedEmployeeName = "";
                            employeeData.TableNumber = "";
                            
                            // Обновляем UI
                            var roleTextBlock = stackPanel.FindControl<TextBlock>("RoleTextBlock");
                            var tableInput = stackPanel.FindControl<global::CafeApp.Controls.Components.Input.Input>("TableInput");
                            
                            if (roleTextBlock != null)
                                roleTextBlock.Text = "не выбрана";
                            
                            if (tableInput != null)
                            {
                                tableInput.IsVisible = false;
                                tableInput.Text = "";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText(@"A:\debug.log", 
                    DateTime.Now.ToString() + " - ERROR in OnEmployeeSelectionChanged: " + ex.Message + "\n");
            }
        }

        private T FindParent<T>(Control control) where T : Control
        {
            var parent = control.Parent;
            while (parent != null)
            {
                if (parent is T parentT)
                    return parentT;
                parent = parent.Parent;
            }
            return null;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
{
    File.AppendAllText(@"A:\Инженерно-техническая поддержка сопровождения ИС\debug.log", 
        $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - click\n");
    try
    {
        var dateInput = this.FindControl<global::CafeApp.Controls.Components.Input.Input>("DateInput");
        var startTimeInput = this.FindControl<global::CafeApp.Controls.Components.Input.Input>("StartTimeInput");
        var endTimeInput = this.FindControl<global::CafeApp.Controls.Components.Input.Input>("EndTimeInput");

        string date = dateInput?.Value?.ToString() ?? dateInput?.Text ?? "";
        string startTime = startTimeInput?.Value?.ToString() ?? startTimeInput?.Text ?? "";
        string endTime = endTimeInput?.Value?.ToString() ?? endTimeInput?.Text ?? "";

        // Детальное логирование состояния всех сотрудников
        File.AppendAllText(@"A:\Инженерно-техническая поддержка сопровождения ИС\debug.log", 
            $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - Проверка сотрудников перед сохранением:\n");
        File.AppendAllText(@"A:\Инженерно-техническая поддержка сопровождения ИС\debug.log", 
            $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - Всего сотрудников в смене: {ShiftEmployees.Count}\n");

        for (int i = 0; i < ShiftEmployees.Count; i++)
        {
            var employee = ShiftEmployees[i];
            File.AppendAllText(@"A:\Инженерно-техническая поддержка сопровождения ИС\debug.log", 
                $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - Сотрудник {i}: SelectedEmployeeName='{employee.SelectedEmployeeName}', Role='{employee.EmployeeRole}', Table='{employee.TableNumber}'\n");
        }

        // Проверяем данные сотрудников
        foreach (var employee in ShiftEmployees)
        {
            if (string.IsNullOrEmpty(employee.SelectedEmployeeName))
            {
                File.AppendAllText(@"A:\Инженерно-техническая поддержка сопровождения ИС\debug.log", 
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - не все выбраны (пустое имя)\n");
                return;
            }
        }

        File.AppendAllText(@"A:\Инженерно-техническая поддержка сопровождения ИС\debug.log", 
            $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - Все сотрудники выбраны, продолжаем сохранение\n");

        // Проверяем дату и время
        if (string.IsNullOrEmpty(date) || string.IsNullOrEmpty(startTime) || string.IsNullOrEmpty(endTime))
        {
            File.AppendAllText(@"A:\debug.log", 
                DateTime.Now.ToString() + " - ERROR: Не заполнены дата или время\n");
            return;
        }

        // Создаем смену
        bool success = CreateShiftInDatabase(date, startTime, endTime);
        File.AppendAllText(@"A:\Инженерно-техническая поддержка сопровождения ИС\debug.log", 
            $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - success: {success}\n");
        
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

                if (_databaseService.IsShiftExists(shiftDate, start, end))
                {
                    File.AppendAllText(@"A:\debug.log", 
                        DateTime.Now.ToString() + " - ERROR: Смена на указанную дату и время уже существует\n");
                    return false;
                }

                int shiftId = _databaseService.CreateShift(shiftDate, start, end);
                if (shiftId == -1)
                {
                    File.AppendAllText(@"A:\debug.log", 
                        DateTime.Now.ToString() + " - ERROR: Не удалось создать смену\n");
                    return false;
                }

                // Добавляем сотрудников в смену
                foreach (var employee in ShiftEmployees)
                {
                    if (!string.IsNullOrEmpty(employee.SelectedEmployeeName))
                    {
                        int employeeId = _databaseService.GetEmployeeIdByName(employee.SelectedEmployeeName);
                        if (employeeId != -1)
                        {
                            bool added = _databaseService.AddEmployeeToShift(shiftId, employeeId, employee.TableNumber);
                            if (!added)
                            {
                                File.AppendAllText(@"A:\debug.log", 
                                    DateTime.Now.ToString() + " - WARNING: Не удалось добавить сотрудника " + 
                                    employee.SelectedEmployeeName + " в смену\n");
                            }
                        }
                    }
                }

                File.AppendAllText(@"A:\debug.log", 
                    DateTime.Now.ToString() + " - Смена создана: ID=" + shiftId.ToString() + 
                    ", Дата: " + date + ", Время: " + startTime + "-" + endTime + 
                    ", Сотрудников: " + ShiftEmployees.Count.ToString() + "\n");

                return true;
            }
            catch (Exception ex)
            {
                File.AppendAllText(@"A:\debug.log", 
                    DateTime.Now.ToString() + " - ERROR in CreateShiftInDatabase: " + ex.Message + "\n" + ex.StackTrace + "\n");
                return false;
            }
        }

        private void UpdateEmployeeCountText()
        {
            var employeeCountText = this.FindControl<TextBlock>("EmployeeCountText");
            if (employeeCountText != null)
                employeeCountText.Text = EmployeeCount.ToString() + "/7";
        }

        public void ClearForm()
        {
            ShiftEmployees.Clear();
            for (int i = 0; i < 4; i++)
            {
                ShiftEmployees.Add(new ShiftEmployeeData());
            }
            EmployeeCount = ShiftEmployees.Count;
            
            var dateInput = this.FindControl<global::CafeApp.Controls.Components.Input.Input>("DateInput");
            var startTimeInput = this.FindControl<global::CafeApp.Controls.Components.Input.Input>("StartTimeInput");
            var endTimeInput = this.FindControl<global::CafeApp.Controls.Components.Input.Input>("EndTimeInput");
            
            if (dateInput != null) dateInput.Text = "";
            if (startTimeInput != null) startTimeInput.Text = "";
            if (endTimeInput != null) endTimeInput.Text = "";
            
            UpdateEmployeeCountText();
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
                _selectedEmployeeName = value;
                OnPropertyChanged(nameof(SelectedEmployeeName));
            }
        }

        public string EmployeeRole
        {
            get => _employeeRole;
            set
            {
                _employeeRole = value;
                OnPropertyChanged(nameof(EmployeeRole));
                OnPropertyChanged(nameof(IsWaiter));
            }
        }

        public bool IsWaiter => EmployeeRole?.ToLower() == "официант";

        public string TableNumber
        {
            get => _tableNumber;
            set
            {
                _tableNumber = value;
                OnPropertyChanged(nameof(TableNumber));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}