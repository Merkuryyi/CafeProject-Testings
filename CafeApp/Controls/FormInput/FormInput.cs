using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;
using System;
using CafeApp.Database;
using CafeApp.Controls.Components.Input;
using System.IO;

namespace CafeApp.Controls
{
    public partial class FormInput : UserControl
    {
        public static readonly StyledProperty<string> TitleProperty =
            AvaloniaProperty.Register<FormInput, string>(nameof(Title), "Вход в систему");

        private DatabaseService _databaseService;

        public static readonly StyledProperty<bool> ShowRoleSelectorProperty =
            AvaloniaProperty.Register<FormInput, bool>(nameof(ShowRoleSelector));

        public FormInput()
        {
            InitializeComponent();
            _databaseService = new DatabaseService();
        }

        public string Title
        {
            get => GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public bool ShowRoleSelector
        {
            get => GetValue(ShowRoleSelectorProperty);
            set => SetValue(ShowRoleSelectorProperty, value);
        }
        
        public event EventHandler<string>? LoginResult; 

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            var usernameInput = this.FindControl<Input>("UsernameInput");
            var passwordInput = this.FindControl<Input>("PasswordInput");

            string username = usernameInput.Value?.ToString() ?? "";
            
            string password = passwordInput.Value?.ToString() ?? 
                             passwordInput.Content?.ToString() ?? 
                             passwordInput.Text ?? "";

            string? role = _databaseService.AuthenticateUser(username, password);
            

            // Передаем роль (или null если аутентификация не удалась)
            LoginResult?.Invoke(this, role ?? "");
        }
    }
}