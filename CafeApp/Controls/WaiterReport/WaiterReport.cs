using Avalonia;
using Avalonia.Controls;
using System.Collections.ObjectModel;
using CafeApp.Database;
using CafeApp.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace CafeApp.Controls
{
    public partial class WaiterReport : UserControl
    {
        private readonly DatabaseService _databaseService;
        private readonly ObservableCollection<ListItem> _waiterOrders = new();
        
        public static readonly StyledProperty<string> WaiterNameProperty =
            AvaloniaProperty.Register<WaiterReport, string>(nameof(WaiterName), "");
            
        public static readonly StyledProperty<int> WaiterIdProperty =
            AvaloniaProperty.Register<WaiterReport, int>(nameof(WaiterId), -1);
            
        public static readonly StyledProperty<int> ShiftIdProperty =
            AvaloniaProperty.Register<WaiterReport, int>(nameof(ShiftId), -1);

        public event EventHandler<int>? OrderClicked;

        public string WaiterName
        {
            get => GetValue(WaiterNameProperty);
            set => SetValue(WaiterNameProperty, value);
        }
        
        public int WaiterId
        {
            get => GetValue(WaiterIdProperty);
            set => SetValue(WaiterIdProperty, value);
        }
        
        public int ShiftId
        {
            get => GetValue(ShiftIdProperty);
            set => SetValue(ShiftIdProperty, value);
        }

        public WaiterReport()
        {
            InitializeComponent();
            _databaseService = new DatabaseService();
            this.DataContext = this;
        }

        public void LoadWaiterReport(int waiterId, string waiterName, int shiftId = -1)
        {
            try
            {
                WaiterId = waiterId;
                WaiterName = waiterName;
                ShiftId = shiftId;

                var waiterNameText = this.FindControl<TextBlock>("WaiterNameText");
                if (waiterNameText != null)
                    waiterNameText.Text = $"Официант: {waiterName}";
                
                LoadWaiterOrders();
            }
            catch (Exception ex)
            {
                File.AppendAllText(@"A:\debug.log", 
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - ERROR in LoadWaiterReport: {ex.Message}\n{ex.StackTrace}\n");
            }
        }

        private void LoadWaiterOrders()
        {
            try
            {
                _waiterOrders.Clear();
                var orders = _databaseService.GetWaiterOrders(WaiterId, ShiftId);
                
                foreach (var order in orders)
                { _waiterOrders.Add(order); }
                var ordersList = this.FindControl<global::CafeApp.Controls.Components.List.List>("WaiterOrdersList");
                if (ordersList != null)
                {
                    ordersList.Items = _waiterOrders;
                    ordersList.Title = $"Принятые заказы ({_waiterOrders.Count})";
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText(@"A:\debug.log", 
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - ERROR in LoadWaiterOrders: {ex.Message}\n{ex.StackTrace}\n");
            }
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            
            var ordersList = this.FindControl<Components.List.List>("WaiterOrdersList");
            if (ordersList != null)
            { ordersList.ItemClicked += OnOrderItemClicked; }
        }

        private void OnOrderItemClicked(object sender, ListItem clickedItem)
        {
            try
            { OrderClicked?.Invoke(this, clickedItem.Id); }
            catch (Exception ex)
            {
                File.AppendAllText(@"A:\debug.log", 
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - ERROR in OnOrderItemClicked: {ex.Message}\n{ex.StackTrace}\n");
            }
        }
    }
}