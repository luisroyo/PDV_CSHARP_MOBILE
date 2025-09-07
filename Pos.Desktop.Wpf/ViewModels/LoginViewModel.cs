using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows;
using Pos.Desktop.Wpf.Services;

namespace Pos.Desktop.Wpf.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private readonly AuthService _authService;
        private string _username = string.Empty;
        private string _password = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _isLoading = false;

        public LoginViewModel()
        {
            _authService = new AuthService();
            LoginCommand = new RelayCommand(async () => await LoginAsync(), () => !IsLoading && !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password));
        }

        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged();
                ((RelayCommand)LoginCommand).RaiseCanExecuteChanged();
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged();
                ((RelayCommand)LoginCommand).RaiseCanExecuteChanged();
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged();
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
                ((RelayCommand)LoginCommand).RaiseCanExecuteChanged();
            }
        }

        public ICommand LoginCommand { get; }

        private async Task LoginAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                // Simular autenticação (em produção, chamaria a API)
                await Task.Delay(1000);

                // Validação simples
                if (Username == "admin" && Password == "123456")
                {
                    // Login bem-sucedido
                    var mainWindow = new MainWindow();
                    mainWindow.Show();
                    Application.Current.MainWindow = mainWindow;
                    CloseCurrentWindow();
                }
                else if (Username == "vendedor" && Password == "123456")
                {
                    // Login bem-sucedido
                    var mainWindow = new MainWindow();
                    mainWindow.Show();
                    Application.Current.MainWindow = mainWindow;
                    CloseCurrentWindow();
                }
                else
                {
                    ErrorMessage = "Usuário ou senha inválidos";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Erro ao fazer login: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void CloseCurrentWindow()
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window is LoginWindow)
                {
                    window.Close();
                    break;
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Func<Task> _execute;
        private readonly Func<bool> _canExecute;

        public RelayCommand(Func<Task> execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

        public async void Execute(object? parameter) => await _execute();

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
