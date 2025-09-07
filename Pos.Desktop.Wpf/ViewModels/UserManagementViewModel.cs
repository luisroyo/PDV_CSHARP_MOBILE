using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Pos.Desktop.Wpf.Models;

namespace Pos.Desktop.Wpf.ViewModels
{
    public class UserManagementViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<UserDto> _users;
        private UserDto _selectedUser;
        private UserFormDto _userForm;
        private string _searchText = string.Empty;
        private string _errorMessage = string.Empty;
        private string _statusMessage = "Pronto";

        public UserManagementViewModel()
        {
            _users = new ObservableCollection<UserDto>();
            _userForm = new UserFormDto();
            
            // Commands
            NewUserCommand = new RelayCommand(NewUser);
            EditUserCommand = new RelayCommand<UserDto>(EditUser);
            DeleteUserCommand = new RelayCommand<UserDto>(DeleteUser);
            SaveUserCommand = new RelayCommand(SaveUser, () => IsFormValid());
            CancelCommand = new RelayCommand(Cancel);
            SearchCommand = new RelayCommand(Search);
            RefreshCommand = new RelayCommand(Refresh);

            // Roles
            Roles = new ObservableCollection<string>
            {
                "Administrador",
                "Gerente",
                "Operador",
                "Vendedor",
                "Caixa"
            };

            LoadUsers();
        }

        public ObservableCollection<UserDto> Users
        {
            get => _users;
            set
            {
                _users = value;
                OnPropertyChanged();
            }
        }

        public UserDto SelectedUser
        {
            get => _selectedUser;
            set
            {
                _selectedUser = value;
                OnPropertyChanged();
            }
        }

