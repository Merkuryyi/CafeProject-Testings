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
            AvaloniaProperty.Register<Order, string>(nameof(Title), "Заказ");
        public string Title
        {
            get => GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }
        public string Role
        {
            get => GetValue(RoleProperty);
            set => SetValue(RoleProperty, value);
        }
        public event EventHandler? SaveButtonClicked;

        public Order()
        {
            InitializeComponent();
            
            _databaseService = new DatabaseService();
            var dataRepository = new DataRepository(_databaseService);
            
            // Загрузка данных
            ListWaiter = new ObservableCollection<string>(
                dataRepository.Employees
                    .Where(e => e.Role.ToLower() == "официант" && e.EmploymentStatus)
                    .Select(w => $"{w.Surname} {w.Name} {w.Patronymic}".Trim())
            );

            AllMenuItems = new ObservableCollection<string>(
                dataRepository.MenuItems.OrderBy(m => m.Name).Select(m => m.Name)
            );

            StatusOrder = new ObservableCollection<string> 
            {
                "принят", "оплачен"
            };
            /*   if ()
            {
                
            }
            ObservableCollection<string> StatusOrdeWaiter = new ObservableCollection<string> 
            {
                "Принят", "Оплачен"
            };
            ObservableCollection<string> StatusOrderCook = new ObservableCollection<string> 
            {
                "Готовится", "Готов"
            };*/
         

            OrderMenuItems = new ObservableCollection<OrderMenuItem> { new OrderMenuItem() };
            
            this.DataContext = this;
        }
     
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
            
            var itemsControl = this.FindControl<ItemsControl>("MenuItemsControl");
            if (itemsControl == null) return orderItems;

            // Проходим по всем контейнерам ItemsControl
            for (int i = 0; i < itemsControl.ItemCount; i++)
            {
                var container = itemsControl.ItemContainerGenerator.ContainerFromIndex(i);
                if (container != null)
                {
                    // Находим ComboBox с блюдами и Input с количеством
                    var menuComboBox = container.FindControl<global::CafeApp.Controls.Components.ComboBox.ComboBox>("MenuItemComboBox");
                    var quantityInput = container.FindControl<global::CafeApp.Controls.Components.Input.Input>("QuantityInput");

                    if (menuComboBox != null && quantityInput != null)
                    {
                        string menuItemName = GetComboBoxValue(menuComboBox);
                        string quantityText = quantityInput.Value?.ToString() ?? 
                                            quantityInput.Content?.ToString() ?? 
                                            quantityInput.Text ?? "";

                        File.AppendAllText("A:/Инженерно-техническая поддержка сопровождения ИС/debug.log", 
                            $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - Строка {i}: Блюдо='{menuItemName}', Количество='{quantityText}'\n");

                        if (!string.IsNullOrEmpty(menuItemName) && int.TryParse(quantityText, out int quantity) && quantity > 0)
                        {
                            int menuItemId = _databaseService.GetMenuItemIdByName(menuItemName);
                            if (menuItemId != -1)
                            {
                                decimal price = _databaseService.GetMenuItemPrice(menuItemId);
                                orderItems.Add(new OrderItem
                                {
                                    MenuItemId = menuItemId,
                                    Quantity = quantity,
                                    Price = price
                                });
                                
                                File.AppendAllText("A:/Инженерно-техническая поддержка сопровождения ИС/debug.log", 
                                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - Добавлено блюдо: ID={menuItemId}, Количество={quantity}, Цена={price}\n");
                            }
                        }
                    }
                }
            }

            return orderItems;
        }
        private string GetComboBoxValue(global::CafeApp.Controls.Components.ComboBox.ComboBox comboBox)
        {
            if (comboBox == null) return string.Empty;
    
            var innerComboBox = comboBox.FindControl<ComboBox>("MainComboBox");
            return innerComboBox?.SelectedItem?.ToString() ?? string.Empty;
        }

        private void ClearForm()
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