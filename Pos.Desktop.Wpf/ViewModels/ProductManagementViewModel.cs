using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Pos.Desktop.Wpf.Models;

namespace Pos.Desktop.Wpf.ViewModels
{
    public class ProductManagementViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<ProductDto> _products;
        private ProductDto _selectedProduct;
        private ProductFormDto _productForm;
        private string _searchText = string.Empty;
        private string _selectedCategory = string.Empty;
        private string _errorMessage = string.Empty;
        private string _statusMessage = "Pronto";

        public ProductManagementViewModel()
        {
            _products = new ObservableCollection<ProductDto>();
            _productForm = new ProductFormDto();
            
            // Commands
            NewProductCommand = new RelayCommand(NewProduct);
            EditProductCommand = new RelayCommand<ProductDto>(EditProduct);
            DeleteProductCommand = new RelayCommand<ProductDto>(DeleteProduct);
            SaveProductCommand = new RelayCommand(SaveProduct, () => IsFormValid());
            CancelCommand = new RelayCommand(Cancel);
            SearchCommand = new RelayCommand(Search);
            RefreshCommand = new RelayCommand(Refresh);

            // Categories
            Categories = new ObservableCollection<string>
            {
                "Medicamentos",
                "Construção",
                "Alimentos",
                "Bebidas",
                "Higiene",
                "Limpeza",
                "Outros"
            };

            // Units
            Units = new ObservableCollection<string>
            {
                "UN", "KG", "G", "L", "ML", "M", "M²", "M³", "CX", "PC"
            };

            LoadProducts();
        }

        public ObservableCollection<ProductDto> Products
        {
            get => _products;
            set
            {
                _products = value;
                OnPropertyChanged();
            }
        }

