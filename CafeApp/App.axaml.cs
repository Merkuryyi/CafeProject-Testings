using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace CafeApp
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
            LoadACSSStyles();
        }

        private void LoadACSSStyles()
        {
            try
            {
                var cssContent = AssetLoader.Load(new Uri("avares://CafeApp/Styles/Components.acss"));
                var styles = AvaloniaRuntimeXamlLoader.Parse<Styles>(cssContent);
                this.Styles.Add(styles);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading ACSS: {ex.Message}");
            }
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow();
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}