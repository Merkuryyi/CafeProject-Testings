using Avalonia.Controls;
using CafeApp.Services;
using CafeApp.Controls;
using CafeApp.Controls.Components.Sidebar;
using System;

namespace CafeApp
{
    public partial class MainWindow : Window
    {
        private DatabaseService _databaseService;
        private Sidebar? _sidebarControl;
        private FormEmployee? _formEmployeeControl;
        private FormInput? _formInputControl;

        public MainWindow()
        {
            InitializeComponent();
            _databaseService = new DatabaseService(new Models.AppConfig());
            
            this.Opened += OnMainWindowOpened;
        }

        private void OnMainWindowOpened(object sender, EventArgs e)
        {
            SubscribeToAuthEvents();
            SubscribeToSidebarEvents();
        }

        private void SubscribeToAuthEvents()
        {
            _formInputControl = this.FindControl<FormInput>("FormInputControl");
            if (_formInputControl != null)
            {
                _formInputControl.AuthenticationCompleted += OnAuthenticationCompleted;
                Console.WriteLine("Подписка на события формы авторизации установлена");
            }
            else
            {
                Console.WriteLine("FormInputControl не найден");
            }
        }

        private void SubscribeToSidebarEvents()
        {
            _sidebarControl = this.FindControl<Sidebar>("SidebarControl");
            if (_sidebarControl != null)
            {
                _sidebarControl.ItemSelected += OnSidebarItemSelected;
                Console.WriteLine("Подписка на события сайдбара установлена");
            }
            else
            {
                Console.WriteLine("SidebarControl не найден");
            }
        }

        private void OnSidebarItemSelected(object sender, string itemName)
        {
            // Находим форму сотрудника каждый раз (на случай если она еще не инициализирована)
            _formEmployeeControl = this.FindControl<FormEmployee>("FormEmployeeControl");
            
            if (_formEmployeeControl == null)
            {
                Console.WriteLine("FormEmployeeControl не найден");
                return;
            }

            // Показываем форму только для регистрации
            if (itemName == "RegistrationText")
            {
                _formEmployeeControl.IsVisible = true;
                Console.WriteLine("Показана форма сотрудника");
            }
            else
            {
                _formEmployeeControl.IsVisible = false;
                Console.WriteLine($"Скрыта форма сотрудника, выбран: {itemName}");
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
            }
        }

        // Метод для показа с сайдбаром (после авторизации)
        public void ShowWithSidebar()
        {
            var withSidebarPanel = this.FindControl<Grid>("WithSidebarPanel");
            var withoutSidebarPanel = this.FindControl<Grid>("WithoutSidebarPanel");
            
            if (withSidebarPanel != null && withoutSidebarPanel != null)
            {
                withSidebarPanel.IsVisible = true;
                withoutSidebarPanel.IsVisible = false;
            }
        }

        // Метод для скрытия сайдбара (выход из системы)
        public void ShowWithoutSidebar()
        {
            var withSidebarPanel = this.FindControl<Grid>("WithSidebarPanel");
            var withoutSidebarPanel = this.FindControl<Grid>("WithoutSidebarPanel");
            
            if (withSidebarPanel != null && withoutSidebarPanel != null)
            {
                withSidebarPanel.IsVisible = false;
                withoutSidebarPanel.IsVisible = true;
            }
        }
    }
}