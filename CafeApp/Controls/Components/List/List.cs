using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using System;
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

        // Событие для клика на элементе списка (теперь передает ListItem)
        public event EventHandler<ListItem> ItemClicked;
        
        // Событие для клика на кнопке "+"
        public event EventHandler AddButtonClicked;

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

        protected override void OnInitialized()
        {
            base.OnInitialized();
            
            var addButton = this.FindControl<TextBlock>("AddOrder");
            if (addButton != null)
            {
                addButton.PointerPressed += OnAddButtonPressed;
            }
        }

        private void OnItemPointerPressed(object sender, PointerPressedEventArgs e)
        {
            var textBlock = sender as TextBlock;
            if (textBlock?.DataContext is ListItem listItem)
            {
                ItemClicked?.Invoke(this, listItem);
            }
        }

        private void OnAddButtonPressed(object sender, PointerPressedEventArgs e)
        {
            AddButtonClicked?.Invoke(this, EventArgs.Empty);
        }
    }
}