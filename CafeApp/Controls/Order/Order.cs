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
            { paymentSelectionComboBox.IsVisible = false; }
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
            ListWaiter = new ObservableCollection<string>();
            StatusOrder = new ObservableCollection<string>();
            OrderMenuItems = new ObservableCollection<OrderMenuItem> { new OrderMenuItem() };
            
            var waiters = dataRepository.Employees
                .Where(e => e.Role.ToLower() == "официант" && e.EmploymentStatus)
                .Select(w => $"{w.Surname} {w.Name} {w.Patronymic}".Trim());
    
            foreach (var waiter in waiters)
            { ListWaiter.Add(waiter); }

            AllMenuItems = new ObservableCollection<string>(
                dataRepository.MenuItems.OrderBy(m => m.Name).Select(m => m.Name)
            );

            this.DataContext = this;
            
            ResetAllComponents();
        }

        public void HideComponentsOrder()
        {
            var tableInput = this.FindControl<Input>("TableInput");
            var statusComboBox = this.FindControl<global::CafeApp.Controls.Components.ComboBox.ComboBox>("StatusComboBox");
            var waiterComboBox = this.FindControl<global::CafeApp.Controls.Components.ComboBox.ComboBox>("WaiterComboBox");
            var addMenuItemButton = this.FindControl<TextBlock>("AddMenuItemButton");
            var removeMenuItemButton = this.FindControl<TextBlock>("RemoveMenuItemButton");
            var waiterPanel = this.FindControl<StackPanel>("WaiterPanel");
            var tablePanel = this.FindControl<StackPanel>("TablePanel");
            var scrollViewer = this.FindControl<ScrollViewer>("MenuScrollViewer");
            var scrollViewerFalse = this.FindControl<ScrollViewer>("MenuScrollViewerFalse");
            var customerCountPanel = this.FindControl<StackPanel>("CustomerCountPanel");
            var customerCountInput = this.FindControl<Input>("CustomerCountInput");
            var statusPanel = this.FindControl<StackPanel>("StatusPanel");
            var paymentPanel = this.FindControl<StackPanel>("PaymentPanel");
            var paymentSelectionComboBox = this.FindControl<global::CafeApp.Controls.Components.ComboBox.ComboBox>("PaymentSelectionComboBox");
            
            ResetAllComponents();
            
            if (Title == "Редактирование заказа")
            {
                if (Role == "администратор")
                {
                    waiterComboBox.IsVisible = true;
                    tableInput.IsVisible = true;
                    customerCountInput.IsVisible = true;
                    statusComboBox.IsVisible = true;
                    addMenuItemButton.IsVisible = true;
                    removeMenuItemButton.IsVisible = true;
                    scrollViewer.IsVisible = true;
                    paymentSelectionComboBox.IsVisible = GetComboBoxValue("StatusComboBox") == "оплачен";
                }
                else if (Role == "официант" || Role == "повар")
                {
                    statusComboBox.IsVisible = true;
                    scrollViewerFalse.IsVisible = true;
                    waiterPanel.IsVisible = true;
                    tablePanel.IsVisible = true;
                    customerCountPanel.IsVisible = true;
                    
                }
            }
            else if (Title == "Новый заказ")
            {
                if (Role == "администратор")
                {
                    waiterComboBox.IsVisible = true;
                    tableInput.IsVisible = true;
                    customerCountInput.IsVisible = true;
                    statusComboBox.IsVisible = true;
                    addMenuItemButton.IsVisible = true;
                    removeMenuItemButton.IsVisible = true;
                    scrollViewer.IsVisible = true;
                }
                else if (Role == "официант")
                {
                    tableInput.IsVisible = true;
                    customerCountInput.IsVisible = true;
                    statusComboBox.IsVisible = true;
                    addMenuItemButton.IsVisible = true;
                    removeMenuItemButton.IsVisible = true;
                    scrollViewer.IsVisible = true;
                    waiterPanel.IsVisible = true;
                    if (CurrentUser.IsAuthenticated)
                    {
                        var waiterTextBlock = this.FindControl<TextBlock>("WaiterTextBlock");
                        waiterTextBlock.Text = CurrentUser.FullName;
                    }
                }
            }
            else if (Title == "Просмотр заказа")
            {
                scrollViewerFalse.IsVisible = true;
                waiterPanel.IsVisible = true;
                tablePanel.IsVisible = true;
                customerCountPanel.IsVisible = true;
                statusPanel.IsVisible = true;
                paymentPanel.IsVisible = true;
            }
        }
        
        public void LoadOrderData(int orderId, string role)
        {
            this.OrderId = orderId;
            this.Role = role;
            var orderInfo = _databaseService.GetOrderById(orderId);
            this.Title = orderInfo.Status == "оплачен" ? "Просмотр заказа" : "Редактирование заказа";
            
            var waiterComboBox = this.FindControl<global::CafeApp.Controls.Components.ComboBox.ComboBox>("WaiterComboBox");
            var customerCountInput = this.FindControl<Input>("CustomerCountInput");
            var waiterTextBlock = this.FindControl<TextBlock>("WaiterTextBlock");
            var tableTextBlock = this.FindControl<TextBlock>("TableTextBlock");
            var customerCountTextBlock = this.FindControl<TextBlock>("CustomerCountTextBlock");
            var tableInput = this.FindControl<Input>("TableInput");
            var statusTextBlock = this.FindControl<TextBlock>("StatusTextBlock");
            var paymentTextBlock = this.FindControl<TextBlock>("PaymentTextBlock");
            
            if (Role == "администратор")
            {
                waiterComboBox.ItemsSource = ListWaiter;
                waiterComboBox.SelectedItem = orderInfo.WaiterName;
                tableInput.Value = orderInfo.TableId.ToString();
                customerCountInput.Value = orderInfo.CustomerCount.ToString();
            }
            else
            {
                if (waiterTextBlock != null) waiterTextBlock.Text = orderInfo.WaiterName;
                if (tableTextBlock != null) tableTextBlock.Text = orderInfo.TableId.ToString();
                if (customerCountTextBlock != null) customerCountTextBlock.Text = orderInfo.CustomerCount.ToString();
                if (statusTextBlock != null) statusTextBlock.Text = orderInfo.Status;
                string paymentType = _databaseService.GetOrderPaymentType(this.OrderId);
                if (paymentTextBlock != null) paymentTextBlock.Text = paymentType;
            }

            if (Title == "Просмотр заказа")
            {
                if (waiterTextBlock != null) waiterTextBlock.Text = orderInfo.WaiterName;
                if (tableTextBlock != null) tableTextBlock.Text = orderInfo.TableId.ToString();
                if (customerCountTextBlock != null) customerCountTextBlock.Text = orderInfo.CustomerCount.ToString();
                if (statusTextBlock != null) statusTextBlock.Text = orderInfo.Status;
                string paymentType = _databaseService.GetOrderPaymentType(this.OrderId);
                if (paymentTextBlock != null) paymentTextBlock.Text = paymentType;
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
            HideComponentsOrder();
        }

       private void ResetAllComponents()
        {
            var tableInput = this.FindControl<Input>("TableInput");
            var statusComboBox = this.FindControl<global::CafeApp.Controls.Components.ComboBox.ComboBox>("StatusComboBox");
            var waiterComboBox = this.FindControl<global::CafeApp.Controls.Components.ComboBox.ComboBox>("WaiterComboBox");
            var addMenuItemButton = this.FindControl<TextBlock>("AddMenuItemButton");
            var removeMenuItemButton = this.FindControl<TextBlock>("RemoveMenuItemButton");
            var waiterPanel = this.FindControl<StackPanel>("WaiterPanel");
            var tablePanel = this.FindControl<StackPanel>("TablePanel");
            var scrollViewer = this.FindControl<ScrollViewer>("MenuScrollViewer");
            var scrollViewerFalse = this.FindControl<ScrollViewer>("MenuScrollViewerFalse");
            var customerCountPanel = this.FindControl<StackPanel>("CustomerCountPanel");
            var customerCountInput = this.FindControl<Input>("CustomerCountInput");
            var statusPanel = this.FindControl<StackPanel>("StatusPanel");
            var paymentPanel = this.FindControl<StackPanel>("PaymentPanel");
            var paymentSelectionComboBox = this.FindControl<global::CafeApp.Controls.Components.ComboBox.ComboBox>("PaymentSelectionComboBox");

            tableInput.IsVisible = false;
            statusComboBox.IsVisible = false;
            waiterComboBox.IsVisible = false;
            addMenuItemButton.IsVisible = false;
            removeMenuItemButton.IsVisible = false;
            waiterPanel.IsVisible = false;
            tablePanel.IsVisible = false;
            scrollViewer.IsVisible = false;
            scrollViewerFalse.IsVisible = false;
            customerCountPanel.IsVisible = false;
            customerCountInput.IsVisible = false;
            statusPanel.IsVisible = false;
            paymentPanel.IsVisible = false;
            paymentSelectionComboBox.IsVisible = false;
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

      
       private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string status = GetComboBoxValue("StatusComboBox");
                if (Title == "Просмотр заказа" && Title == "официант")
                {
                    GenerateReceipt();
                    return;
                }

                string waiterName = "";
                int tableId = 0;
                int countCustomer = 0;
                
                var paymentSelectionComboBox = this.FindControl<global::CafeApp.Controls.Components.ComboBox.ComboBox>("PaymentSelectionComboBox");
                var tableInput = this.FindControl<Input>("TableInput");
                var tableTextBlock = this.FindControl<TextBlock>("TableTextBlock");
                var customerCountInput = this.FindControl<Input>("CustomerCountInput");
                var customerCountTextBlock = this.FindControl<TextBlock>("CustomerCountTextBlock");
                var waiterTextBlock = this.FindControl<TextBlock>("WaiterTextBlock");
                
                paymentSelectionComboBox.IsVisible = GetComboBoxValue("StatusComboBox") == "оплачен";
                
                if (Role == "администратор")
                {
                    waiterName = GetComboBoxValue("WaiterComboBox");
                    string tableNumber = tableInput.Value?.ToString() ?? tableInput.Text ?? "";
                    string customerCount = customerCountInput.Value?.ToString() ?? customerCountInput.Text ?? "";
                    
                    if (string.IsNullOrEmpty(waiterName) || string.IsNullOrEmpty(tableNumber) || string.IsNullOrEmpty(customerCount))
                        return;
                    if (!int.TryParse(tableNumber, out tableId) || tableId <= 0)
                        return;
                    if (!int.TryParse(customerCount, out countCustomer) || countCustomer <= 0)
                        return;
                }
                else
                {
                    string tableNumber = "";
                    string customerCount = "";
                    if (Title == "Новый заказ")
                    {
                        waiterName = waiterTextBlock?.Text ?? "";
                        tableNumber = tableInput.Value?.ToString() ?? tableInput.Text ?? "";
                        customerCount = customerCountInput.Value?.ToString() ?? customerCountInput.Text ?? "";
                    }
                    else
                    {
                        waiterName = waiterTextBlock?.Text ?? "";
                        tableNumber = tableTextBlock?.Text ?? "";
                        customerCount = customerCountTextBlock?.Text ?? "";
                    }
                    
                    if (string.IsNullOrEmpty(waiterName) || string.IsNullOrEmpty(tableNumber) || string.IsNullOrEmpty(customerCount))
                        return;
                    
                    if (!int.TryParse(tableNumber, out tableId) || tableId <= 0)
                        return;
                    if (!int.TryParse(customerCount, out countCustomer) || countCustomer <= 0)
                        return;
                }
                
                var orderItems = CollectOrderItemsFromUI();
                if (orderItems.Count == 0)
                    return;
                    
                int waiterId = _databaseService.GetWaiterIdByName(waiterName);
                if (waiterId == -1)
                {
                    File.AppendAllText(@"A:\Инженерно-техническая поддержка сопровождения ИС\debug.log", 
                        $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - VALIDATION ERROR: Waiter not found: {waiterName}\n");
                    return;
                }
                
                int shiftId = _databaseService.GetCurrentOrLatestShiftId();
                bool success = false;
                int currentOrderId = this.OrderId;

                if (Title == "Редактирование заказа" && OrderId > 0)
                { 
                    if (status == "оплачен")
                    { 
                        if (string.IsNullOrEmpty(GetComboBoxValue("PaymentSelectionComboBox")))
                            return;
                    }
                    success = _databaseService.UpdateOrder(OrderId, tableId, waiterId, status, orderItems, countCustomer);
                }
                else if (Title == "Новый заказ") 
                {
                    int newOrderId = _databaseService.CreateOrder(tableId, waiterId, shiftId, countCustomer, status, orderItems);
                    success = newOrderId > 0;
                    currentOrderId = newOrderId; 
                }
                
                if (success)
                { 
                    if (currentOrderId > 0 && this.OrderId <= 0)
                        this.OrderId = currentOrderId;

                    if (status == "оплачен" )
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
  
        private void GenerateReceipt()
        {
            var paymentTextBlock = this.FindControl<TextBlock>("PaymentTextBlock");
            string paymentType = "";
            
            if (Title == "Просмотр заказа")
            { paymentType = paymentTextBlock?.Text ?? ""; }
            else
            { paymentType = GetComboBoxValue("PaymentSelectionComboBox"); }
            

            if (!string.IsNullOrEmpty(paymentType))
            {
                bool paymentUpdated = _databaseService.UpdatePaymentOrder(OrderId, paymentType);
                if (paymentUpdated) {
                    int correctTableId = GetCurrentTableId();
                    var receiptData = _databaseService.GetReceiptOrderData(OrderId);
                    receiptData.TableId = correctTableId;
                    string filePath = _excelService.GenerateReceiptOrder(receiptData);
                    
                    if (!string.IsNullOrEmpty(filePath))
                    { OpenFileInExplorer(filePath); }
                }
            }
        }
        private void OpenFileInExplorer(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                { System.Diagnostics.Process.Start("explorer.exe", $"/select, \"{filePath}\""); }
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
                        { return tableId; }
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
        
        private string GetComboBoxValue(string comboBoxName)
        {
            var comboBox = this.FindControl<global::CafeApp.Controls.Components.ComboBox.ComboBox>(comboBoxName);
            var innerComboBox = comboBox?.FindControl<ComboBox>("MainComboBox");
            return innerComboBox?.SelectedItem?.ToString() ?? "";
        }

        private List<OrderItem> CollectOrderItemsFromUI()
        {
            var orderItems = new List<OrderItem>();

            foreach (var menuItem in OrderMenuItems)
            {
                if (!string.IsNullOrEmpty(menuItem.SelectedMenuItem) && menuItem.Quantity > 0)
                {
                    int menuItemId = _databaseService.GetMenuItemIdByName(menuItem.SelectedMenuItem);
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
    
            var tableInput = this.FindControl<Input>("TableInput");
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
        private int? _quantity;

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