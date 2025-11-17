using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CafeApp.Data;
using CafeApp.Models;
using System.Linq;
using CafeApp.Database;
using System;

namespace CafeApp.Controls
{
    public partial class Order : UserControl
    {
        private readonly DataRepository _dataRepository;
        
        public ObservableCollection<string> StatusOrder { get; set; }
        public ObservableCollection<string> CategoryMenu { get; set; }
        public ObservableCollection<Models.MenuItem> AllMenuItems { get; set; }
        public ObservableCollection<string> FilteredMenuItems { get; set; }
        public ObservableCollection<string> ListWaiter { get; set; }

        public Order()
        {
            InitializeComponent();
            
            _dataRepository = new DataRepository(new DatabaseService());
            LoadWaiters();
            LoadMenuData();
            LoadOtherData();
            
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
            // Загружаем все категории из базы данных
            CategoryMenu = new ObservableCollection<string>(
                _dataRepository.MenuItems
                    .Select(m => m.Category)
                    .Distinct()
                    .OrderBy(c => c)
            );

            // Загружаем все блюда
            AllMenuItems = new ObservableCollection<Models.MenuItem>(_dataRepository.MenuItems);
            FilteredMenuItems = new ObservableCollection<string>();

            // Если категорий нет, добавляем тестовые данные
            if (!CategoryMenu.Any())
            {
                CategoryMenu = new ObservableCollection<string> 
                {
                    "Супы",
                    "Горячие блюда",
                    "Салаты",
                    "Основные блюда",
                    "Первые блюда"
                };
            }
        }

        // Метод для фильтрации блюд по категории
        public void FilterMenuItemsByCategory(string category)
        {
            FilteredMenuItems.Clear();
            
            if (string.IsNullOrEmpty(category))
                return;

            var itemsInCategory = AllMenuItems
                .Where(m => m.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
                .OrderBy(m => m.Name)
                .Select(m => m.Name);

            foreach (var itemName in itemsInCategory)
            {
                FilteredMenuItems.Add(itemName);
            }

            // Если в категории нет блюд, добавляем сообщение
            if (!FilteredMenuItems.Any())
            {
                FilteredMenuItems.Add("Блюда не найдены");
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

        // Обработчик изменения выбранной категории - вызывается после полной загрузки
        protected override void OnInitialized()
        {
            base.OnInitialized();
            
            // Подписываемся на событие выбора категории
            var categoryComboBox = this.FindControl<global::CafeApp.Controls.Components.ComboBox.ComboBox>("CategoryComboBox");
            if (categoryComboBox != null)
            {
                // Получаем внутренний ComboBox и подписываемся на его событие
                var innerComboBox = categoryComboBox.FindControl<Avalonia.Controls.ComboBox>("MainComboBox");
                if (innerComboBox != null)
                {
                    innerComboBox.SelectionChanged += OnCategorySelectionChanged;
                }
            }
        }

        private void OnCategorySelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as Avalonia.Controls.ComboBox;
            if (comboBox?.SelectedItem is string selectedCategory)
            {
                FilterMenuItemsByCategory(selectedCategory);
            }
        }
        public void RefreshData()
        {
            LoadWaiters();
            LoadMenuData();
        }
    }
}