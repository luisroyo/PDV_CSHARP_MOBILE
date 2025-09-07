# Documentação Técnica - Arquitetura do Sistema PDV

## 🏛️ Arquitetura Geral

### Padrões Arquiteturais
- **Domain-Driven Design (DDD)** - Separação clara entre domínio, aplicação e infraestrutura
- **Clean Architecture** - Dependências apontam para dentro
- **CQRS** - Separação entre comandos e consultas
- **Event Sourcing** - Para auditoria e sincronização
- **Outbox Pattern** - Garantia de entrega de eventos
- **Strategy Pattern** - Políticas por vertical
- **Plugin Architecture** - Extensibilidade sem fork

### Camadas da Aplicação

```
┌─────────────────────────────────────────────────────────────┐
│                    Presentation Layer                       │
├─────────────────┬─────────────────┬─────────────────────────┤
│   Desktop WPF   │   Mobile MAUI   │      Web API            │
│   (Offline)     │   (Cache)       │   (Real-time)           │
└─────────────────┴─────────────────┴─────────────────────────┘
┌─────────────────────────────────────────────────────────────┐
│                    Application Layer                        │
├─────────────────┬─────────────────┬─────────────────────────┤
│   Commands      │    Queries      │    Event Handlers       │
│   (CQRS)        │    (CQRS)       │    (Domain Events)      │
└─────────────────┴─────────────────┴─────────────────────────┘
┌─────────────────────────────────────────────────────────────┐
│                     Domain Layer                            │
├─────────────────┬─────────────────┬─────────────────────────┤
│   Entities      │   Value Objects │    Domain Events        │
│   (Rich Model)  │   (Immutables)  │    (Business Rules)     │
└─────────────────┴─────────────────┴─────────────────────────┘
┌─────────────────────────────────────────────────────────────┐
│                  Infrastructure Layer                       │
├─────────────────┬─────────────────┬─────────────────────────┤
│   Data Access   │   External APIs │    Cross-cutting        │
│   (EF Core)     │   (Integrations)│    (Logging, Auth)      │
└─────────────────┴─────────────────┴─────────────────────────┘
```

## 🗄️ Modelo de Dados

### Entidades Core

#### Product (Produto)
```csharp
public class Product : Entity
{
    public string Sku { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public bool Active { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<ProductAttribute> Attributes { get; set; }
}
```

#### Order (Pedido)
```csharp
public class Order : Entity
{
    public string Number { get; set; }
    public Guid? CustomerId { get; set; }
    public OrderStatus Status { get; set; }
    public decimal Total { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<OrderItem> Items { get; set; }
}

public enum OrderStatus
{
    Draft,      // Rascunho
    Placed,     // Confirmado
    Cancelled,  // Cancelado
    Fulfilled   // Atendido
}
```

#### Stock (Estoque)
```csharp
public class Stock : Entity
{
    public Guid ProductId { get; set; }
    public Guid LocationId { get; set; }
    public decimal Quantity { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

### Extensões por Vertical

#### Farmácia
```csharp
public class Batch : Entity
{
    public Guid ProductId { get; set; }
    public string LotCode { get; set; }
    public DateTime ExpiryDate { get; set; }
    public decimal Quantity { get; set; }
}

public class Prescription : Entity
{
    public Guid OrderId { get; set; }
    public string ImageUrl { get; set; }
    public string Number { get; set; }
    public DateTime IssuedAt { get; set; }
}
```

#### Construção
```csharp
public class UoM : Entity
{
    public string Code { get; set; }
    public string Name { get; set; }
    public string Type { get; set; } // Length, Area, Volume, Count
}

public class UoMConversion : Entity
{
    public Guid FromUoMId { get; set; }
    public Guid ToUoMId { get; set; }
    public decimal Factor { get; set; }
}
```

## 🔌 Sistema de Plugins

### Interface Base
```csharp
public interface IVerticalPlugin
{
    string Key { get; }
    string Name { get; }
    string Version { get; }
    
