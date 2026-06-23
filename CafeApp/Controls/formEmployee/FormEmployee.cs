using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;
using System;
using System.Collections;
using System.Collections.Generic; 
using System.Collections.ObjectModel;
using System.IO;
using CafeApp.Controls.Components.Input;
using CafeApp.Database;

namespace CafeApp.Controls
{
    
    public partial class FormEmployee : UserControl
    {
        public string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "debug.log");
        private string _photoFilePath = "";
        private string _contractFilePath = "";
        private DatabaseService _databaseService;
        public List<string> Roles { get; set; }
        public List<string> Status { get; set; }

        public static readonly StyledProperty<string> TitleProperty =
            AvaloniaProperty.Register<FormEmployee, string>(nameof(Title), "Регистрация сотрудника");
        public static readonly StyledProperty<int> EmployeeIdProperty =
            AvaloniaProperty.Register<FormEmployee, int>(nameof(EmployeeId), -1);
        
        public int EmployeeId
        {
            get => GetValue(EmployeeIdProperty);
            set => SetValue(EmployeeIdProperty, value);
        }
        public string Title
        {
            get => GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }
        public FormEmployee()
        {
            InitializeComponent();
            Roles = new List<string> 
            {
                "Администратор", 
                "Официант", 
                "Повар",
            };
            Status = new List<string> 
            { 
                "Работает", 
                "Уволен",      
            };
        
            DataContext = this;
            _databaseService = new DatabaseService();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            ResetComponents();
        }

        public ObservableCollection<string> Employees { get; } = new ObservableCollection<string>();
        public event EventHandler? SaveButtonClicked;

        public void ResetComponents()
        {
            File.AppendAllText(logPath,
                $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - " +
                $"Employee loaded successfully: {Title}\n");
            EmployeeId = -1;
            var loginInput = this.FindControl<Input>("UsernameInput");
            var passwordInput = this.FindControl<Input>("PasswordInput");
            var roleComboBox = this.FindControl<global::CafeApp.Controls.Components.ComboBox.ComboBox>("RoleComboBox");
            var addPhotoEmployee = this.FindControl<TextBlock>("AddPhotoEmployee");
            
            var nameInput = this.FindControl<Input>("NameInput");
            var surnameInput = this.FindControl<Input>("SurnameInput");
            var patronymicInput = this.FindControl<Input>("PatronymicInput");
            var addEmploymentContract = this.FindControl<TextBlock>("AddEmploymentContract");
            
            var loginPanel = this.FindControl<StackPanel>("LoginPanel");
            var passwordPanel = this.FindControl<StackPanel>("PasswordPanel");
            var rolePanel = this.FindControl<StackPanel>("RolePanel");
            
            var namePanel = this.FindControl<StackPanel>("NamePanel");
            var surnamePanel = this.FindControl<StackPanel>("SurnamePanel");
            var patronymicPanel = this.FindControl<StackPanel>("PatronymicPanel");
            
            var statusComboBox = this.FindControl<global::CafeApp.Controls.Components.ComboBox.ComboBox>("StatusComboBox");
            if (Title == "Регистрация сотрудника")
            {
                loginPanel.IsVisible = false;
                passwordPanel.IsVisible = false;
                rolePanel.IsVisible = false;
                namePanel.IsVisible = false;
                surnamePanel.IsVisible = false;
                patronymicPanel.IsVisible = false;
                statusComboBox.IsVisible = false;
                
                loginInput.IsVisible = true;
                passwordInput.IsVisible = true;
                roleComboBox.IsVisible = true;
                addPhotoEmployee.IsVisible = true;
                nameInput.IsVisible = true;
                surnameInput.IsVisible = true;
                patronymicInput.IsVisible = true;
                addEmploymentContract.IsVisible = true;
            }
            if (Title == "Редактирование сотрудника")
            {
                loginPanel.IsVisible = true;
                passwordPanel.IsVisible = true;
                rolePanel.IsVisible = true;
                namePanel.IsVisible = true;
                surnamePanel.IsVisible = true;
                patronymicPanel.IsVisible = true;
                statusComboBox.IsVisible = true;
                
                loginInput.IsVisible = false;
                passwordInput.IsVisible = false;
                roleComboBox.IsVisible = false;
                addPhotoEmployee.IsVisible = false;
                nameInput.IsVisible = false;
                surnameInput.IsVisible = false;
                patronymicInput.IsVisible = false;
                addEmploymentContract.IsVisible = false;
            }
            
        }

