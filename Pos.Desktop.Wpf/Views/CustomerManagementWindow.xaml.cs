using System.Windows;
using Pos.Desktop.Wpf.ViewModels;

namespace Pos.Desktop.Wpf.Views
{
    public partial class CustomerManagementWindow : Window
    {
        public CustomerManagementWindow()
        {
            InitializeComponent();
            DataContext = new CustomerManagementViewModel();
            
            // Focar no campo de busca
            Loaded += (s, e) => SearchBox.Focus();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