    void ConfigureServices(IServiceCollection services, IConfiguration config);
    void ConfigureModel(ModelBuilder modelBuilder);
    void RegisterPolicies(IVerticalPolicyRegistry registry);
    void RegisterEventHandlers(IEventHandlerRegistry registry);
}
```

### Registry de Políticas
```csharp
public interface IVerticalPolicyRegistry
{
    void RegisterPricingPolicy<T>() where T : class, IPricingPolicy;
    void RegisterStockDecrementPolicy<T>() where T : class, IStockDecrementPolicy;
    void RegisterOrderWorkflow<T>() where T : class, IOrderWorkflow;
    void RegisterValidationRule<T>() where T : class, IValidationRule;
}
```

### Políticas por Vertical

#### IPricingPolicy
```csharp
public interface IPricingPolicy
{
    decimal GetPrice(Product product, Customer customer, DateTime when);
    bool CanApplyDiscount(Product product, decimal discount);
}
```

#### IStockDecrementPolicy
```csharp
public interface IStockDecrementPolicy
{
    Task ApplyAsync(Order order, IStockRepository stockRepo, CancellationToken cancellationToken);
}
```

## 🔄 Sincronização Offline

### Outbox Pattern
```csharp
public class OutboxEvent : Entity
{
    public string EventType { get; set; }
    public string PayloadJson { get; set; }
    public int Attempts { get; set; }
    public DateTime? NextRunAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

### Worker de Sincronização
```csharp
public class SyncWorker : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var pendingEvents = await _outboxRepository.GetPendingEventsAsync();
            
            foreach (var evt in pendingEvents)
            {
                try
                {
                    await _eventPublisher.PublishAsync(evt);
                    await _outboxRepository.MarkAsProcessedAsync(evt.Id);
                }
                catch (Exception ex)
                {
                    await _outboxRepository.IncrementAttemptsAsync(evt.Id);
                    _logger.LogError(ex, "Erro ao processar evento {EventId}", evt.Id);
                }
            }
            
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}
```

## 🖥️ Desktop WPF

### MVVM Pattern
```csharp
public class MainViewModel : ViewModelBase
{
    private readonly IOrderService _orderService;
    private readonly IPrinterService _printerService;
    
    public ObservableCollection<OrderItem> CartItems { get; set; }
    public ICommand AddProductCommand { get; set; }
    public ICommand ProcessOrderCommand { get; set; }
}
```

### Serviços de Periféricos
```csharp
public interface IReceiptPrinter
{
    Task PrintAsync(Order order);
    Task OpenDrawerAsync();
}

public interface IBarcodeScanner
{
    event EventHandler<string> BarcodeScanned;
    void Start();
    void Stop();
}

public interface IScale
{
    Task<decimal> GetWeightAsync();
    event EventHandler<decimal> WeightChanged;
}
```

## 📱 Mobile MAUI

### Estrutura de Páginas
```
Pages/
├── LoginPage.xaml
├── OrdersPage.xaml
├── OrderDetailPage.xaml
├── DashboardPage.xaml
└── SettingsPage.xaml
```

### Serviços Mobile
```csharp
public interface IApiService
{
    Task<T> GetAsync<T>(string endpoint);
    Task<T> PostAsync<T>(string endpoint, object data);
    Task<bool> SyncOfflineDataAsync();
}

public interface IOfflineCache
{
    Task SaveAsync<T>(string key, T data);
    Task<T> GetAsync<T>(string key);
    Task ClearAsync();
}
```

## 🌐 API REST

### Endpoints Principais
```csharp
// Autenticação
POST /auth/login
POST /auth/refresh
POST /auth/logout

// Produtos
GET /products?q={query}&page={page}&size={size}
GET /products/{id}
POST /products
PUT /products/{id}

// Pedidos
POST /orders
GET /orders/{id}
GET /orders?status={status}&dateFrom={date}&dateTo={date}
PATCH /orders/{id}/cancel
PATCH /orders/{id}/fulfill

// Estoque
POST /stock/movements
GET /stock/{productId}

// Promoções
GET /promotions/active
```

### SignalR Hubs
```csharp
public class PosHub : Hub
{
    public async Task JoinGroup(string tenantId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"tenant-{tenantId}");
    }
    
    public async Task JoinOrderGroup(string orderId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"order-{orderId}");
    }
}
```

## 🔒 Segurança

### JWT Configuration
```csharp
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = configuration["Jwt:Issuer"],
            ValidAudience = configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(configuration["Jwt:Key"]))
        };
    });
```

### RBAC (Role-Based Access Control)
```csharp
public enum Role
{
    Cashier,        // Operador de caixa
    Manager,        // Gerente
    Admin,          // Administrador
    Salesperson     // Vendedor mobile
}

[Authorize(Roles = "Cashier,Manager")]
public class OrdersController : ControllerBase
{
    // Endpoints protegidos
}
```

## 📊 Observabilidade

### Logging Estruturado
```csharp
public class OrderService
{
    private readonly ILogger<OrderService> _logger;
    
    public async Task<Order> CreateOrderAsync(CreateOrderCommand command)
    {
        using var activity = _activitySource.StartActivity("CreateOrder");
        activity?.SetTag("order.customerId", command.CustomerId);
        
        _logger.LogInformation("Criando pedido para cliente {CustomerId}", command.CustomerId);
        
        try
        {
            var order = await _orderRepository.CreateAsync(command);
            _logger.LogInformation("Pedido {OrderId} criado com sucesso", order.Id);
            return order;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar pedido para cliente {CustomerId}", command.CustomerId);
            throw;
        }
    }
}
```

### Métricas Customizadas
```csharp
public class OrderMetrics
{
    private readonly Counter<int> _ordersCreated;
    private readonly Histogram<double> _orderProcessingTime;
    
    public OrderMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create("Pos.Orders");
        _ordersCreated = meter.CreateCounter<int>("orders_created_total");
        _orderProcessingTime = meter.CreateHistogram<double>("order_processing_duration_seconds");
    }
}
```

## 🧪 Estratégia de Testes

### Testes Unitários
- **Domain**: Entidades, Value Objects, Domain Events
- **Application**: Casos de uso, validações, policies
- **Infrastructure**: Repositórios, serviços externos

### Testes de Integração
- **API**: Endpoints com banco em memória
- **Database**: Migrations e queries complexas
- **External Services**: Mocks e stubs

### Testes E2E
- **Desktop**: Fluxo completo de venda
- **Mobile**: Pedidos e sincronização
- **Vertical**: Cenários específicos por ramo

## 🚀 Deploy e DevOps

### Docker
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Pos.Api/Pos.Api.csproj", "Pos.Api/"]
RUN dotnet restore "Pos.Api/Pos.Api.csproj"
COPY . .
WORKDIR "/src/Pos.Api"
RUN dotnet build "Pos.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Pos.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Pos.Api.dll"]
```

### CI/CD Pipeline
```yaml
name: Build and Deploy
on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v3
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '8.0.x'
    - name: Restore dependencies
      run: dotnet restore
    - name: Build
      run: dotnet build --no-restore
    - name: Test
      run: dotnet test --no-build --verbosity normal
```

---

**Documentação atualizada em**: 06/09/2025
**Versão**: 1.0.0
