using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace CafeApp.Controls.Components.DragAndDrop
{
    public partial class DragAndDrop : UserControl
    {
        public DragAndDrop()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}