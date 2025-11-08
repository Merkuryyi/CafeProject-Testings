using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Collections;
using System.Collections.Generic; 
using System.Collections.ObjectModel; // Добавьте эту директиву


namespace CafeApp.Controls
{
    public partial class FormEmployee : UserControl
    {
        public FormEmployee()
        {
            InitializeComponent();
			Roles = new List<string> 
        	{
            	"Администратор", 
            	"Официант", 
            	"Повар",
        	};
			Status = new List<string> 
        	{ 
            	"Работает", 
            	"Уволен",      
       		};
        
       		DataContext = this;
        }

		public List<string> Roles { get; set; }
		public List<string> Status { get; set; }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
public ObservableCollection<string> Employees { get; } = new ObservableCollection<string>();
    
  /*  public void LoadEmployees(DatabaseService dbService)
    {
        var employees = dbService.GetEmployeesList();
        Employees.Clear();
        foreach (var employee in employees)
        {
            Employees.Add(employee);
        }
    }*/
    }
}