        public void LoadEmployee(int employeeId)
        {
            
           ResetComponents();
           EmployeeId = employeeId;
           var employeeInfo = _databaseService.GetEmployeeById(employeeId);
           
           var loginTextBlock = this.FindControl<TextBlock>("LoginTextBlock");
           var passwordTextBlock = this.FindControl<TextBlock>("PasswordTextBlock");
           var roleTextBlock = this.FindControl<TextBlock>("RoleTextBlock");
           
           var nameTextBlock = this.FindControl<TextBlock>("NameTextBlock");
           var surnameTextBlock = this.FindControl<TextBlock>("SurnameTextBlock");
           var patronymicTextBlock = this.FindControl<TextBlock>("PatronymicTextBlock");
           
           var statusComboBox = this.FindControl<global::CafeApp.Controls.Components.ComboBox.ComboBox>("StatusComboBox");
      
           if (Title == "Редактирование сотрудника")
           {
               if (employeeInfo.EmploymentStatus)
               { statusComboBox.SelectedItem = "Работает"; }
               else
               { statusComboBox.SelectedItem = "Уволен"; }

                loginTextBlock.Text = employeeInfo.Username;
                passwordTextBlock.Text = employeeInfo.Password;
                roleTextBlock.Text = employeeInfo.Role;
                nameTextBlock.Text = employeeInfo.Name;
                surnameTextBlock.Text = employeeInfo.Surname;
                patronymicTextBlock.Text = employeeInfo.Patronymic;
                File.AppendAllText(logPath, 
                $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - " +
                $"Employee loaded successfully: {employeeInfo.Surname} " +
                $"{employeeInfo.Name} $\"{employeeInfo.Patronymic}" +
                $"{employeeInfo.Role} {employeeInfo.Password}\n");
           }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var usernameInput = this.FindControl<Input>("UsernameInput");
            var passwordInput = this.FindControl<Input>("PasswordInput");
            var nameInput = this.FindControl<Input>("NameInput");
            var surnameInput = this.FindControl<Input>("SurnameInput");
            var patronymicInput = this.FindControl<Input>("PatronymicInput");
            var roleComboBox = this.FindControl<global::CafeApp.Controls.Components.ComboBox.ComboBox>("RoleComboBox");
           
            string username = usernameInput.Value?.ToString() ?? 
                             usernameInput.Content?.ToString() ?? 
                             usernameInput.Text ?? "";
            
            string password = passwordInput.Value?.ToString() ?? 
                             passwordInput.Content?.ToString() ?? 
                             passwordInput.Text ?? "";
            string name = nameInput.Value?.ToString() ?? 
                         nameInput.Content?.ToString() ?? 
                         nameInput.Text ?? "";

            string surname = surnameInput.Value?.ToString() ?? 
                            surnameInput.Content?.ToString() ?? 
                            surnameInput.Text ?? "";

            string patronymic = patronymicInput.Value?.ToString() ?? 
                               patronymicInput.Content?.ToString() ?? 
                               patronymicInput.Text ?? "";

            var roleInnerComboBox = roleComboBox?.FindControl<Avalonia.Controls.ComboBox>("MainComboBox");
            string selectedRole = roleInnerComboBox?.SelectedItem?.ToString() ?? "";
            
            try
            {
                var statusComboBox = this.FindControl<global::CafeApp.Controls.Components.ComboBox.ComboBox>("StatusComboBox");
                
                if (Title == "Редактирование сотрудника")
                {
                    bool status = false;
                    if (GetComboBoxValue("StatusComboBox") == "Работает")
                    { status = true; }
                    else
                    { status = false; }
                    
                   bool success = _databaseService.UpdateEmployeeStatus(EmployeeId, status);
                   if (success)
                   { statusComboBox.SelectedItem = ""; }
                    
                }
                else
                {
                    if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || 
                        string.IsNullOrEmpty(name) || string.IsNullOrEmpty(surname) ||
                        string.IsNullOrEmpty(selectedRole) || string.IsNullOrEmpty(_photoFilePath) ||
                        string.IsNullOrEmpty(_contractFilePath))
                    { return; }
                    string projectDirectory = Directory.GetCurrentDirectory();
                    string photoLink = "";
                    string contractScanLink = "";
                    
                    if (!string.IsNullOrEmpty(_photoFilePath))
                    {
                        string photoTargetDir = Path.Combine(projectDirectory, "images/EmployeePhoto");
                        Directory.CreateDirectory(photoTargetDir);
                        string photoFileName = Path.GetFileName(_photoFilePath);
                        string photoTargetPath = Path.Combine(photoTargetDir, photoFileName);
                        
                        using (var sourceStream = new FileStream(_photoFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        using (var targetStream = new FileStream(photoTargetPath, FileMode.Create, FileAccess.Write))
                        { sourceStream.CopyTo(targetStream); }
                        photoLink = $"CafeApp/images/EmployeePhoto/{photoFileName}";
                    }
                    if (!string.IsNullOrEmpty(_contractFilePath))
                    {
                        string contractTargetDir = Path.Combine(projectDirectory, "images/EmploymentContract");
                        Directory.CreateDirectory(contractTargetDir);
                        string contractFileName = Path.GetFileName(_contractFilePath);
                        string contractTargetPath = Path.Combine(contractTargetDir, contractFileName);
                        using (var sourceStream = new FileStream(_contractFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        using (var targetStream = new FileStream(contractTargetPath, FileMode.Create, FileAccess.Write))
                        { sourceStream.CopyTo(targetStream); }
                        contractScanLink = $"CafeApp/images/EmploymentContract/{contractFileName}";
                    }
                    bool isRegistered = _databaseService.RegisterUser(username, password, name, surname, patronymic, selectedRole, photoLink, contractScanLink);

                    if (isRegistered)
                    {
                        ClearForm();
                        SaveButtonClicked?.Invoke(this, EventArgs.Empty);
                    }
                }
            }
            catch (Exception ex)
            {
                string errorMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - ERROR saving files: {ex.Message}\n";
                File.AppendAllText(logPath, errorMessage);
            }
        }
        private string GetComboBoxValue(string comboBoxName)
        {
            var comboBox = this.FindControl<global::CafeApp.Controls.Components.ComboBox.ComboBox>(comboBoxName);
            var innerComboBox = comboBox?.FindControl<ComboBox>("MainComboBox");
            return innerComboBox?.SelectedItem?.ToString() ?? "";
        }

        public void ClearForm()
        {
            var usernameInput = this.FindControl<Input>("UsernameInput");
            var passwordInput = this.FindControl<Input>("PasswordInput");
            var nameInput = this.FindControl<Input>("NameInput");
            var surnameInput = this.FindControl<Input>("SurnameInput");
            var patronymicInput = this.FindControl<Input>("PatronymicInput");
            var roleComboBox = this.FindControl<global::CafeApp.Controls.Components.ComboBox.ComboBox>("RoleComboBox");
            var roleInnerComboBox = roleComboBox?.FindControl<Avalonia.Controls.ComboBox>("MainComboBox");
            usernameInput.Value = "";
            passwordInput.Value = "";
            nameInput.Value = "";
            surnameInput.Value = "";
            patronymicInput.Value = "";

            if (roleInnerComboBox != null)
            { roleInnerComboBox.SelectedIndex = -1; }

            var addPhotoEmployee = this.FindControl<TextBlock>("AddPhotoEmployee");
            var addEmploymentContract = this.FindControl<TextBlock>("AddEmploymentContract");
            addPhotoEmployee.Text = "+";
            addEmploymentContract.Text = "+";

            _photoFilePath = "";
            _contractFilePath = "";
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
    
            var addPhotoEmployee = this.FindControl<TextBlock>("AddPhotoEmployee");
            var addEmploymentContract = this.FindControl<TextBlock>("AddEmploymentContract");
            var dragAndDrop = this.FindControl<global::CafeApp.Controls.Components.DragAndDrop.DragAndDrop>("DragAndDrop");
    
            addPhotoEmployee.PointerPressed += AddPhoto;
            addEmploymentContract.PointerPressed += AddContract;
    
            dragAndDrop.FileSelected += OnFileSelected;
        }

        private void AddPhoto(object sender, RoutedEventArgs e)
        {
            var dragAndDrop = this.FindControl<global::CafeApp.Controls.Components.DragAndDrop.DragAndDrop>("DragAndDrop");
            dragAndDrop.SetFileType("photo");
            dragAndDrop.IsVisible = !dragAndDrop.IsVisible;
        }

        private void AddContract(object sender, RoutedEventArgs e)
        {
            var dragAndDrop = this.FindControl<global::CafeApp.Controls.Components.DragAndDrop.DragAndDrop>("DragAndDrop");
            dragAndDrop.SetFileType("contract");
            dragAndDrop.IsVisible = !dragAndDrop.IsVisible;
        }

        private void OnFileSelected(object sender, string filePath)
        {
            var dragAndDrop = sender as global::CafeApp.Controls.Components.DragAndDrop.DragAndDrop;
            
            if (dragAndDrop != null)
            {
                var addPhotoEmployee = this.FindControl<TextBlock>("AddPhotoEmployee");
                var addEmploymentContract = this.FindControl<TextBlock>("AddEmploymentContract");
                
                if (addPhotoEmployee.Text == "+")
                {
                    _photoFilePath = filePath;
                    addPhotoEmployee.Text = "✓";
                }
                else if (addEmploymentContract.Text == "+")
                {
                    _contractFilePath = filePath;
                    addEmploymentContract.Text = "✓";
                }
            }
        }
    }
}