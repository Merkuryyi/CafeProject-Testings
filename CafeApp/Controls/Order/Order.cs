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
using System;
using Avalonia.Interactivity;
using CafeApp.Controls.Components.Input;
using System.IO;
using System.ComponentModel;
namespace CafeApp.Controls
{
    public partial class Order : UserControl
    {
        private readonly DatabaseService _databaseService;
        private readonly ExcelService _excelService = new ExcelService();
        public ObservableCollection<string> StatusOrder { get; set; }
        public List<string> PaymentSelection { get; set; }
        public ObservableCollection<string> AllMenuItems { get; set; }
        public ObservableCollection<string> ListWaiter { get; set; }
        public ObservableCollection<OrderMenuItem> OrderMenuItems { get; set; }
        
        public static readonly StyledProperty<string> TitleProperty =
            AvaloniaProperty.Register<Order, string>(nameof(Title), "Заказ");
        
        public static readonly StyledProperty<string> RoleProperty =
            AvaloniaProperty.Register<Order, string>(nameof(Role), "администратор");
        
        public static readonly StyledProperty<int> OrderIdProperty =
            AvaloniaProperty.Register<Order, int>(nameof(OrderId), -1);
        public static readonly StyledProperty<decimal> TotalPriceProperty =
            AvaloniaProperty.Register<Order, decimal>(nameof(TotalPrice), 0);

