using System.Windows;
using Pos.Desktop.Wpf.ViewModels;

namespace Pos.Desktop.Wpf.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            DataContext = new LoginViewModel();
            
            // Focar no campo de usuário
            Loaded += (s, e) => UsernameBox.Focus();
            
            // Enter no campo de senha executa login
            PasswordBox.KeyDown += (s, e) =>
            {
                if (e.Key == System.Windows.Input.Key.Enter)
                {
                    if (DataContext is LoginViewModel viewModel)
                    {
                        viewModel.LoginCommand.Execute(null);
                    }
                }
            };
        }
    }
}
