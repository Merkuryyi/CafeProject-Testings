using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CafeApp.Data;
using CafeApp.Models;
using System.Linq;
using CafeApp.Database;
using System;
using Avalonia.Interactivity;
using System.IO;
using CafeApp.Controls.Components.Input;

namespace CafeApp.Controls
{
    public partial class Order : UserControl
    {
        private readonly DatabaseService _databaseService;

        public ObservableCollection<string> StatusOrder { get; set; }
     
        public ObservableCollection<string> AllMenuItems { get; set; }
        public ObservableCollection<string> ListWaiter { get; set; }
        public ObservableCollection<OrderMenuItem> OrderMenuItems { get; set; }
        
        public static readonly StyledProperty<string> TitleProperty =
            AvaloniaProperty.Register<Order, string>(nameof(Title), "Заказ");
        
        public static readonly StyledProperty<string> RoleProperty =
            AvaloniaProperty.Register<Order, string>(nameof(Role), "администратор");
        public static readonly StyledProperty<int> OrderIdProperty =
            AvaloniaProperty.Register<Order, int>(nameof(OrderId), -1);
        
        public string Title
        {
            get => GetValue(TitleProperty);
            set
            {
                SetValue(TitleProperty, value);
              
            } 
            
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
        
      
        public void HideComponentsOrder()
        {
            if (this.Title == "Редактирование заказа")
            {
                var tableInput = this.FindControl<Input>("TableInput");
                //var quantityInput = this.FindControl<Input>("QuantityInput");
        
                var statusComboBox = this.FindControl<global::CafeApp.Controls.Components.ComboBox.ComboBox>("StatusComboBox");
                statusComboBox.ItemsSource = StatusOrder;
      
            
                var waiterComboBox = this.FindControl<global::CafeApp.Controls.Components.ComboBox.ComboBox>("WaiterComboBox");
                waiterComboBox.ItemsSource = ListWaiter;
                
                var addMenuItemButton = this.FindControl<TextBlock>("AddMenuItemButton");
                var removeMenuItemButton = this.FindControl<TextBlock>("RemoveMenuItemButton");
                
                var waiterPanel = this.FindControl<StackPanel>("WaiterPanel");
                var tablePanel = this.FindControl<StackPanel>("TablePanel");
                var scrollViewer = this.FindControl<ScrollViewer>("MenuScrollViewer");
                var scrollViewerFalse = this.FindControl<ScrollViewer>("MenuScrollViewerFalse");
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
            
        }
        public void LoadOrderData(int orderId, string role)
        {
            try
            {
                this.OrderId = orderId;
                HideComponentsOrder();
                var orderInfo = _databaseService.GetOrderById(orderId);
               
                    File.AppendAllText("A:/Инженерно-техническая поддержка сопровождения ИС/debug.log", 
                        $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - Order not found: {orderId}\n");

                    if (Role == "администратор")
                    {
                        var statusComboBox = this.FindControl<global::CafeApp.Controls.Components.ComboBox.ComboBox>("StatusComboBox");
                        statusComboBox.ItemsSource = StatusOrder;
                        statusComboBox.SelectedItem = orderInfo.Status;
                        

                        // Заполняем официанта
                        var waiterComboBox = this.FindControl<global::CafeApp.Controls.Components.ComboBox.ComboBox>("WaiterComboBox");
                        waiterComboBox.ItemsSource = ListWaiter;
                        waiterComboBox.SelectedItem = orderInfo.WaiterName;
                       

                        // Заполняем столик
                        var tableInput = this.FindControl<Input>("TableInput");
                        tableInput.Value = orderInfo.TableId.ToString();
                        

                        // Заполняем элементы меню
                        OrderMenuItems.Clear();
                        foreach (var item in orderInfo.Items)
                        {
                            OrderMenuItems.Add(new OrderMenuItem
                            {
                                SelectedMenuItem = item.MenuItemName,
                                Quantity = item.Quantity
                            });
            
                            File.AppendAllText("A:/Инженерно-техническая поддержка сопровождения ИС/debug.log", 
                                $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - Added menu item: {item.MenuItemName}, Quantity: {item.Quantity}\n");
                        }
                        OrderMenuItems.Add(new OrderMenuItem());
                        
                    }
                    else
                    {
                        
                    }

                

                File.AppendAllText("A:/Инженерно-техническая поддержка сопровождения ИС/debug.log", 
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - Order loaded successfully. Total items: {OrderMenuItems.Count}\n");
            }
            catch (Exception ex)
            {
                File.AppendAllText("A:/Инженерно-техническая поддержка сопровождения ИС/debug.log", 
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - ERROR loading order: {ex.Message}\n{ex.StackTrace}\n");
            }
        }

        public event EventHandler? SaveButtonClicked;

        public Order()
        {
            InitializeComponent();
            
            _databaseService = new DatabaseService();
            var dataRepository = new DataRepository(_databaseService);
            
            ListWaiter = new ObservableCollection<string>(
                dataRepository.Employees
                    .Where(e => e.Role.ToLower() == "официант" && e.EmploymentStatus)
                    .Select(w => $"{w.Surname} {w.Name} {w.Patronymic}".Trim())
            );

            AllMenuItems = new ObservableCollection<string>(
                dataRepository.MenuItems.OrderBy(m => m.Name).Select(m => m.Name)
            );

            StatusOrder = new ObservableCollection<string>();
            
            OrderMenuItems = new ObservableCollection<OrderMenuItem> { new OrderMenuItem() };
            
            this.DataContext = this;

            string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - Order constructor: Role='{Role}'\n";
            File.AppendAllText("A:/Инженерно-техническая поддержка сопровождения ИС/debug.log", logMessage);
            
            UpdateStatusOrder();
     
        }

        private void UpdateStatusOrder()
        {
            if (StatusOrder == null) return;

            StatusOrder.Clear();

            string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - UpdateStatusOrder: Role='{Role}'\n";
            File.AppendAllText("A:/Инженерно-техническая поддержка сопровождения ИС/debug.log", logMessage);

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

            // ДОБАВЛЕНО: Логируем обновленные статусы
          //  logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - StatusOrder updated: {string.Join(", ", StatusOrder)}\n";
          //  File.AppendAllText("A:/Инженерно-техническая поддержка сопровождения ИС/debug.log", logMessage);
        }

        // Остальные методы без изменений...
        private void OnAddMenuItemClicked(object sender, PointerPressedEventArgs e)
        {
            OrderMenuItems.Add(new OrderMenuItem());
        }

        private void OnRemoveMenuItemClicked(object sender, PointerPressedEventArgs e)
        {
            if (OrderMenuItems.Count > 1) 
            {
                OrderMenuItems.RemoveAt(OrderMenuItems.Count - 1);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string status = GetComboBoxValue("StatusComboBox");
                string waiterName = GetComboBoxValue("WaiterComboBox");
                var tableInput = this.FindControl<Input>("TableInput");
             
                string tableNumber =  tableInput.Value?.ToString() ?? 
                                      tableInput.Content?.ToString() ?? 
                                      tableInput.Text ?? "";
                int.TryParse(tableNumber, out int tableId);
             
                string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - Form data saved:\n" +
                                    $"tableId: '{tableId}'\n" +
                                    $"waiterId: '{1}'\n"+
                                    $"status: '{status}'\n" +
                                    $"orderItems: '{1}'\n";
                File.AppendAllText("A:/Инженерно-техническая поддержка сопровождения ИС/debug.log", logMessage);
                int waiterId = _databaseService.GetWaiterIdByName(waiterName);
                var orderItems = CollectOrderItemsFromUI();
                int orderId = _databaseService.CreateOrder(tableId, waiterId, 1, 1, status, orderItems);
                if (orderId > 0)
                {
                    ClearForm();
                    SaveButtonClicked?.Invoke(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText("debug.log", $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - ERROR: {ex.Message}\n");
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
    
            // Используем напрямую коллекцию OrderMenuItems, так как данные уже привязаны
            foreach (var menuItem in OrderMenuItems)
            {
                if (!string.IsNullOrEmpty(menuItem.SelectedMenuItem) && menuItem.Quantity > 0)
                {
                    int menuItemId = _databaseService.GetMenuItemIdByName(menuItem.SelectedMenuItem);
                    if (menuItemId != -1)
                    {
                        decimal price = _databaseService.GetMenuItemPrice(menuItemId);
                        orderItems.Add(new OrderItem
                        {
                            MenuItemId = menuItemId,
                            Quantity = menuItem.Quantity,
                            Price = price
                        });
                
                        File.AppendAllText("A:/Инженерно-техническая поддержка сопровождения ИС/debug.log", 
                            $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - Добавлено блюдо: {menuItem.SelectedMenuItem}, Количество={menuItem.Quantity}, Цена={price}\n");
                    }
                    else
                    {
                        File.AppendAllText("A:/Инженерно-техническая поддержка сопровождения ИС/debug.log", 
                            $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - ERROR: Блюдо '{menuItem.SelectedMenuItem}' не найдено в базе\n");
                    }
                }
            }

            File.AppendAllText("A:/Инженерно-техническая поддержка сопровождения ИС/debug.log", 
                $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - Всего собрано блюд: {orderItems.Count}\n");
    
            return orderItems;
        }

    

        public void ClearForm()
        {
            OrderMenuItems.Clear();
            OrderMenuItems.Add(new OrderMenuItem());
            
            var tableInput = this.FindControl<global::CafeApp.Controls.Components.Input.Input>("TableInput");
            if (tableInput != null)
                tableInput.Text = "";
                
            ClearComboBox("StatusComboBox");
            ClearComboBox("WaiterComboBox");
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
            
            if (addButton != null)
                addButton.PointerPressed += OnAddMenuItemClicked;
                
            if (removeButton != null)
                removeButton.PointerPressed += OnRemoveMenuItemClicked;
                
            if (saveButton != null)
                saveButton.PointerPressed += SaveButton_Click;
        }
    }

    public class OrderMenuItem
    {
        public string SelectedMenuItem { get; set; } = "";
        public int Quantity { get; set; } = 1;
    }
}