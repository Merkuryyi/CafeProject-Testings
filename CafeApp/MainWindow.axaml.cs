using Avalonia.Controls;
using CafeApp.Services;
using CafeApp.Controls;
using System;

namespace CafeApp
{
    public partial class MainWindow : Window
    {
        private DatabaseService _databaseService;

        public MainWindow()
        {
            InitializeComponent();
            _databaseService = new DatabaseService(new Models.AppConfig());
            
            // Подписываемся на события после загрузки окна
            this.Opened += OnMainWindowOpened;
        }

        private void OnMainWindowOpened(object sender, EventArgs e)
        {
            SubscribeToAuthEvents();
        }

        private void SubscribeToAuthEvents()
        {
            // Находим форму авторизации и подписываемся на события
            var formInput = this.FindControl<FormInput>("FormInputControlName");
            if (formInput != null)
            {
                formInput.AuthenticationCompleted += OnAuthenticationCompleted;
                Console.WriteLine("Подписка на события формы установлена");
            }
        }

        private void OnAuthenticationCompleted(object sender, bool isAuthenticated)
        {
            if (isAuthenticated)
            {
                ShowWithSidebar();
                Console.WriteLine("Авторизация успешна - показываем сайдбар");
            }
            else
            {
                Console.WriteLine("Авторизация не удалась");
                // Можно показать сообщение об ошибке
            }
        }

        // Метод для показа с сайдбаром (после авторизации)
        public void ShowWithSidebar()
        {
            WithSidebarPanel.IsVisible = true;
            WithoutSidebarPanel.IsVisible = false;
        }

        // Метод для скрытия сайдбара (выход из системы)
        public void ShowWithoutSidebar()
        {
            WithSidebarPanel.IsVisible = false;
            WithoutSidebarPanel.IsVisible = true;
        }
    }
}