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
        private string _photoFilePath = "";
        private string _contractFilePath = "";

        public List<string> Roles { get; set; }
        public List<string> Status { get; set; }

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
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public ObservableCollection<string> Employees { get; } = new ObservableCollection<string>();
        public event EventHandler? SaveButtonClicked;

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

            // Проверка что все поля заполнены
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || 
                string.IsNullOrEmpty(name) || string.IsNullOrEmpty(surname) ||
                string.IsNullOrEmpty(selectedRole) || string.IsNullOrEmpty(_photoFilePath) ||
                string.IsNullOrEmpty(_contractFilePath))
            {
                // Логируем ошибку
                string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - ERROR: Not all fields are filled\n";
                File.AppendAllText("A:/Инженерно-техническая поддержка сопровождения ИС/debug.log", logMessage);
                return;
            }

            // Копируем файлы при сохранении
            try
            {
                string projectDirectory = Directory.GetCurrentDirectory();
                
                // Копируем фото
                if (!string.IsNullOrEmpty(_photoFilePath))
                {
                    string photoTargetDir = Path.Combine(projectDirectory, "images/EmployeePhoto");
                    Directory.CreateDirectory(photoTargetDir);
                    string photoFileName = Path.GetFileName(_photoFilePath);
                    string photoTargetPath = Path.Combine(photoTargetDir, photoFileName);
                    File.Copy(_photoFilePath, photoTargetPath, true);
                }

                // Копируем договор
                if (!string.IsNullOrEmpty(_contractFilePath))
                {
                    string contractTargetDir = Path.Combine(projectDirectory, "images/EmploymentContract");
                    Directory.CreateDirectory(contractTargetDir);
                    string contractFileName = Path.GetFileName(_contractFilePath);
                    string contractTargetPath = Path.Combine(contractTargetDir, contractFileName);
                    File.Copy(_contractFilePath, contractTargetPath, true);
                }

                string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - Form data saved:\n" +
                                   $"Username: '{username}'\n" +
                                   $"Password: '{password}'\n"+
                                   $"Name: '{name}'\n" +
                                   $"Surname: '{surname}'\n" +
                                   $"Patronymic: '{patronymic}'\n" +
                                   $"Role: '{selectedRole}'\n" +
                                   $"Photo: '{_photoFilePath}'\n" +
                                   $"Contract: '{_contractFilePath}'\n";
                File.AppendAllText("A:/Инженерно-техническая поддержка сопровождения ИС/debug.log", logMessage);

                SaveButtonClicked?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                string errorMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - ERROR saving files: {ex.Message}\n";
                File.AppendAllText("A:/Инженерно-техническая поддержка сопровождения ИС/debug.log", errorMessage);
            }
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