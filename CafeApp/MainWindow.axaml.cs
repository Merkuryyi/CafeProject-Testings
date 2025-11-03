using Avalonia.Controls;

namespace CafeApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
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