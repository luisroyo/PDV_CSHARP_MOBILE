using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Pos.Desktop.Wpf.Models;

namespace Pos.Desktop.Wpf.ViewModels
{
    public class CustomerManagementViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<CustomerDto> _customers;
        private CustomerDto _selectedCustomer;
        private CustomerFormDto _customerForm;
        private string _searchText = string.Empty;
        private string _errorMessage = string.Empty;
        private string _statusMessage = "Pronto";

        public CustomerManagementViewModel()
        {
            _customers = new ObservableCollection<CustomerDto>();
            _customerForm = new CustomerFormDto();
            
            // Commands
            NewCustomerCommand = new RelayCommand(NewCustomer);
            EditCustomerCommand = new RelayCommand<CustomerDto>(EditCustomer);
            DeleteCustomerCommand = new RelayCommand<CustomerDto>(DeleteCustomer);
            SaveCustomerCommand = new RelayCommand(SaveCustomer, () => IsFormValid());
            CancelCommand = new RelayCommand(Cancel);
            SearchCommand = new RelayCommand(Search);
            RefreshCommand = new RelayCommand(Refresh);

            LoadCustomers();
        }

        public ObservableCollection<CustomerDto> Customers
        {
            get => _customers;
            set
            {
                _customers = value;
                OnPropertyChanged();
            }
        }

        public CustomerDto SelectedCustomer
        {
            get => _selectedCustomer;
            set
            {
                _selectedCustomer = value;
                OnPropertyChanged();
            }
        }

        public CustomerFormDto CustomerForm
        {
            get => _customerForm;
            set
            {
                _customerForm = value;
                OnPropertyChanged();
                ((RelayCommand)SaveCustomerCommand).RaiseCanExecuteChanged();
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

        public ICommand NewCustomerCommand { get; }
        public ICommand EditCustomerCommand { get; }
        public ICommand DeleteCustomerCommand { get; }
        public ICommand SaveCustomerCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand RefreshCommand { get; }

        private void LoadCustomers()
        {
            // Simular carregamento de clientes
            Customers.Clear();
            
            var sampleCustomers = new[]
            {
                new CustomerDto { Id = 1, Name = "João Silva", Document = "123.456.789-00", Phone = "(11) 99999-9999", Email = "joao@email.com", Active = true },
                new CustomerDto { Id = 2, Name = "Maria Santos", Document = "987.654.321-00", Phone = "(11) 88888-8888", Email = "maria@email.com", Active = true },
                new CustomerDto { Id = 3, Name = "Empresa ABC Ltda", Document = "12.345.678/0001-90", Phone = "(11) 77777-7777", Email = "contato@empresa.com", Active = true },
                new CustomerDto { Id = 4, Name = "Pedro Oliveira", Document = "111.222.333-44", Phone = "(11) 66666-6666", Email = "pedro@email.com", Active = false }
            };

            foreach (var customer in sampleCustomers)
            {
                Customers.Add(customer);
            }

            StatusMessage = $"Carregados {Customers.Count} clientes";
        }

        private void NewCustomer()
        {
            CustomerForm = new CustomerFormDto
            {
                Active = true
            };
            ErrorMessage = string.Empty;
            StatusMessage = "Novo cliente - preencha os dados";
        }

        private void EditCustomer(CustomerDto customer)
        {
            if (customer == null) return;

            CustomerForm = new CustomerFormDto
            {
                Id = customer.Id,
                Name = customer.Name,
                Document = customer.Document,
                Phone = customer.Phone,
                Email = customer.Email,
                Address = customer.Address,
                BirthDate = customer.BirthDate,
                Active = customer.Active
            };
            ErrorMessage = string.Empty;
            StatusMessage = $"Editando cliente: {customer.Name}";
        }

        private void DeleteCustomer(CustomerDto customer)
        {
            if (customer == null) return;

            var result = MessageBox.Show(
                $"Deseja realmente excluir o cliente '{customer.Name}'?",
                "Confirmar Exclusão",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                Customers.Remove(customer);
                StatusMessage = $"Cliente '{customer.Name}' excluído";
            }
        }

        private void SaveCustomer()
        {
            try
            {
                ErrorMessage = string.Empty;

                // Validação
                if (string.IsNullOrWhiteSpace(CustomerForm.Name))
                {
                    ErrorMessage = "Nome é obrigatório";
                    return;
                }

                if (string.IsNullOrWhiteSpace(CustomerForm.Document))
                {
                    ErrorMessage = "CPF/CNPJ é obrigatório";
                    return;
                }

                // Verificar se documento já existe
                var existingCustomer = Customers.FirstOrDefault(c => c.Document == CustomerForm.Document && c.Id != CustomerForm.Id);
                if (existingCustomer != null)
                {
                    ErrorMessage = "CPF/CNPJ já existe para outro cliente";
                    return;
                }

                if (CustomerForm.Id == 0)
                {
                    // Novo cliente
                    var newCustomer = new CustomerDto
                    {
                        Id = Customers.Count + 1,
                        Name = CustomerForm.Name,
                        Document = CustomerForm.Document,
                        Phone = CustomerForm.Phone,
                        Email = CustomerForm.Email,
                        Address = CustomerForm.Address,
                        BirthDate = CustomerForm.BirthDate,
                        Active = CustomerForm.Active
                    };
                    Customers.Add(newCustomer);
                    StatusMessage = $"Cliente '{newCustomer.Name}' criado com sucesso";
                }
                else
                {
                    // Editar cliente existente
                    var existing = Customers.FirstOrDefault(c => c.Id == CustomerForm.Id);
                    if (existing != null)
                    {
                        existing.Name = CustomerForm.Name;
                        existing.Document = CustomerForm.Document;
                        existing.Phone = CustomerForm.Phone;
                        existing.Email = CustomerForm.Email;
                        existing.Address = CustomerForm.Address;
                        existing.BirthDate = CustomerForm.BirthDate;
                        existing.Active = CustomerForm.Active;
                        StatusMessage = $"Cliente '{existing.Name}' atualizado com sucesso";
                    }
                }

                // Limpar formulário
                CustomerForm = new CustomerFormDto();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Erro ao salvar cliente: {ex.Message}";
            }
        }

        private void Cancel()
        {
            CustomerForm = new CustomerFormDto();
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
            LoadCustomers();
        }

        private bool IsFormValid()
        {
            return !string.IsNullOrWhiteSpace(CustomerForm.Name) &&
                   !string.IsNullOrWhiteSpace(CustomerForm.Document);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class CustomerFormDto : INotifyPropertyChanged
    {
        private int _id;
        private string _name = string.Empty;
        private string _document = string.Empty;
        private string _phone = string.Empty;
        private string _email = string.Empty;
        private string _address = string.Empty;
        private DateTime? _birthDate;
        private bool _active = true;

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

        public string Document
        {
            get => _document;
            set
            {
                _document = value;
                OnPropertyChanged();
            }
        }

        public string Phone
        {
            get => _phone;
            set
            {
                _phone = value;
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

        public string Address
        {
            get => _address;
            set
            {
                _address = value;
                OnPropertyChanged();
            }
        }

        public DateTime? BirthDate
        {
            get => _birthDate;
            set
            {
                _birthDate = value;
                OnPropertyChanged();
            }
        }

        public bool Active
        {
            get => _active;
            set
            {
                _active = value;
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
