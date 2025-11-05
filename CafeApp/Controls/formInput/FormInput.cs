using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;
using CafeApp.Services;
using CafeApp.Models;
using System;
using System.Threading.Tasks;

namespace CafeApp.Controls
{
    public partial class FormInput : UserControl
    {
        private DatabaseService _databaseService;

        public static readonly StyledProperty<string> TitleProperty =
            AvaloniaProperty.Register<FormInput, string>(nameof(Title), "Название");

        public static readonly StyledProperty<bool> ShowRoleSelectorProperty =
            AvaloniaProperty.Register<FormInput, bool>(nameof(ShowRoleSelector), true);

        // Событие для уведомления о результате авторизации
        public event EventHandler<bool> AuthenticationCompleted;

        public FormInput()
        {
            InitializeComponent();
            _databaseService = new DatabaseService(new AppConfig());
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
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


        // Сделайте обработчик асинхронным
        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Кнопка нажата!");
            if (this.VisualRoot is MainWindow currentWindow)
            {
                currentWindow.ShowWithSidebar();
                Console.WriteLine("Сайдбар показан!");
            }

            try
            {
                // Получаем значения из полей ввода
                string username = GetUsernameFromInput();
                string password = GetPasswordFromInput();

                //Console.WriteLine($"Логин: {username}, Пароль: {password}");
/*
                // Проверяем, что поля не пустые
                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    Console.WriteLine("Ошибка: Логин и пароль не могут быть пустыми");
                    return;
                }

                // Выполняем проверку в БД асинхронно
                bool isAuthenticated = await Task.Run(() =>
                {
                    return _databaseService.AuthenticateUser(username, password);
                });

                Console.WriteLine($"Результат авторизации: {isAuthenticated}");

                // Если авторизация успешна, показываем сайдбар
                if (isAuthenticated && this.VisualRoot is MainWindow mainWindow)
                {
                                        mainWindow.ShowWithSidebar();
                    Console.WriteLine("Сайдбар показан!");
                }*/
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при авторизации: {ex.Message}");
            }
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == TitleProperty)
            {
                ShowRoleSelector = change.NewValue?.ToString() != "Авторизация";
            }
        }

        private string GetUsernameFromInput()
        {
            var usernameInput = this.FindControl<CafeApp.Controls.Components.Input.Input>("UsernameInput");
            var textProperty = usernameInput.GetType().GetProperty("Text");
            return textProperty.GetValue(usernameInput)?.ToString() ?? string.Empty;;

        }

        private string GetPasswordFromInput()
        {
            var usernameInput = this.FindControl<CafeApp.Controls.Components.Input.Input>("PasswordInput");
            var textProperty = usernameInput.GetType().GetProperty("Text");
            return textProperty.GetValue(usernameInput)?.ToString() ?? string.Empty;;

        }
    }
}