using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Win32;
using System.Windows;

namespace Pos.Desktop.Wpf.ViewModels
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private string _companyName = "Minha Empresa";
        private string _companyDocument = "12.345.678/0001-90";
        private string _selectedBusinessProfile = "Farmácia";
        private string _selectedCurrency = "BRL";
        private string _selectedPrinter = "Impressora Padrão";
        private string _selectedPaperSize = "80mm";
        private bool _autoPrint = true;
        private string _databaseServer = "localhost";
        private string _databaseName = "pos_multivertical";
        private string _databaseUser = "postgres";
        private string _apiUrl = "https://localhost:5001";
        private int _apiTimeout = 30;
        private bool _useHttps = true;
        private string _selectedBarcodeReader = "USB HID";
        private string _selectedScale = "Nenhuma";
        private string _selectedCashDrawer = "USB";
        private int _sessionTimeout = 60;
        private bool _requirePasswordForCriticalOperations = true;
        private bool _auditLogEnabled = true;
        private string _backupDirectory = @"C:\Backups\PDV";
        private bool _autoBackupEnabled = true;
        private string _statusMessage = "Pronto";

        public SettingsViewModel()
        {
            // Initialize collections
            BusinessProfiles = new ObservableCollection<string>
            {
                "Farmácia", "Construção", "Supermercado", "Alimentação", "Geral"
            };

            Currencies = new ObservableCollection<string>
            {
                "BRL", "USD", "EUR"
            };

            Printers = new ObservableCollection<string>
            {
                "Impressora Padrão", "EPSON TM-T20", "Zebra ZD420", "Bematech MP-4200"
            };

            PaperSizes = new ObservableCollection<string>
            {
                "80mm", "58mm", "A4"
            };

            BarcodeReaders = new ObservableCollection<string>
            {
                "USB HID", "Serial", "Bluetooth", "Nenhum"
            };

            Scales = new ObservableCollection<string>
            {
                "Nenhuma", "Toledo 8142", "Filizola", "Urano"
            };

            CashDrawers = new ObservableCollection<string>
            {
                "USB", "Serial", "Nenhuma"
            };

            // Commands
            SaveCommand = new RelayCommand(Save);
            ResetCommand = new RelayCommand(Reset);
            TestConnectionCommand = new RelayCommand(TestConnection);
            BrowseBackupDirectoryCommand = new RelayCommand(BrowseBackupDirectory);
            ExecuteBackupCommand = new RelayCommand(ExecuteBackup);
        }

        public string CompanyName
        {
            get => _companyName;
            set
            {
                _companyName = value;
                OnPropertyChanged();
            }
        }

        public string CompanyDocument
        {
            get => _companyDocument;
            set
            {
                _companyDocument = value;
                OnPropertyChanged();
            }
        }

        public string SelectedBusinessProfile
        {
            get => _selectedBusinessProfile;
            set
            {
                _selectedBusinessProfile = value;
                OnPropertyChanged();
            }
        }

        public string SelectedCurrency
        {
            get => _selectedCurrency;
            set
            {
                _selectedCurrency = value;
                OnPropertyChanged();
            }
        }

        public string SelectedPrinter
        {
            get => _selectedPrinter;
            set
            {
                _selectedPrinter = value;
                OnPropertyChanged();
            }
        }

        public string SelectedPaperSize
        {
            get => _selectedPaperSize;
            set
            {
                _selectedPaperSize = value;
                OnPropertyChanged();
            }
        }

        public bool AutoPrint
        {
            get => _autoPrint;
            set
            {
                _autoPrint = value;
                OnPropertyChanged();
            }
        }

        public string DatabaseServer
        {
            get => _databaseServer;
            set
            {
                _databaseServer = value;
                OnPropertyChanged();
            }
        }

        public string DatabaseName
        {
            get => _databaseName;
            set
            {
                _databaseName = value;
                OnPropertyChanged();
            }
        }

        public string DatabaseUser
        {
            get => _databaseUser;
            set
            {
                _databaseUser = value;
                OnPropertyChanged();
            }
        }

        public string ApiUrl
        {
            get => _apiUrl;
            set
            {
                _apiUrl = value;
                OnPropertyChanged();
            }
        }

        public int ApiTimeout
        {
            get => _apiTimeout;
            set
            {
                _apiTimeout = value;
                OnPropertyChanged();
            }
        }

        public bool UseHttps
        {
            get => _useHttps;
            set
            {
                _useHttps = value;
                OnPropertyChanged();
            }
        }

        public string SelectedBarcodeReader
        {
            get => _selectedBarcodeReader;
            set
            {
                _selectedBarcodeReader = value;
                OnPropertyChanged();
            }
        }

        public string SelectedScale
        {
            get => _selectedScale;
            set
            {
                _selectedScale = value;
                OnPropertyChanged();
            }
        }

        public string SelectedCashDrawer
        {
            get => _selectedCashDrawer;
            set
            {
                _selectedCashDrawer = value;
                OnPropertyChanged();
            }
        }

        public int SessionTimeout
        {
            get => _sessionTimeout;
            set
            {
                _sessionTimeout = value;
                OnPropertyChanged();
            }
        }

        public bool RequirePasswordForCriticalOperations
        {
            get => _requirePasswordForCriticalOperations;
            set
            {
                _requirePasswordForCriticalOperations = value;
                OnPropertyChanged();
            }
        }

        public bool AuditLogEnabled
        {
            get => _auditLogEnabled;
            set
            {
                _auditLogEnabled = value;
                OnPropertyChanged();
            }
        }

        public string BackupDirectory
        {
            get => _backupDirectory;
            set
            {
                _backupDirectory = value;
                OnPropertyChanged();
            }
        }

        public bool AutoBackupEnabled
        {
            get => _autoBackupEnabled;
            set
            {
                _autoBackupEnabled = value;
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

        public ObservableCollection<string> BusinessProfiles { get; }
        public ObservableCollection<string> Currencies { get; }
        public ObservableCollection<string> Printers { get; }
        public ObservableCollection<string> PaperSizes { get; }
        public ObservableCollection<string> BarcodeReaders { get; }
        public ObservableCollection<string> Scales { get; }
        public ObservableCollection<string> CashDrawers { get; }

        public ICommand SaveCommand { get; }
        public ICommand ResetCommand { get; }
        public ICommand TestConnectionCommand { get; }
        public ICommand BrowseBackupDirectoryCommand { get; }
        public ICommand ExecuteBackupCommand { get; }

        private void Save()
        {
            try
            {
                // Simular salvamento das configurações
                StatusMessage = "Configurações salvas com sucesso!";
                
                // Em produção, salvaria em arquivo de configuração ou banco
                MessageBox.Show("Configurações salvas com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erro ao salvar configurações: {ex.Message}";
                MessageBox.Show($"Erro ao salvar configurações: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Reset()
        {
            try
            {
                var result = MessageBox.Show(
                    "Deseja realmente restaurar as configurações padrão?",
                    "Confirmar Restauração",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    // Restaurar valores padrão
                    CompanyName = "Minha Empresa";
                    CompanyDocument = "12.345.678/0001-90";
                    SelectedBusinessProfile = "Farmácia";
                    SelectedCurrency = "BRL";
                    SelectedPrinter = "Impressora Padrão";
                    SelectedPaperSize = "80mm";
                    AutoPrint = true;
                    DatabaseServer = "localhost";
                    DatabaseName = "pos_multivertical";
                    DatabaseUser = "postgres";
                    ApiUrl = "https://localhost:5001";
                    ApiTimeout = 30;
                    UseHttps = true;
                    SelectedBarcodeReader = "USB HID";
                    SelectedScale = "Nenhuma";
                    SelectedCashDrawer = "USB";
                    SessionTimeout = 60;
                    RequirePasswordForCriticalOperations = true;
                    AuditLogEnabled = true;
                    BackupDirectory = @"C:\Backups\PDV";
                    AutoBackupEnabled = true;

                    StatusMessage = "Configurações restauradas para o padrão";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erro ao restaurar configurações: {ex.Message}";
            }
        }

        private void TestConnection()
        {
            try
            {
                StatusMessage = "Testando conexão com o banco de dados...";
                
                // Simular teste de conexão
                System.Threading.Thread.Sleep(2000);
                
                StatusMessage = "Conexão com o banco de dados estabelecida com sucesso!";
                MessageBox.Show("Conexão com o banco de dados estabelecida com sucesso!", "Teste de Conexão", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erro na conexão: {ex.Message}";
                MessageBox.Show($"Erro na conexão: {ex.Message}", "Erro de Conexão", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BrowseBackupDirectory()
        {
            try
            {
                var dialog = new OpenFolderDialog
                {
                    Title = "Selecionar Diretório de Backup"
                };

                if (dialog.ShowDialog() == true)
                {
                    BackupDirectory = dialog.FolderName;
                    StatusMessage = $"Diretório de backup alterado para: {BackupDirectory}";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erro ao selecionar diretório: {ex.Message}";
            }
        }

        private void ExecuteBackup()
        {
            try
            {
                StatusMessage = "Executando backup do banco de dados...";
                
                // Simular backup
                System.Threading.Thread.Sleep(3000);
                
                StatusMessage = "Backup executado com sucesso!";
                MessageBox.Show("Backup executado com sucesso!", "Backup", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erro ao executar backup: {ex.Message}";
                MessageBox.Show($"Erro ao executar backup: {ex.Message}", "Erro de Backup", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
