using System.Windows;
using Pos.Desktop.Wpf.ViewModels;

namespace Pos.Desktop.Wpf.Views
{
    public partial class ProductManagementWindow : Window
    {
        public ProductManagementWindow()
        {
            InitializeComponent();
            DataContext = new ProductManagementViewModel();
            
            // Focar no campo de busca
            Loaded += (s, e) => SearchBox.Focus();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