        public ProductDto SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                _selectedProduct = value;
                OnPropertyChanged();
            }
        }

        public ProductFormDto ProductForm
        {
            get => _productForm;
            set
            {
                _productForm = value;
                OnPropertyChanged();
                ((RelayCommand)SaveProductCommand).RaiseCanExecuteChanged();
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

        public string SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                _selectedCategory = value;
                OnPropertyChanged();
                Search();
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

        public ObservableCollection<string> Categories { get; }
        public ObservableCollection<string> Units { get; }

        public ICommand NewProductCommand { get; }
        public ICommand EditProductCommand { get; }
        public ICommand DeleteProductCommand { get; }
        public ICommand SaveProductCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand RefreshCommand { get; }

        private void LoadProducts()
        {
            // Simular carregamento de produtos
            Products.Clear();
            
            var sampleProducts = new[]
            {
                new ProductDto { Id = 1, Sku = "MED001", Name = "Paracetamol 500mg", Category = "Medicamentos", Price = 12.50m, Stock = 100, Active = true },
                new ProductDto { Id = 2, Sku = "MED002", Name = "Ibuprofeno 400mg", Category = "Medicamentos", Price = 15.80m, Stock = 50, Active = true },
                new ProductDto { Id = 3, Sku = "CON001", Name = "Cimento 50kg", Category = "Construção", Price = 25.00m, Stock = 20, Active = true },
                new ProductDto { Id = 4, Sku = "GRO001", Name = "Arroz 5kg", Category = "Alimentos", Price = 18.90m, Stock = 30, Active = true },
                new ProductDto { Id = 5, Sku = "FOO001", Name = "Hambúrguer", Category = "Alimentos", Price = 15.00m, Stock = 25, Active = true }
            };

            foreach (var product in sampleProducts)
            {
                Products.Add(product);
            }

            StatusMessage = $"Carregados {Products.Count} produtos";
        }

        private void NewProduct()
        {
            ProductForm = new ProductFormDto
            {
                Active = true,
                Unit = "UN"
            };
            ErrorMessage = string.Empty;
            StatusMessage = "Novo produto - preencha os dados";
        }

        private void EditProduct(ProductDto product)
        {
            if (product == null) return;

            ProductForm = new ProductFormDto
            {
                Id = product.Id,
                Sku = product.Sku,
                Name = product.Name,
                Description = product.Description,
                Category = product.Category,
                Price = product.Price,
                Barcode = product.Barcode,
                Unit = product.Unit,
                Stock = product.Stock,
                Active = product.Active
            };
            ErrorMessage = string.Empty;
            StatusMessage = $"Editando produto: {product.Name}";
        }

        private void DeleteProduct(ProductDto product)
        {
            if (product == null) return;

            var result = MessageBox.Show(
                $"Deseja realmente excluir o produto '{product.Name}'?",
                "Confirmar Exclusão",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                Products.Remove(product);
                StatusMessage = $"Produto '{product.Name}' excluído";
            }
        }

        private void SaveProduct()
        {
            try
            {
                ErrorMessage = string.Empty;

                // Validação
                if (string.IsNullOrWhiteSpace(ProductForm.Sku))
                {
                    ErrorMessage = "SKU é obrigatório";
                    return;
                }

                if (string.IsNullOrWhiteSpace(ProductForm.Name))
                {
                    ErrorMessage = "Nome é obrigatório";
                    return;
                }

                if (string.IsNullOrWhiteSpace(ProductForm.Category))
                {
                    ErrorMessage = "Categoria é obrigatória";
                    return;
                }

                if (ProductForm.Price <= 0)
                {
                    ErrorMessage = "Preço deve ser maior que zero";
                    return;
                }

                // Verificar se SKU já existe
                var existingProduct = Products.FirstOrDefault(p => p.Sku == ProductForm.Sku && p.Id != ProductForm.Id);
                if (existingProduct != null)
                {
                    ErrorMessage = "SKU já existe para outro produto";
                    return;
                }

                if (ProductForm.Id == 0)
                {
                    // Novo produto
                    var newProduct = new ProductDto
                    {
                        Id = Products.Count + 1,
                        Sku = ProductForm.Sku,
                        Name = ProductForm.Name,
                        Description = ProductForm.Description,
                        Category = ProductForm.Category,
                        Price = ProductForm.Price,
                        Barcode = ProductForm.Barcode,
                        Unit = ProductForm.Unit,
                        Stock = ProductForm.Stock,
                        Active = ProductForm.Active
                    };
                    Products.Add(newProduct);
                    StatusMessage = $"Produto '{newProduct.Name}' criado com sucesso";
                }
                else
                {
                    // Editar produto existente
                    var existing = Products.FirstOrDefault(p => p.Id == ProductForm.Id);
                    if (existing != null)
                    {
                        existing.Sku = ProductForm.Sku;
                        existing.Name = ProductForm.Name;
                        existing.Description = ProductForm.Description;
                        existing.Category = ProductForm.Category;
                        existing.Price = ProductForm.Price;
                        existing.Barcode = ProductForm.Barcode;
                        existing.Unit = ProductForm.Unit;
                        existing.Stock = ProductForm.Stock;
                        existing.Active = ProductForm.Active;
                        StatusMessage = $"Produto '{existing.Name}' atualizado com sucesso";
                    }
                }

                // Limpar formulário
                ProductForm = new ProductFormDto();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Erro ao salvar produto: {ex.Message}";
            }
        }

        private void Cancel()
        {
            ProductForm = new ProductFormDto();
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
            LoadProducts();
        }

        private bool IsFormValid()
        {
            return !string.IsNullOrWhiteSpace(ProductForm.Sku) &&
                   !string.IsNullOrWhiteSpace(ProductForm.Name) &&
                   !string.IsNullOrWhiteSpace(ProductForm.Category) &&
                   ProductForm.Price > 0;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ProductFormDto : INotifyPropertyChanged
    {
        private int _id;
        private string _sku = string.Empty;
        private string _name = string.Empty;
        private string _description = string.Empty;
        private string _category = string.Empty;
        private decimal _price;
        private string _barcode = string.Empty;
        private string _unit = "UN";
        private decimal _stock;
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

        public string Sku
        {
            get => _sku;
            set
            {
                _sku = value;
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

        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                OnPropertyChanged();
            }
        }

        public string Category
        {
            get => _category;
            set
            {
                _category = value;
                OnPropertyChanged();
            }
        }

        public decimal Price
        {
            get => _price;
            set
            {
                _price = value;
                OnPropertyChanged();
            }
        }

        public string Barcode
        {
            get => _barcode;
            set
            {
                _barcode = value;
                OnPropertyChanged();
            }
        }

        public string Unit
        {
            get => _unit;
            set
            {
                _unit = value;
                OnPropertyChanged();
            }
        }

        public decimal Stock
        {
            get => _stock;
            set
            {
                _stock = value;
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
