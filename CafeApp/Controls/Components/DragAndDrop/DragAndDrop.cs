using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Input;
using System;
using System.IO;
using System.Threading.Tasks;

namespace CafeApp.Controls.Components.DragAndDrop
{
    public partial class DragAndDrop : UserControl
    {
        private string _fileType = "";

        public DragAndDrop()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            var addFile = this.FindControl<TextBlock>("AddDocument");
            addFile.PointerPressed += async (s, e) => await OpenFilePicker();
            
            DragDrop.SetAllowDrop(this, true);
            this.AddHandler(DragDrop.DropEvent, OnDrop);
        }

        public void SetFileType(string fileType) => _fileType = fileType;

        private async void OnDrop(object sender, DragEventArgs e)
        {
            var files = e.Data.GetFiles();
            if (files != null)
            {
                foreach (var file in files)
                {
                    await SaveFile(file.Path.LocalPath);
                }
            }
        }

        private async Task OpenFilePicker()
        {
            var fileDialog = new OpenFileDialog();
            fileDialog.Filters.Add(new FileDialogFilter { Name = "PDF Files", Extensions = { "pdf" } });
            
            var window = this.VisualRoot as Window;
            var result = await fileDialog.ShowAsync(window);
            if (result != null && result.Length > 0)
            {
                await SaveFile(result[0]);
            }
        }

        private async Task SaveFile(string sourceFilePath)
        {
            try
            {
                // Используем абсолютный путь к папке проекта
                string projectDirectory = Directory.GetCurrentDirectory();
                string targetDirectory = Path.Combine(projectDirectory, 
                    _fileType == "photo" ? "images/EmployeePhoto" : "images/EmploymentContract");

                Directory.CreateDirectory(targetDirectory);
                string fileName = Path.GetFileName(sourceFilePath);
                string targetFilePath = Path.Combine(targetDirectory, fileName);

                File.Copy(sourceFilePath, targetFilePath, true);
                
                // Логируем для отладки
                string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - File copied from {sourceFilePath} to {targetFilePath}\n";
                File.AppendAllText(Path.Combine(projectDirectory, "debug.log"), logMessage);

                this.IsVisible = false;
                FileSelected?.Invoke(this, targetFilePath);
            }
            catch (Exception ex)
            {
                // Логируем ошибку
                string projectDirectory = Directory.GetCurrentDirectory();
                string errorMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - Error: {ex.Message}\n";
                File.AppendAllText(Path.Combine(projectDirectory, "debug.log"), errorMessage);
            }
        }

        public event EventHandler<string> FileSelected;
    }
}