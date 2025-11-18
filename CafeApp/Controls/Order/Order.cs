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

namespace CafeApp.Controls
{
    public partial class Order : UserControl
    {
        private readonly DataRepository _dataRepository;
        private readonly DatabaseService _databaseService;
        
        public ObservableCollection<string> StatusOrder { get; set; }
        public ObservableCollection<string> CategoryMenu { get; set; }
        public ObservableCollection<string> AllMenuItems { get; set; }
        public ObservableCollection<string> ListWaiter { get; set; }
        
        // Коллекция для хранения позиций меню в заказе
        public ObservableCollection<OrderMenuItem> OrderMenuItems { get; set; }

        // Событие для кнопки сохранения
        public event EventHandler? SaveButtonClicked;

        public Order()
        {
            InitializeComponent();
            
            _databaseService = new DatabaseService();
            _dataRepository = new DataRepository(_databaseService);
            
            LoadWaiters();
            LoadMenuData();
            LoadOtherData();
            
            // Инициализируем коллекцию позиций меню
            OrderMenuItems = new ObservableCollection<OrderMenuItem>
            {
                new OrderMenuItem() // Первая строка по умолчанию
            };
            
            this.DataContext = this;
        }

        private void LoadWaiters()
        {
            ListWaiter = new ObservableCollection<string>();
            
            var waiters = _dataRepository.Employees
                .Where(e => e.Role.ToLower() == "официант" && e.EmploymentStatus)
                .ToList();

            foreach (var waiter in waiters)
            {
                string fullName = $"{waiter.Surname} {waiter.Name}";
                if (!string.IsNullOrEmpty(waiter.Patronymic))
                {
                    fullName += $" {waiter.Patronymic}";
                }
                ListWaiter.Add(fullName);
            }

            if (!ListWaiter.Any())
            {
                ListWaiter.Add("Официанты не найдены");
            }
        }

        private void LoadMenuData()
        {
            // Загружаем все категории из базы данных (если еще нужны для чего-то)
            CategoryMenu = new ObservableCollection<string>(
                _dataRepository.MenuItems
                    .Select(m => m.Category)
                    .Distinct()
                    .OrderBy(c => c)
            );

            // Загружаем все названия блюд
            AllMenuItems = new ObservableCollection<string>(
                _dataRepository.MenuItems
                    .OrderBy(m => m.Name)
                    .Select(m => m.Name)
            );

            if (!AllMenuItems.Any())
            {
                AllMenuItems.Add("Блюда не найдены");
            }
        }

        private void LoadOtherData()
        {
            StatusOrder = new ObservableCollection<string> 
            {
                "Принят", 
                "Готовится", 
                "Готов",
                "Оплачен"
            };
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
            else if (OrderMenuItems.Count == 1)
            {
                var lastItem = OrderMenuItems[0];
                lastItem.SelectedMenuItem = "";
                lastItem.Quantity = 1;
            }
        }

        // Обработчик нажатия кнопки "Сохранить"
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Получаем контролы из UI
                var statusComboBox = this.FindControl<global::CafeApp.Controls.Components.ComboBox.ComboBox>("StatusComboBox");
                var waiterComboBox = this.FindControl<global::CafeApp.Controls.Components.ComboBox.ComboBox>("WaiterComboBox");
                var tableInput = this.FindControl<global::CafeApp.Controls.Components.Input.Input>("TableInput");
                
                // Получаем выбранные значения
                string status = GetSelectedComboBoxValue(statusComboBox);
                string waiterName = GetSelectedComboBoxValue(waiterComboBox);
                string tableNumber = tableInput?.Text ?? "";

                // Проверка что все поля заполнены
                if (string.IsNullOrEmpty(status) || string.IsNullOrEmpty(waiterName) || string.IsNullOrEmpty(tableNumber))
                {
                    string errorLog = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - ERROR: Not all fields are filled\n";
                    File.AppendAllText("A:/Инженерно-техническая поддержка сопровождения ИС/debug.log", errorLog);
                    return;
                }

                if (!int.TryParse(tableNumber, out int tableId))
                {
                    string errorLog = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - ERROR: Invalid table number\n";
                    File.AppendAllText("A:/Инженерно-техническая поддержка сопровождения ИС/debug.log", errorLog);
                    return;
                }

                // Конвертируем данные
                int waiterId = _databaseService.GetWaiterIdByName(waiterName);
                int shiftId = 1; // ID смены по умолчанию = 1
                int customerCount = 1; // Количество гостей по умолчанию

                if (waiterId == -1)
                {
                    string errorLog = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - ERROR: Waiter not found\n";
                    File.AppendAllText("A:/Инженерно-техническая поддержка сопровождения ИС/debug.log", errorLog);
                    return;
                }