        public UserFormDto UserForm
        {
            get => _userForm;
            set
            {
                _userForm = value;
                OnPropertyChanged();
                ((RelayCommand)SaveUserCommand).RaiseCanExecuteChanged();
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
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

        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<string> Roles { get; }

        public ICommand NewUserCommand { get; }
        public ICommand EditUserCommand { get; }
        public ICommand DeleteUserCommand { get; }
        public ICommand SaveUserCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand RefreshCommand { get; }

        private void LoadUsers()
        {
            // Simular carregamento de usuários
            Users.Clear();
            
            var sampleUsers = new[]
            {
                new UserDto { Id = 1, Name = "Administrador Sistema", Username = "admin", Email = "admin@sistema.com", Role = "Administrador", IsActive = true },
                new UserDto { Id = 2, Name = "João Silva", Username = "joao", Email = "joao@empresa.com", Role = "Gerente", IsActive = true },
                new UserDto { Id = 3, Name = "Maria Santos", Username = "maria", Email = "maria@empresa.com", Role = "Operador", IsActive = true },
                new UserDto { Id = 4, Name = "Pedro Oliveira", Username = "pedro", Email = "pedro@empresa.com", Role = "Vendedor", IsActive = false }
            };

            foreach (var user in sampleUsers)
            {
                Users.Add(user);
            }

            StatusMessage = $"Carregados {Users.Count} usuários";
        }

        private void NewUser()
        {
            UserForm = new UserFormDto
            {
                IsActive = true,
                Role = "Operador"
            };
            ErrorMessage = string.Empty;
            StatusMessage = "Novo usuário - preencha os dados";
        }

        private void EditUser(UserDto user)
        {
            if (user == null) return;

            UserForm = new UserFormDto
            {
                Id = user.Id,
                Name = user.Name,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                IsActive = user.IsActive
            };
            ErrorMessage = string.Empty;
            StatusMessage = $"Editando usuário: {user.Name}";
        }

        private void DeleteUser(UserDto user)
        {
            if (user == null) return;

            var result = MessageBox.Show(
                $"Deseja realmente excluir o usuário '{user.Name}'?",
                "Confirmar Exclusão",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                Users.Remove(user);
                StatusMessage = $"Usuário '{user.Name}' excluído";
            }
        }

        private void SaveUser()
        {
            try
            {
                ErrorMessage = string.Empty;

                // Validação
                if (string.IsNullOrWhiteSpace(UserForm.Name))
                {
                    ErrorMessage = "Nome é obrigatório";
                    return;
                }

                if (string.IsNullOrWhiteSpace(UserForm.Username))
                {
                    ErrorMessage = "Nome de usuário é obrigatório";
                    return;
                }

                if (string.IsNullOrWhiteSpace(UserForm.Role))
                {
                    ErrorMessage = "Perfil é obrigatório";
                    return;
                }

                // Verificar se username já existe
                var existingUser = Users.FirstOrDefault(u => u.Username == UserForm.Username && u.Id != UserForm.Id);
                if (existingUser != null)
                {
                    ErrorMessage = "Nome de usuário já existe para outro usuário";
                    return;
                }

                // Validar senha se for novo usuário ou se foi informada
                if (UserForm.Id == 0 || !string.IsNullOrWhiteSpace(UserForm.Password))
                {
                    if (string.IsNullOrWhiteSpace(UserForm.Password))
                    {
                        ErrorMessage = "Senha é obrigatória para novos usuários";
                        return;
                    }

                    if (UserForm.Password != UserForm.ConfirmPassword)
                    {
                        ErrorMessage = "Senha e confirmação não coincidem";
                        return;
                    }

                    if (UserForm.Password.Length < 6)
                    {
                        ErrorMessage = "Senha deve ter pelo menos 6 caracteres";
                        return;
                    }
                }

                if (UserForm.Id == 0)
                {
                    // Novo usuário
                    var newUser = new UserDto
                    {
                        Id = Users.Count + 1,
                        Name = UserForm.Name,
                        Username = UserForm.Username,
                        Email = UserForm.Email,
                        Role = UserForm.Role,
                        IsActive = UserForm.IsActive
                    };
                    Users.Add(newUser);
                    StatusMessage = $"Usuário '{newUser.Name}' criado com sucesso";
                }
                else
                {
                    // Editar usuário existente
                    var existing = Users.FirstOrDefault(u => u.Id == UserForm.Id);
                    if (existing != null)
                    {
                        existing.Name = UserForm.Name;
                        existing.Username = UserForm.Username;
                        existing.Email = UserForm.Email;
                        existing.Role = UserForm.Role;
                        existing.IsActive = UserForm.IsActive;
                        StatusMessage = $"Usuário '{existing.Name}' atualizado com sucesso";
                    }
                }

                // Limpar formulário
                UserForm = new UserFormDto();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Erro ao salvar usuário: {ex.Message}";
            }
        }

        private void Cancel()
        {
            UserForm = new UserFormDto();
            ErrorMessage = string.Empty;
            StatusMessage = "Operação cancelada";
        }

        private void Search()
        {
            // Implementar busca
            StatusMessage = "Busca realizada";
        }

        private void Refresh()
        {
            LoadUsers();
        }

        private bool IsFormValid()
        {
            return !string.IsNullOrWhiteSpace(UserForm.Name) &&
                   !string.IsNullOrWhiteSpace(UserForm.Username) &&
                   !string.IsNullOrWhiteSpace(UserForm.Role);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class UserFormDto : INotifyPropertyChanged
    {
        private int _id;
        private string _name = string.Empty;
        private string _username = string.Empty;
        private string _email = string.Empty;
        private string _password = string.Empty;
        private string _confirmPassword = string.Empty;
        private string _role = string.Empty;
        private bool _isActive = true;

        public int Id
        {
            get => _id;
            set
            {
                _id = value;
                OnPropertyChanged();
            }
        }

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged();
            }
        }

        public string Email
        {
            get => _email;
            set
            {
                _email = value;
                OnPropertyChanged();
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged();
            }
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                _confirmPassword = value;
                OnPropertyChanged();
            }
        }

        public string Role
        {
            get => _role;
            set
            {
                _role = value;
                OnPropertyChanged();
            }
        }

        public bool IsActive
        {
            get => _isActive;
            set
            {
                _isActive = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
