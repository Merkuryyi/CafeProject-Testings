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
    public partial class Report : UserControl
    {
        private readonly DatabaseService _databaseService;
        private readonly ExcelService _excelService = new ExcelService();
       
        public static readonly StyledProperty<string> TitleProperty =
            AvaloniaProperty.Register<Order, string>(nameof(Title), "Заказ");

        public static readonly StyledProperty<string> RoleProperty =
            AvaloniaProperty.Register<Order, string>(nameof(Role), "администратор");
        
        public ObservableCollection<string> FormatSelection { get; set; }
        public ObservableCollection<string> ShiftSelection { get; set; }
     
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
            }
        }
        

        public event EventHandler? SaveButtonClicked;

        public Report()
        {
            InitializeComponent();

            
            _databaseService = new DatabaseService();
            var dataRepository = new DataRepository(_databaseService);
         
            this.DataContext = this;

        }

        public void resetComponents()
        {
            var titleText = this.FindControl<TextBlock>("TitleText");
            
            
            
            if (Role == "официант")
            {
                
            }
            else
            {
                
            }
            
        }


    }
}