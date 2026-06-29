using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CafeApp.Data;
using CafeApp.Models;
using System.Linq;
using CafeApp.Database;
using CafeApp.Excel;
using CafeApp.PDF;
using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;

namespace CafeApp.Controls
{
    public partial class Report : UserControl
    {
        private readonly DatabaseService _databaseService;
        private readonly ExcelService _excelService;
        private readonly PdfService _pdfService;
       
        public static readonly StyledProperty<string> TitleProperty =
            AvaloniaProperty.Register<Report, string>(nameof(Title), "Отчеты");

        public static readonly StyledProperty<string> RoleProperty =
            AvaloniaProperty.Register<Report, string>(nameof(Role), "администратор");
        
        public ObservableCollection<string> FormatSelection { get; set; }
        public ObservableCollection<ListItem> ShiftSelection { get; set; }
        
        public string Title
        {
            get => GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public string Role
        {
            get => GetValue(RoleProperty);
            set
            {
                SetValue(RoleProperty, value);
                ResetComponents();
            }
        }

        public Report()
        {
            InitializeComponent();
            
            _databaseService = new DatabaseService();
            _excelService = new ExcelService();
            _pdfService = new PdfService();
            
            FormatSelection = new ObservableCollection<string>
            {
                "Excel",
                "PDF"
            };
            
            ShiftSelection = new ObservableCollection<ListItem>(_databaseService.GetShiftsList());
            this.DataContext = this;
        }

        public void ResetComponents()
        {
            var reportWaiterPanel = this.FindControl<StackPanel>("ReportWaiterPanel");
            var reportOrdersReceivedPanel = this.FindControl<StackPanel>("ReportOrdersReceivedPanel");
            var reportOrdersPaidPanel = this.FindControl<StackPanel>("ReportOrdersPaidPanel");

            if (Role == "официант")
            {
                reportWaiterPanel.IsVisible = true;
                reportOrdersReceivedPanel.IsVisible = false;
                reportOrdersPaidPanel.IsVisible = false;
            }
            else if (Role == "администратор")
            {
                reportWaiterPanel.IsVisible = false;
                reportOrdersReceivedPanel.IsVisible = true;
                reportOrdersPaidPanel.IsVisible = true;
            }
            else
            {
                reportWaiterPanel.IsVisible = false;
                reportOrdersReceivedPanel.IsVisible = false;
                reportOrdersPaidPanel.IsVisible = false;
            }
        }

        private void OnReportWaiterPressed(object sender, PointerPressedEventArgs e)
        {
            try
            {
                int waiterId = CurrentUser.Id;
                string waiterName = CurrentUser.FullName ?? "";
                
                if (waiterId == -1) { return; }

                int currentShiftId = _databaseService.GetCurrentOrLatestShiftId();
                
                var mainWindow = (MainWindow)this.VisualRoot;
                if (mainWindow != null)
                { mainWindow.ShowWaiterReport(waiterId, waiterName, currentShiftId);}

            }
            catch (Exception ex)
            {
                File.AppendAllText(@"A:\debug.log", 
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - ERROR in OnReportWaiterPressed: {ex.Message}\n{ex.StackTrace}\n");
            }
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
    
            InitializeComboBoxes();
            ResetComponents();
    
            var reportWaiterTextBlock = this.FindControl<TextBlock>("ReportWaiterTextBlock");
            if (reportWaiterTextBlock != null)
                reportWaiterTextBlock.PointerPressed += OnReportWaiterPressed;
        }

        private void InitializeComboBoxes()
        {
            var ordersReceivedFormatComboBox = this.FindControl<global::CafeApp.Controls.Components.ComboBox.ComboBox>("OrdersReceivedFormatSelectionComboBox");
            var shiftSelectionComboBox = this.FindControl<global::CafeApp.Controls.Components.ComboBox.ComboBox>("ShiftSelectionComboBox");
            var ordersPaidFormatComboBox = this.FindControl<global::CafeApp.Controls.Components.ComboBox.ComboBox>("OrdersPaidFormatSelectionComboBox");

            if (ordersReceivedFormatComboBox != null)
            { 
                ordersReceivedFormatComboBox.ItemsSource = FormatSelection;
                ordersReceivedFormatComboBox.SelectedIndex = 0;
            }
            
            if (shiftSelectionComboBox != null)
            { 
                shiftSelectionComboBox.ItemsSource = ShiftSelection;
                if (ShiftSelection.Count > 0) shiftSelectionComboBox.SelectedIndex = 0;
            }
            
            if (ordersPaidFormatComboBox != null)
            { 
                ordersPaidFormatComboBox.ItemsSource = FormatSelection;
                ordersPaidFormatComboBox.SelectedIndex = 0;
            }
        }
        
        private async void OnReportOrdersReceivedPressed(object sender, PointerPressedEventArgs e)
        { await GenerateOrdersReceivedReport(); }

        private async void OnReportOrdersPaidPressed(object sender, PointerPressedEventArgs e)
        { await GenerateOrdersPaidReport(); }

        private async Task GenerateOrdersReceivedReport()
        {
            try
            {
                string format = GetComboBoxValue("OrdersReceivedFormatSelectionComboBox");
                string shiftDisplay = GetComboBoxValue("ShiftSelectionComboBox");
                
                if (string.IsNullOrEmpty(format) || string.IsNullOrEmpty(shiftDisplay)) { return; }
                
                int shiftId = GetShiftIdFromDisplayText(shiftDisplay);
                if (shiftId == -1) { return; }

                var orders = _databaseService.GetOrdersReceivedReport(shiftId);
                
                if (!orders.Any())
                {
                    await ShowMessage("Информация", "Нет данных для выбранной смены");
                    File.AppendAllText(@"A:\debug.log", 
                        $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - No orders found for shift {shiftId}\n");
                    return;
                }
                
                var filePath = await ShowSaveFileDialog(format.ToLower(), $"отчет_полученных_заказов_{shiftId}");
                if (string.IsNullOrEmpty(filePath))
                { return; }

                string resultFilePath = string.Empty;
                
                if (format == "Excel")
                { resultFilePath = _excelService.GenerateOrdersReceivedReport(orders, shiftDisplay, filePath); }
                else if (format == "PDF")
                { resultFilePath = _pdfService.GenerateOrdersReceivedReport(orders, shiftDisplay, filePath); }
                if (!string.IsNullOrEmpty(resultFilePath) && File.Exists(resultFilePath))
                { OpenFileInExplorer(resultFilePath); }
            }
            catch (Exception ex)
            {
                await ShowMessage("Ошибка", $"Произошла ошибка: {ex.Message}");
                File.AppendAllText(@"A:\debug.log", 
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - ERROR in GenerateOrdersReceivedReport: {ex.Message}\n{ex.StackTrace}\n");
            }
        }

        private async Task GenerateOrdersPaidReport()
        {
            try
            {
                string format = GetComboBoxValue("OrdersPaidFormatSelectionComboBox");
                
                if (string.IsNullOrEmpty(format))
                { return; }
                var orders = _databaseService.GetPaidOrdersReport();
                
                if (!orders.Any())
                {
                    await ShowMessage("Информация", "Нет оплаченных заказов");
                    File.AppendAllText(@"A:\debug.log", 
                        $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - No paid orders found\n");
                    return;
                }

                string shiftInfo = "Текущая/последняя смена";
                
                var filePath = await ShowSaveFileDialog(format.ToLower(), "отчет_оплаченных_заказов");
                if (string.IsNullOrEmpty(filePath))
                { return; }

                string resultFilePath = string.Empty;

                if (format == "Excel")
                { resultFilePath = _excelService.GeneratePaidOrdersReport(orders, shiftInfo, filePath); }
                else if (format == "PDF")
                { resultFilePath = _pdfService.GeneratePaidOrdersReport(orders, shiftInfo, filePath); }

                if (!string.IsNullOrEmpty(resultFilePath) && File.Exists(resultFilePath))
                { OpenFileInExplorer(resultFilePath); }
               
            }
            catch (Exception ex)
            {
                await ShowMessage("Ошибка", $"Произошла ошибка: {ex.Message}");
                File.AppendAllText(@"A:\debug.log", 
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - ERROR in GenerateOrdersPaidReport: {ex.Message}\n{ex.StackTrace}\n");
            }
        }

        private async Task<string> ShowSaveFileDialog(string format, string defaultFileName)
        {
            try
            {
                var topLevel = TopLevel.GetTopLevel(this);
                if (topLevel == null) return string.Empty;

                var fileExtension = format == "excel" ? "xlsx" : "pdf";
                var mimeType = format == "excel" ? "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" : "application/pdf";

                var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
                {
                    Title = "Сохранить отчет",
                    SuggestedFileName = $"{defaultFileName}_{DateTime.Now:yyyyMMdd_HHmmss}.{fileExtension}",
                    FileTypeChoices = new[]
                    {
                        new FilePickerFileType(format == "excel" ? "Excel файл" : "PDF файл")
                        {
                            Patterns = new[] { $"*.{fileExtension}" },
                            MimeTypes = new[] { mimeType }
                        }
                    }
                });

                return file?.Path.LocalPath ?? string.Empty;
            }
            catch (Exception ex)
            {
                File.AppendAllText(@"A:\debug.log",
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - ERROR in file dialog: {ex.Message}\n");
                return string.Empty;
            }
        }
        
        private async Task ShowMessage(string title, string message)
        {
            try
            {
                var dialog = new Window
                {
                    Title = title,
                    Content = new TextBlock { Text = message, Margin = new Thickness(20) },
                    SizeToContent = SizeToContent.WidthAndHeight,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };

                var mainWindow = VisualRoot as Window;
                if (mainWindow != null)
                { await dialog.ShowDialog(mainWindow); }
            }
            catch (Exception ex)
            {
                File.AppendAllText(@"A:\debug.log",
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - ERROR showing message: {ex.Message}\n");
            }
        }
        
        private int GetShiftIdFromDisplayText(string displayText)
        {
            var shift = ShiftSelection.FirstOrDefault(s => s.DisplayText == displayText);
            return shift?.Id ?? -1;
        }
        
        private string GetComboBoxValue(string comboBoxName)
        {
            var comboBox = this.FindControl<global::CafeApp.Controls.Components.ComboBox.ComboBox>(comboBoxName);
            var innerComboBox = comboBox?.FindControl<ComboBox>("MainComboBox");
            return innerComboBox?.SelectedItem?.ToString() ?? "";
        }

        private void OpenFileInExplorer(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                { System.Diagnostics.Process.Start("explorer.exe", $"/select, \"{filePath}\""); }
            }
            catch (Exception ex)
            {
                File.AppendAllText(@"A:\debug.log",
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - ERROR opening explorer: {ex.Message}\n");
            }
        }
    }
}