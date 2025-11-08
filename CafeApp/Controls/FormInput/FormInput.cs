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

        // Событие для кнопки входа
        public event EventHandler? LoginButtonClicked;
        public event EventHandler<string>? LoginResult; // Теперь передаем строку (роль)

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            var usernameInput = this.FindControl<Input>("UsernameInput");
            var passwordInput = this.FindControl<Input>("PasswordInput");

            string username = usernameInput.Value?.ToString() ?? 
                             usernameInput.Content?.ToString() ?? 
                             usernameInput.Text ?? "";
            
            string password = passwordInput.Value?.ToString() ?? 
                             passwordInput.Content?.ToString() ?? 
                             passwordInput.Text ?? "";

            string? role = _databaseService.AuthenticateUser(username, password);
            
            string filePath = "A:/Инженерно-техническая поддержка сопровождения ИС/debug.log";
            string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - Login result: role='{role}'\n";
            File.AppendAllText(filePath, logMessage);

            // Передаем роль (или null если аутентификация не удалась)
            LoginResult?.Invoke(this, role ?? "");
        }
    }
}