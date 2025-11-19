using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;
using System.Collections.ObjectModel;
using System;
using System.Linq;

namespace CafeApp.Controls
{
    // Модель для элемента заказа
    public class OrderItemDisplay
    {
        public string Name { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Total => Quantity * Price;
    }

    public partial class EditOrder : UserControl
    {
        public ObservableCollection<string> StatusOrder { get; set; }
        public ObservableCollection<OrderItemDisplay> OrderItems { get; set; }
        
        // Свойства для привязки
        public string WaiterName { get; set; } = "Иван Иванов";
        public string TableNumber { get; set; } = "5";
        public decimal TotalAmount => OrderItems?.Sum(item => item.Total) ?? 0;

        public EditOrder()
        {
            InitializeComponent();
            
            InitializeData();
        }

        private void InitializeData()
        {
            StatusOrder = new ObservableCollection<string> 
            {
                "принят", "готовится", "готов", "оплачен"
            };
            
            // ЗАПОЛНЕНИЕ ТЕСТОВЫМИ ДАННЫМИ
            OrderItems = new ObservableCollection<OrderItemDisplay>
            {
                new OrderItemDisplay { Name = "Кофе латте", Quantity = 2, Price = 180 },
                new OrderItemDisplay { Name = "Чай зеленый", Quantity = 1, Price = 120 },
                new OrderItemDisplay { Name = "Пирог яблочный", Quantity = 3, Price = 150 },
                new OrderItemDisplay { Name = "Сэндвич с ветчиной", Quantity = 1, Price = 220 },
                new OrderItemDisplay { Name = "Салат Цезарь", Quantity = 2, Price = 300 }
            };
            
            this.DataContext = this;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("=== СОХРАНЕНИЕ ЗАКАЗА ===");
            Console.WriteLine($"Официант: {WaiterName}");
            Console.WriteLine($"Столик: {TableNumber}");
            Console.WriteLine($"Общая сумма: {TotalAmount} руб.");
            Console.WriteLine("Позиции заказа:");
            
            foreach (var item in OrderItems)
            {
                Console.WriteLine($"- {item.Name}: {item.Quantity} x {item.Price} руб. = {item.Total} руб.");
            }
            
            Console.WriteLine("=== СОХРАНЕНО ===");
            
            // TODO: Добавить реальную логику сохранения в базу данных
        }

        // Метод для загрузки реальных данных заказа (вызовите его из MainWindow)
        public void LoadOrderData(int orderId)
        {
            // TODO: Загрузите данные заказа из базы данных по orderId
            // Пример:
            /*
            var order = _databaseService.GetOrderById(orderId);
            if (order != null)
            {
                WaiterName = order.WaiterName;
                TableNumber = order.TableNumber.ToString();
                
                OrderItems.Clear();
                foreach (var item in order.OrderItems)
                {
                    OrderItems.Add(new OrderItemDisplay 
                    { 
                        Name = item.MenuItemName, 
                        Quantity = item.Quantity, 
                        Price = item.Price 
                    });
                }
                
                // Обновляем привязки
                this.DataContext = null;
                this.DataContext = this;
            }
            */
        }

        // Метод для обновления данных (если нужно динамически менять)
        public void UpdateOrderItems(ObservableCollection<OrderItemDisplay> newItems)
        {
            OrderItems.Clear();
            foreach (var item in newItems)
            {
                OrderItems.Add(item);
            }
            
            // Уведомляем об изменении общей суммы
        }
    }
}