        public decimal TotalPrice
        {
            get => GetValue(TotalPriceProperty);
            set => SetValue(TotalPriceProperty, value);
        }
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
                UpdateStatusOrder();
                HideComponentsOrder();
            }
        }
        public int OrderId
        {
            get => GetValue(OrderIdProperty);
            set => SetValue(OrderIdProperty, value);
        }
        public event EventHandler? SaveButtonClicked;
        public Order()
        {
            InitializeComponent();
            
            PaymentSelection = new List<string> { "наличная", "безналичная" };
            _databaseService = new DatabaseService();
            var dataRepository = new DataRepository(_databaseService);
            
            if (CurrentUser.IsAuthenticated && CurrentUser.IsWaiter)
            {
                ListWaiter.Add(CurrentUser.FullName);
            }

            AllMenuItems = new ObservableCollection<string>(
                dataRepository.MenuItems.OrderBy(m => m.Name).Select(m => m.Name)
            );

            StatusOrder = new ObservableCollection<string>();
            OrderMenuItems = new ObservableCollection<OrderMenuItem> { new OrderMenuItem() };
            this.DataContext = this;
        }

        public void HideComponentsOrder()
        {
            if (this.Title != "Редактирование заказа") return;

            var tableInput = this.FindControl<Input>("TableInput");
            var statusComboBox = this.FindControl<global::CafeApp.Controls.Components.ComboBox.ComboBox>("StatusComboBox");
            var waiterComboBox = this.FindControl<global::CafeApp.Controls.Components.ComboBox.ComboBox>("WaiterComboBox");
            var addMenuItemButton = this.FindControl<TextBlock>("AddMenuItemButton");
            var removeMenuItemButton = this.FindControl<TextBlock>("RemoveMenuItemButton");
            var waiterPanel = this.FindControl<StackPanel>("WaiterPanel");
            var tablePanel = this.FindControl<StackPanel>("TablePanel");
            var scrollViewer = this.FindControl<ScrollViewer>("MenuScrollViewer");
            var scrollViewerFalse = this.FindControl<ScrollViewer>("MenuScrollViewerFalse");

            statusComboBox.ItemsSource = StatusOrder;
            waiterComboBox.ItemsSource = ListWaiter;

            if (Role != "администратор")
            {
                waiterComboBox.IsVisible = false;
                tableInput.IsVisible = false;
                addMenuItemButton.IsVisible = false;
                removeMenuItemButton.IsVisible = false;
                waiterPanel.IsVisible = true;
                tablePanel.IsVisible = true;
                scrollViewerFalse.IsVisible = true;
                scrollViewer.IsVisible = false;
            }
        }

        public void ReadOrder()
        {
            var orderInfo = _databaseService.GetOrderById(this.OrderId);
            var tableInput = this.FindControl<Input>("TableInput");
            var statusComboBox = this.FindControl<global::CafeApp.Controls.Components.ComboBox.ComboBox>("StatusComboBox");
            var waiterComboBox = this.FindControl<global::CafeApp.Controls.Components.ComboBox.ComboBox>("WaiterComboBox");
            var addMenuItemButton = this.FindControl<TextBlock>("AddMenuItemButton");
            var removeMenuItemButton = this.FindControl<TextBlock>("RemoveMenuItemButton");
            var waiterPanel = this.FindControl<StackPanel>("WaiterPanel");
            var paymentPanel = this.FindControl<StackPanel>("PaymentPanel");
            var statusPanel = this.FindControl<StackPanel>("StatusPanel");
            var tablePanel = this.FindControl<StackPanel>("TablePanel");
            var scrollViewer = this.FindControl<ScrollViewer>("MenuScrollViewer");
            var scrollViewerFalse = this.FindControl<ScrollViewer>("MenuScrollViewerFalse");

            waiterComboBox.IsVisible = false;
            tableInput.IsVisible = false;
            addMenuItemButton.IsVisible = false;
            removeMenuItemButton.IsVisible = false;
            statusComboBox.IsVisible = false;
            scrollViewerFalse.IsVisible = true;
            scrollViewer.IsVisible = false;

            waiterPanel.IsVisible = true;
            tablePanel.IsVisible = true;
            statusPanel.IsVisible = true;
            paymentPanel.IsVisible = true;

            var waiterTextBlock = this.FindControl<TextBlock>("WaiterTextBlock");
            var tableTextBlock = this.FindControl<TextBlock>("TableTextBlock");
            var statusTextBlock = this.FindControl<TextBlock>("StatusTextBlock");
            var paymentTextBlock = this.FindControl<TextBlock>("PaymentTextBlock");

            if (waiterTextBlock != null) waiterTextBlock.Text = orderInfo.WaiterName;
            if (tableTextBlock != null) tableTextBlock.Text = orderInfo.TableId.ToString();
            if (statusTextBlock != null) statusTextBlock.Text = orderInfo.Status;

            string paymentType = _databaseService.GetOrderPaymentType(this.OrderId);
            if (paymentTextBlock != null) paymentTextBlock.Text = paymentType;

            var saveButton = this.FindControl<global::CafeApp.Controls.Components.Button.Button>("SaveButton");
            if (saveButton != null) saveButton.IsVisible = false;
        }

        public void LoadOrderData(int orderId, string role)
        {
            this.OrderId = orderId;
            this.Role = role;
            var orderInfo = _databaseService.GetOrderById(orderId);
            this.Title = orderInfo.Status == "оплачен" ? "Просмотр заказа" : "Редактирование заказа";

            if (this.Title == "Просмотр заказа")
                ResetToEditMode();
            else
                ResetToEditMode();
            if (Role == "администратор")
            {
                var waiterComboBox = this.FindControl<global::CafeApp.Controls.Components.ComboBox.ComboBox>("WaiterComboBox");
                waiterComboBox.ItemsSource = ListWaiter;
                waiterComboBox.SelectedItem = orderInfo.WaiterName;
                var tableInput = this.FindControl<Input>("TableInput");
                tableInput.Value = orderInfo.TableId.ToString();
            }
            else
            {
                var waiterTextBlock = this.FindControl<TextBlock>("WaiterTextBlock");
                var tableTextBlock = this.FindControl<TextBlock>("TableTextBlock");
                if (waiterTextBlock != null) waiterTextBlock.Text = orderInfo.WaiterName;
                if (tableTextBlock != null) tableTextBlock.Text = orderInfo.TableId.ToString();
            }

            var statusComboBox = this.FindControl<global::CafeApp.Controls.Components.ComboBox.ComboBox>("StatusComboBox");
            statusComboBox.ItemsSource = StatusOrder;
            statusComboBox.SelectedItem = orderInfo.Status;

            OrderMenuItems.Clear();
            foreach (var item in orderInfo.Items)
            {
                OrderMenuItems.Add(new OrderMenuItem
                {
                    SelectedMenuItem = item.MenuItemName,
                    Quantity = item.Quantity
                });
            }
            UpdateTotalPrice();

            if (this.Title == "Просмотр заказа")
                ReadOrder();
            else
                HideComponentsOrder();
        }
        public void ResetToEditMode()
        {
            var tableInput = this.FindControl<Input>("TableInput");
            var statusComboBox = this.FindControl<global::CafeApp.Controls.Components.ComboBox.ComboBox>("StatusComboBox");
            var waiterComboBox = this.FindControl<global::CafeApp.Controls.Components.ComboBox.ComboBox>("WaiterComboBox");
            var addMenuItemButton = this.FindControl<TextBlock>("AddMenuItemButton");
            var removeMenuItemButton = this.FindControl<TextBlock>("RemoveMenuItemButton");
            var waiterPanel = this.FindControl<StackPanel>("WaiterPanel");
            var paymentPanel = this.FindControl<StackPanel>("PaymentPanel");
            var statusPanel = this.FindControl<StackPanel>("StatusPanel");
            var tablePanel = this.FindControl<StackPanel>("TablePanel");
            var scrollViewer = this.FindControl<ScrollViewer>("MenuScrollViewer");
            var scrollViewerFalse = this.FindControl<ScrollViewer>("MenuScrollViewerFalse");

            if (Role == "администратор")
            {
                waiterComboBox.IsVisible = true;
                tableInput.IsVisible = true;
                addMenuItemButton.IsVisible = true;
                removeMenuItemButton.IsVisible = true;
                waiterPanel.IsVisible = false;
                tablePanel.IsVisible = false;
                var dataRepository = new DataRepository(_databaseService);
                ListWaiter = new ObservableCollection<string>(
                    dataRepository.Employees
                        .Where(e => e.Role.ToLower() == "официант" && e.EmploymentStatus)
                        .Select(w => $"{w.Surname} {w.Name} {w.Patronymic}".Trim())
                );
                waiterComboBox.ItemsSource = ListWaiter;
            }
            else if (Role == "официант")
            {
                // ДЛЯ ОФИЦИАНТА: показываем поля для ввода, скрываем текстовые блоки
                waiterComboBox.IsVisible = false;  // Скрываем выбор официанта
                tableInput.IsVisible = true;       // ПОКАЗЫВАЕМ поле ввода столика
                addMenuItemButton.IsVisible = true; // ПОКАЗЫВАЕМ кнопки добавления/удаления
                removeMenuItemButton.IsVisible = true;
                waiterPanel.IsVisible = true;     // Скрываем текстовый блок официанта
                tablePanel.IsVisible = false;      // Скрываем текстовый блок столика
        
                // Автоматически устанавливаем текущего официанта
                if (CurrentUser.IsAuthenticated)
                {
                    var waiterTextBlock = this.FindControl<TextBlock>("WaiterTextBlock");
                    if (waiterTextBlock != null)
                    {
                        waiterTextBlock.Text = CurrentUser.FullName;
                    }
            
                    // Также устанавливаем в комбобокс (если он используется)
                    waiterComboBox.SelectedItem = CurrentUser.FullName;
                }
            }
            else
            {
                // Для других ролей (повар и т.д.)
                waiterComboBox.IsVisible = false;
                tableInput.IsVisible = false;
                addMenuItemButton.IsVisible = false;
                removeMenuItemButton.IsVisible = false;
                waiterPanel.IsVisible = true;
                tablePanel.IsVisible = true;
            }

            statusComboBox.IsVisible = true;
            scrollViewerFalse.IsVisible = false;
            scrollViewer.IsVisible = true;
            statusPanel.IsVisible = false;
            paymentPanel.IsVisible = false;

            var saveButton = this.FindControl<global::CafeApp.Controls.Components.Button.Button>("SaveButton");
            if (saveButton != null) saveButton.IsVisible = true;
        }
   
        public decimal CalculateTotalPrice()
        {
            decimal total = 0;
            foreach (var menuItem in OrderMenuItems)
            {
                if (!string.IsNullOrEmpty(menuItem.SelectedMenuItem) && menuItem.Quantity > 0)
                {
                    var menuItemPrice = _databaseService.GetMenuItemPriceByName(menuItem.SelectedMenuItem);
                    if (menuItemPrice > 0)
                    { total += menuItemPrice * (menuItem.Quantity ?? 0); }
                }
            }
            return total;
        }

        private void UpdateStatusOrder()
        {
            StatusOrder.Clear();
            if (Role == "администратор")
            {
                StatusOrder.Add("принят");
                StatusOrder.Add("оплачен");
                StatusOrder.Add("готовится");
                StatusOrder.Add("готов");
            }
            else if (Role == "повар")
            {
                StatusOrder.Add("готовится");
                StatusOrder.Add("готов");
            }
            else if (Role == "официант")
            {
                StatusOrder.Add("принят");
                StatusOrder.Add("оплачен");
            }
            else
            {
                StatusOrder.Add("принят");
                StatusOrder.Add("оплачен");
            }
            var paymentSelectionComboBox = this.FindControl<global::CafeApp.Controls.Components.ComboBox.ComboBox>("PaymentSelectionComboBox");

            if (GetComboBoxValue("StatusComboBox") != "оплачен")
            {
                 paymentSelectionComboBox.IsVisible = false;
            }
            
        }

        private void OnAddMenuItemClicked(object sender, PointerPressedEventArgs e)
        {
            OrderMenuItems.Add(new OrderMenuItem()); 
            UpdateTotalPrice();
        }
        private void UpdateTotalPrice()
        { TotalPrice = CalculateTotalPrice(); }
      
        private void OnRemoveMenuItemClicked(object sender, PointerPressedEventArgs e)
        { 
            if (OrderMenuItems.Count > 1) 
                OrderMenuItems.RemoveAt(OrderMenuItems.Count - 1); 
            UpdateTotalPrice();
        }

       private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string status = GetComboBoxValue("StatusComboBox");
                string waiterName = "";
                int tableId = 0;
                var paymentSelectionComboBox = this.FindControl<global::CafeApp.Controls.Components.ComboBox.ComboBox>("PaymentSelectionComboBox");

                if (GetComboBoxValue("StatusComboBox") != "оплачен")
                { paymentSelectionComboBox.IsVisible = false; }
                else
                { paymentSelectionComboBox.IsVisible = true; }
                if (Role == "администратор")
                {
                    waiterName = GetComboBoxValue("WaiterComboBox");
                    var tableInput = this.FindControl<Input>("TableInput");
                    string tableNumber = tableInput.Value?.ToString() ?? tableInput.Text ?? "";
                    
                    if (string.IsNullOrEmpty(waiterName) || string.IsNullOrEmpty(tableNumber))
                    { return; }
                    
                    if (!int.TryParse(tableNumber, out tableId) || tableId <= 0)
                    { return; }
                }
                else
                {
                    string tableNumber = "";
                    var waiterTextBlock = this.FindControl<TextBlock>("WaiterTextBlock");
                    var tableTextBlock = this.FindControl<TextBlock>("TableTextBlock");
                    var tableInput = this.FindControl<Input>("TableInput");
                    if (Title == "Новый заказ")
                    {
                        waiterName = waiterTextBlock?.Text ?? "";
                        tableNumber = tableInput.Value?.ToString() ?? tableInput.Text ?? "";
                    }
                    else
                    {
                        waiterName = waiterTextBlock?.Text ?? "";
                        tableNumber = tableTextBlock?.Text ?? "";
                    }
                    File.AppendAllText(@"A:\Инженерно-техническая поддержка сопровождения ИС\debug.log", 
                        $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - Saving order: Waiter={waiterName}, tb={tableNumber} S\n");
                    
                    if (string.IsNullOrEmpty(waiterName) || string.IsNullOrEmpty(tableNumber))
                    { return; }
                    
                    if (!int.TryParse(tableNumber, out tableId) || tableId <= 0)
                    { return; }
                }
                var orderItems = CollectOrderItemsFromUI();
                if (orderItems.Count == 0)
                { return; }
                int waiterId = _databaseService.GetWaiterIdByName(waiterName);
                if (waiterId == -1)
                {
                    File.AppendAllText(@"A:\Инженерно-техническая поддержка сопровождения ИС\debug.log", 
                        $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - VALIDATION ERROR: Waiter not found: {waiterName}\n");
                    return;
                }
                int shiftId = 1;
                int customerCount = 1;

                bool success;
                int currentOrderId = this.OrderId;

                File.AppendAllText(@"A:\Инженерно-техническая поддержка сопровождения ИС\debug.log", 
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - Saving order: Title={Title}, OrderId={OrderId}, Table={tableId}, Waiter={waiterId}, Status={status}, Items={orderItems.Count}\n");

                if (Title == "Редактирование заказа" && OrderId > 0)
                { 
                    if (status == "оплачен")
                    { 
                        if (string.IsNullOrEmpty(GetComboBoxValue("PaymentSelectionComboBox")))
                        { return; }
                    }
                    success = _databaseService.UpdateOrder(OrderId, tableId, waiterId, status, orderItems);
                    
                }
                else
                {
                    int newOrderId = _databaseService.CreateOrder(tableId, waiterId, shiftId, customerCount, status, orderItems);
                    success = newOrderId > 0;
                    currentOrderId = newOrderId; 
                }
                
                if (success)
                { 
                    if (currentOrderId > 0 && this.OrderId <= 0)
                    { this.OrderId = currentOrderId; }

                    if (status == "оплачен")
                    { 
                        string paymentType = GetComboBoxValue("PaymentSelectionComboBox");
                        if (!string.IsNullOrEmpty(paymentType) && currentOrderId > 0)
                        {
                            _databaseService.UpdatePaymentOrder(currentOrderId, paymentType);
                            GenerateReceipt();
                        }
                    }
                    
                    ClearForm();
                    SaveButtonClicked?.Invoke(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText(@"A:\Инженерно-техническая поддержка сопровождения ИС\debug.log", 
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - EXCEPTION in SaveButton_Click: {ex.Message}\n{ex.StackTrace}\n");
            }
        }
        private void OpenFileInExplorer(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    System.Diagnostics.Process.Start("explorer.exe", $"/select, \"{filePath}\"");
                }
                else
                {
                    string directory = Path.GetDirectoryName(filePath);
                    if (Directory.Exists(directory))
                    { System.Diagnostics.Process.Start("explorer.exe", $"\"{directory}\""); }
                }
            }
            catch (Exception ex)
            {
                string logPath = @"A:\Инженерно-техническая поддержка сопровождения ИС\debug.log";
                File.AppendAllText(logPath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - ERROR opening explorer: {ex.Message}\n");
            }
        }
        private int GetCurrentTableId()
        {
            try
            {
                if (Role == "администратор")
                {
                    var tableInput = this.FindControl<Input>("TableInput");
                    if (tableInput != null && !string.IsNullOrEmpty(tableInput.Text))
                    {
                        if (int.TryParse(tableInput.Text, out int tableId))
                        {
                            return tableId;
                        }
                    }
                }
                else
                {
                    var tableTextBlock = this.FindControl<TextBlock>("TableTextBlock");
                    if (tableTextBlock != null && !string.IsNullOrEmpty(tableTextBlock.Text))
                    {
                        if (int.TryParse(tableTextBlock.Text, out int tableId))
                        {
                            return tableId;
                        }
                    }
                }
        
                // Если не удалось получить из UI, пробуем из базы
                var orderInfo = _databaseService.GetOrderById(OrderId);
                return orderInfo?.TableId ?? 0;
            }
            catch (Exception ex)
            {
                string logPath = @"A:\Инженерно-техническая поддержка сопровождения ИС\debug.log";
                File.AppendAllText(logPath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - ERROR in GetCurrentTableId: {ex.Message}\n");
                return 0;
            }
        }


        private void GenerateReceipt()
        {
            string paymentType = GetComboBoxValue("PaymentSelectionComboBox");
            
            if (!string.IsNullOrEmpty(paymentType))
            {
                bool paymentUpdated = _databaseService.UpdatePaymentOrder(OrderId, paymentType);
                if (paymentUpdated)
                {
                    int correctTableId = GetCurrentTableId();
                    
                    var receiptData = _databaseService.GetReceiptOrderData(OrderId);
                    receiptData.TableId = correctTableId;
                    string logPath = @"A:\Инженерно-техническая поддержка сопровождения ИС\debug.log";
                    File.AppendAllText(logPath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - GenerateReceipt: OrderId={OrderId}, TableId={receiptData.TableId}, WaiterName={receiptData.WaiterName}\n");
                    string filePath = _excelService.GenerateReceiptOrder(receiptData);
                    
                    if (!string.IsNullOrEmpty(filePath))
                    { OpenFileInExplorer(filePath); }
                }
            }
        }

        private string GetComboBoxValue(string comboBoxName)
        {
            var comboBox = this.FindControl<global::CafeApp.Controls.Components.ComboBox.ComboBox>(comboBoxName);
            var innerComboBox = comboBox?.FindControl<ComboBox>("MainComboBox");
            return innerComboBox?.SelectedItem?.ToString() ?? "";
        }

        private List<OrderItem> CollectOrderItemsFromUI()
        {
            var orderItems = new List<OrderItem>();
    
            File.AppendAllText(@"A:\Инженерно-техническая поддержка сопровождения ИС\debug.log", 
                $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - CollectOrderItemsFromUI: OrderMenuItems count = {OrderMenuItems.Count}\n");

            foreach (var menuItem in OrderMenuItems)
            {
                File.AppendAllText(@"A:\Инженерно-техническая поддержка сопровождения ИС\debug.log", 
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - Checking item: SelectedMenuItem='{menuItem.SelectedMenuItem}', Quantity={menuItem.Quantity}\n");

                if (!string.IsNullOrEmpty(menuItem.SelectedMenuItem) && menuItem.Quantity > 0)
                {
                    int menuItemId = _databaseService.GetMenuItemIdByName(menuItem.SelectedMenuItem);
                    File.AppendAllText(@"A:\Инженерно-техническая поддержка сопровождения ИС\debug.log", 
                        $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - MenuItemId found: {menuItemId} for '{menuItem.SelectedMenuItem}'\n");

                    if (menuItemId != -1)
                    {
                        orderItems.Add(new OrderItem
                        {
                            MenuItemId = menuItemId,
                            Quantity = menuItem.Quantity ?? 0,
                        });
                        File.AppendAllText(@"A:\Инженерно-техническая поддержка сопровождения ИС\debug.log", 
                            $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - Added to order items: {menuItem.SelectedMenuItem}, Quantity: {menuItem.Quantity}\n");
                    }
                }
                else
                {
                    File.AppendAllText(@"A:\Инженерно-техническая поддержка сопровождения ИС\debug.log", 
                        $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - Item skipped: Empty selection or quantity\n");
                }
            }
    
            File.AppendAllText(@"A:\Инженерно-техническая поддержка сопровождения ИС\debug.log", 
                $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - Final order items count: {orderItems.Count}\n");
    
            return orderItems;
        }
        
        public void ClearForm()
        {
            OrderMenuItems.Clear();
            OrderMenuItems.Add(new OrderMenuItem());
    
            var tableInput = this.FindControl<global::CafeApp.Controls.Components.Input.Input>("TableInput");
            if (tableInput != null) tableInput.Text = "";
            
            var waiterTextBlock = this.FindControl<TextBlock>("WaiterTextBlock");
            var tableTextBlock = this.FindControl<TextBlock>("TableTextBlock");
    
            if (waiterTextBlock != null) waiterTextBlock.Text = "";
            if (tableTextBlock != null) tableTextBlock.Text = "";
        
            ClearComboBox("StatusComboBox");
            ClearComboBox("WaiterComboBox");
            ClearComboBox("PaymentSelectionComboBox");
    
            this.OrderId = -1;
            this.Title = "Заказ";
            UpdateTotalPrice();
        }

        private void ClearComboBox(string comboBoxName)
        {
            var comboBox = this.FindControl<global::CafeApp.Controls.Components.ComboBox.ComboBox>(comboBoxName);
            var innerComboBox = comboBox?.FindControl<ComboBox>("MainComboBox");
            innerComboBox?.ClearValue(ComboBox.SelectedItemProperty);
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            
            var addButton = this.FindControl<TextBlock>("AddMenuItemButton");
            var removeButton = this.FindControl<TextBlock>("RemoveMenuItemButton");
            var saveButton = this.FindControl<global::CafeApp.Controls.Components.Button.Button>("SaveButton");
            
            if (addButton != null) addButton.PointerPressed += OnAddMenuItemClicked;
            if (removeButton != null) removeButton.PointerPressed += OnRemoveMenuItemClicked;
            if (saveButton != null) saveButton.PointerPressed += SaveButton_Click;
            
            var paymentSelectionComboBox = this.FindControl<global::CafeApp.Controls.Components.ComboBox.ComboBox>("PaymentSelectionComboBox");
            paymentSelectionComboBox.IsVisible = GetComboBoxValue("StatusComboBox") == "оплачен";
            UpdateTotalPrice();
        }
        
    }

    public class OrderMenuItem : INotifyPropertyChanged
    {
        private string _selectedMenuItem = "";
        private int? _quantity = 1;

        public string SelectedMenuItem
        {
            get => _selectedMenuItem;
            set
            {
                _selectedMenuItem = value;
                OnPropertyChanged(nameof(SelectedMenuItem));
            }
        }

        public int? Quantity
        {
            get => _quantity;
            set
            {
                _quantity = value;
                OnPropertyChanged(nameof(Quantity));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}