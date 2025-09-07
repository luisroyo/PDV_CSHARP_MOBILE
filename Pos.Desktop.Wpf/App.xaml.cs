using System.Configuration;
using System.Data;
using System.Windows;
using Pos.Desktop.Wpf.Views;

namespace Pos.Desktop.Wpf;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        // Mostrar tela de login primeiro
        var loginWindow = new LoginWindow();
        loginWindow.Show();
        
        MainWindow = loginWindow;
    }
}

