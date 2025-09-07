using System.Windows;
using Pos.Desktop.Wpf.ViewModels;

namespace Pos.Desktop.Wpf.Views
{
    public partial class CloseCashWindow : Window
    {
        public CloseCashWindow()
        {
            InitializeComponent();
            DataContext = new CloseCashViewModel();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
