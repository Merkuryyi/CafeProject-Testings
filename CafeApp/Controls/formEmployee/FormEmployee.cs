using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity; // Добавьте эту директиву
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

		public List<string> Roles { get; set; }
		public List<string> Status { get; set; }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

		public ObservableCollection<string> Employees { get; } = new ObservableCollection<string>();

		public event EventHandler? SaveButtonClicked;
    //    public event EventHandler<string>? LoginResult; // Теперь передаем строку (роль)

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
			string name =nameInput.Value?.ToString() ?? 
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
    		
    
         //   string? role = _databaseService.AuthenticateUser(username, password);
            
            string filePath = "A:/Инженерно-техническая поддержка сопровождения ИС/debug.log";
            string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - Form data:\n" +
                               $"Username: '{username}'\n" +
                               $"Password: '{password}'\n"+
                               $"Name: '{name}'\n" +
                               $"Surname: '{surname}'\n" +
                               $"Patronymic: '{patronymic}'\n" +
                               $"Role: '{selectedRole}'\n";
            File.AppendAllText(filePath, logMessage);

            // Передаем роль (или null если аутентификация не удалась)
        //    LoginResult?.Invoke(this, role ?? "");
        }

    }
}