using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Pos.Desktop.Wpf.ViewModels;

namespace Pos.Desktop.Wpf.Views;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }

    private void ProductsButton_Click(object sender, RoutedEventArgs e)
    {
        var productWindow = new ProductManagementWindow();
        productWindow.Show();
    }

    private void CustomersButton_Click(object sender, RoutedEventArgs e)
    {
        var customerWindow = new CustomerManagementWindow();
        customerWindow.Show();
    }

    private void ReportsButton_Click(object sender, RoutedEventArgs e)
    {
        var reportsWindow = new ReportsWindow();
        reportsWindow.Show();
    }

    private void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        var settingsWindow = new SettingsWindow();
        settingsWindow.Show();
    }

    private void CloseCashButton_Click(object sender, RoutedEventArgs e)
    {
        var closeCashWindow = new CloseCashWindow();
        closeCashWindow.Show();
    }

    private void UsersButton_Click(object sender, RoutedEventArgs e)
    {
        var userWindow = new UserManagementWindow();
        userWindow.Show();
    }
}