                // Собираем данные о блюдах
                var orderItems = CollectOrderItemsFromUI();
                if (orderItems.Count == 0)
                {
                    string errorLog = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - ERROR: No menu items added\n";
                    File.AppendAllText("A:/Инженерно-техническая поддержка сопровождения ИС/debug.log", errorLog);
                    return;
                }

                // Сохраняем заказ в БД
                int orderId = _databaseService.CreateOrder(tableId, waiterId, shiftId, customerCount, status, orderItems);

                string successLog = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - Order data saved:\n" +
                                   $"Table: '{tableId}'\n" +
                                   $"Waiter: '{waiterName} (ID: {waiterId})'\n" +
                                   $"Status: '{status}'\n" +
                                   $"Shift ID: '{shiftId}'\n" +
                                   $"Menu Items Count: '{orderItems.Count}'\n" +
                                   $"DB Order ID: {orderId}\n";
                File.AppendAllText("A:/Инженерно-техническая поддержка сопровождения ИС/debug.log", successLog);

                if (orderId > 0)
                {
                    // Очищаем форму после успешного сохранения
                    ClearForm();
                    SaveButtonClicked?.Invoke(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
                string exceptionLog = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - ERROR saving order: {ex.Message}\n";
                File.AppendAllText("A:/Инженерно-техническая поддержка сопровождения ИС/debug.log", exceptionLog);
            }
        }

        // Метод для получения значения из ComboBox
        private string GetSelectedComboBoxValue(global::CafeApp.Controls.Components.ComboBox.ComboBox comboBox)
        {
            if (comboBox == null) return string.Empty;
            
            // Получаем внутренний ComboBox
            var innerComboBox = comboBox.FindControl<ComboBox>("MainComboBox");
            return innerComboBox?.SelectedItem?.ToString() ?? string.Empty;
        }

        // Метод для сбора данных о блюдах из UI
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
                    // Находим ComboBox с блюдами и Input с количеством в StackPanel
                    var stackPanel = container.FindControl<StackPanel>("MenuItemStackPanel");
                    if (stackPanel != null)
                    {
                        var menuComboBox = stackPanel.Children
                            .OfType<global::CafeApp.Controls.Components.ComboBox.ComboBox>()
                            .FirstOrDefault();
                        
                        var quantityInput = stackPanel.Children
                            .OfType<global::CafeApp.Controls.Components.Input.Input>()
                            .FirstOrDefault();

                        if (menuComboBox != null && quantityInput != null)
                        {
                            string menuItemName = GetSelectedComboBoxValue(menuComboBox);
                            string quantityText = quantityInput.Text;

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
                                }
                            }
                        }
                    }
                }
            }

            return orderItems;
        }

        private void ClearForm()
        {
            // Очищаем коллекцию блюд
            OrderMenuItems.Clear();
            OrderMenuItems.Add(new OrderMenuItem());

            // Очищаем основные поля
            var tableInput = this.FindControl<global::CafeApp.Controls.Components.Input.Input>("TableInput");
            if (tableInput != null)
                tableInput.Text = "";

            var statusComboBox = this.FindControl<global::CafeApp.Controls.Components.ComboBox.ComboBox>("StatusComboBox");
            var waiterComboBox = this.FindControl<global::CafeApp.Controls.Components.ComboBox.ComboBox>("WaiterComboBox");
            
            ClearComboBoxSelection(statusComboBox);
            ClearComboBoxSelection(waiterComboBox);
        }

        private void ClearComboBoxSelection(global::CafeApp.Controls.Components.ComboBox.ComboBox comboBox)
        {
            if (comboBox != null)
            {
                var innerComboBox = comboBox.FindControl<ComboBox>("MainComboBox");
                innerComboBox?.ClearValue(ComboBox.SelectedItemProperty);
            }
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            
            var addButton = this.FindControl<TextBlock>("AddMenuItemButton");
            if (addButton != null)
            {
                addButton.PointerPressed += OnAddMenuItemClicked;
            }
            
            var removeButton = this.FindControl<TextBlock>("RemoveMenuItemButton");
            if (removeButton != null)
            {
                removeButton.PointerPressed += OnRemoveMenuItemClicked;
            }
            
            // Подписываемся на кнопку сохранения
            var saveButton = this.FindControl<global::CafeApp.Controls.Components.Button.Button>("SaveButton");
            if (saveButton != null)
            {
                saveButton.PointerPressed += SaveButton_Click;
            }
        }
        
        public void RefreshData()
        {	
            LoadWaiters();
            LoadMenuData();
        }
    }

    public class OrderMenuItem
    {
        public string SelectedMenuItem { get; set; } = "";
        public int Quantity { get; set; } = 1;
    }
}