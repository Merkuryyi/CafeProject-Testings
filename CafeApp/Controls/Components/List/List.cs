using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Collections;

namespace CafeApp.Controls.Components.List
{
    public partial class List : UserControl
    {
        public static readonly StyledProperty<string> TitleProperty =
            AvaloniaProperty.Register<List, string>(nameof(Title), "Default Title");
        
        public static readonly StyledProperty<double> ListHeightProperty =
            AvaloniaProperty.Register<List, double>(nameof(ListHeight), 300);

        public static readonly StyledProperty<IEnumerable> ItemsProperty =
            AvaloniaProperty.Register<List, IEnumerable>(nameof(Items));

        public string Title
        {
            get => GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }
        
        public double ListHeight
        {
            get => GetValue(ListHeightProperty);
            set => SetValue(ListHeightProperty, value);
        }

        public IEnumerable Items
        {
            get => GetValue(ItemsProperty);
            set => SetValue(ItemsProperty, value);
        }

        public List()
        {
            InitializeComponent();
        }
    }
}