# Documentação do Domínio - Sistema PDV Multi-Vertical

## 🏗️ Arquitetura do Domínio

O domínio do sistema PDV foi projetado seguindo os princípios do **Domain-Driven Design (DDD)** e **Clean Architecture**, garantindo:

- **Separação clara** entre regras de negócio e infraestrutura
- **Entidades ricas** com comportamento encapsulado
- **Value Objects** imutáveis para conceitos importantes
- **Domain Events** para comunicação entre bounded contexts
- **Interfaces** bem definidas para políticas e repositórios

## 📦 Estrutura do Domínio

```
Pos.Domain/
├── Entities/
│   ├── Base/
│   │   ├── Entity.cs              # Classe base para entidades
│   │   └── ValueObject.cs         # Classe base para value objects
│   ├── Product.cs                 # Produto
│   ├── ProductAttribute.cs        # Atributos do produto (EAV)
│   ├── Order.cs                   # Pedido/Venda
│   ├── OrderItem.cs               # Item do pedido
│   ├── OrderPayment.cs            # Pagamento do pedido
│   ├── Customer.cs                # Cliente
│   ├── Stock.cs                   # Estoque
│   ├── Location.cs                # Localização
│   ├── User.cs                    # Usuário
│   ├── Tenant.cs                  # Empresa/Loja (multi-tenant)
│   ├── Promotion.cs               # Promoção
│   └── OutboxEvent.cs             # Eventos para sincronização
├── ValueObjects/
│   ├── Money.cs                   # Valor monetário
│   └── Email.cs                   # Endereço de email
├── Events/
│   ├── Base/
│   │   └── DomainEvent.cs         # Classe base para eventos
│   ├── OrderEvents.cs             # Eventos de pedidos
│   └── StockEvents.cs             # Eventos de estoque
└── Interfaces/
    ├── IVerticalPlugin.cs         # Plugin vertical
    ├── IVerticalPolicyRegistry.cs # Registry de políticas
    ├── IEventHandlerRegistry.cs   # Registry de handlers
    ├── IPricingPolicy.cs          # Política de preços
    ├── IStockDecrementPolicy.cs   # Política de estoque
    ├── IOrderWorkflow.cs          # Fluxo de pedidos
    ├── IValidationRule.cs         # Regras de validação
    ├── IDiscountPolicy.cs         # Política de descontos
    ├── IRepository.cs             # Repositório base
    ├── IStockRepository.cs        # Repositório de estoque
    ├── IEventHandler.cs           # Handler de eventos
    ├── IDomainEventPublisher.cs   # Publicador de eventos
    └── IUnitOfWork.cs             # Unit of Work
```

## 🎯 Entidades Principais

### Product (Produto)
**Responsabilidade**: Representar um item vendável no sistema

**Propriedades**:
- `Sku`: Código único do produto
- `Name`: Nome do produto
- `Price`: Preço de venda
- `CostPrice`: Preço de custo
- `Active`: Status ativo/inativo
- `Category`: Categoria do produto
- `Barcode`: Código de barras
- `Weight`: Peso (para produtos pesáveis)
- `Unit`: Unidade de medida
- `MinStock`/`MaxStock`: Controle de estoque
- `TenantId`: ID do tenant (multi-tenant)

**Comportamentos**:
- Validação de dados na criação
- Atualização de informações básicas
- Controle de estoque mínimo/máximo
- Gerenciamento de atributos (EAV)
- Ativação/desativação

### Order (Pedido)
**Responsabilidade**: Representar uma venda ou pedido

**Propriedades**:
- `Number`: Número do pedido
- `CustomerId`: ID do cliente (opcional)
- `Status`: Status do pedido (Draft, Placed, Cancelled, Fulfilled)
- `Subtotal`: Subtotal dos itens
- `DiscountAmount`: Valor do desconto
- `TaxAmount`: Valor dos impostos
- `Total`: Valor total
- `Notes`: Observações
- `TenantId`: ID do tenant
- `UserId`: ID do usuário que criou

**Comportamentos**:
- Adição/remoção de itens
- Cálculo automático de totais
- Aplicação de descontos e impostos
- Controle de status (workflow)
- Gerenciamento de pagamentos

### Stock (Estoque)
**Responsabilidade**: Controlar o estoque de produtos

**Propriedades**:
- `ProductId`: ID do produto
- `LocationId`: ID da localização
- `Quantity`: Quantidade em estoque
- `ReservedQuantity`: Quantidade reservada
- `AvailableQuantity`: Quantidade disponível (calculada)
- `MinQuantity`/`MaxQuantity`: Controles de estoque

**Comportamentos**:
- Adição/remoção de quantidade
- Reserva de estoque
- Liberação de reservas
- Confirmação de reservas
- Controle de estoque mínimo/máximo

### Customer (Cliente)
**Responsabilidade**: Representar um cliente do sistema

**Propriedades**:
- `Name`: Nome do cliente
- `Document`: CPF/CNPJ
- `Email`: Email do cliente
- `Phone`: Telefone
- `Address`: Endereço completo
- `CreditLimit`: Limite de crédito
- `CurrentBalance`: Saldo atual

**Comportamentos**:
- Validação de dados
- Controle de limite de crédito
- Ativação/desativação
- Verificação de elegibilidade para compra

## 🔧 Value Objects

### Money (Valor Monetário)
**Responsabilidade**: Representar valores monetários de forma segura

**Características**:
- Imutável
- Suporte a diferentes moedas
- Operações matemáticas seguras
- Validação de valores negativos
- Arredondamento controlado

**Operações**:
- Soma, subtração, multiplicação, divisão
- Comparações (>, <, >=, <=)
- Arredondamento
- Formatação para exibição

### Email (Endereço de Email)
**Responsabilidade**: Representar endereços de email válidos

**Características**:
- Imutável
- Validação de formato
- Normalização (lowercase)
- Conversão implícita para string

## 📡 Domain Events

### OrderEvents
- **OrderCreatedEvent**: Pedido criado
- **OrderConfirmedEvent**: Pedido confirmado
- **OrderCancelledEvent**: Pedido cancelado
- **OrderCompletedEvent**: Pedido finalizado

### StockEvents
- **StockUpdatedEvent**: Estoque alterado
- **StockBelowMinimumEvent**: Estoque abaixo do mínimo
- **ProductOutOfStockEvent**: Produto sem estoque

## 🔌 Sistema de Plugins

### IVerticalPlugin
Interface base para todos os plugins verticais:

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

### Políticas por Vertical

#### IPricingPolicy
Calcula preços considerando regras específicas da vertical:
- Farmácia: Preços regulamentados
- Construção: Conversões de unidade
- Supermercado: Preços por peso
- Food Service: Preços de combos

#### IStockDecrementPolicy
Controla como o estoque é decrementado:
- Farmácia: FEFO (First Expired, First Out)
- Construção: BOM (Bill of Materials)
- Supermercado: Por peso
- Food Service: Por ingredientes

#### IOrderWorkflow
Define fluxos de trabalho específicos:
- Farmácia: Validação de prescrições
- Construção: Aprovação de orçamentos
- Supermercado: Validação de pesagem
- Food Service: Roteamento para cozinha

## 🏪 Multi-Tenancy

### Tenant (Empresa/Loja)
Cada tenant representa uma empresa ou loja no sistema:

**Propriedades**:
- `Name`: Nome da empresa
- `Code`: Código único
- `Document`: CNPJ
- `BusinessProfile`: Perfil de negócio (Pharmacy, Construction, etc.)
- `SettingsJson`: Configurações específicas

**Perfis de Negócio**:
- **Generic**: Genérico
- **Pharmacy**: Farmácia
- **Construction**: Material de construção
- **Grocery**: Supermercado
- **FoodService**: Lanchonete/Food service

## 🔄 Sincronização Offline

### OutboxEvent
Implementa o padrão Outbox para sincronização confiável:

**Características**:
- Armazena eventos para processamento assíncrono
- Retry com backoff exponencial
- Controle de tentativas máximas
- Processamento em background

**Fluxo**:
1. Evento é criado e salvo no Outbox
2. Worker de sincronização processa eventos pendentes
3. Evento é enviado para a API central
4. Sucesso: evento marcado como processado
5. Falha: evento agendado para nova tentativa

## 🧪 Validações e Regras

### IValidationRule
Interface para regras de validação específicas por vertical:

```csharp
public interface IValidationRule
{
    bool CanApply(Order order);
    Task<ValidationResult> ValidateAsync(Order order);
}
```

**Exemplos de regras**:
- Farmácia: Prescrição obrigatória para medicamentos controlados
- Construção: Validação de conversões de unidade
- Supermercado: Validação de códigos de balança
- Food Service: Validação de modificadores de combos

## 📊 Observabilidade

### Logging Estruturado
Todas as entidades suportam logging estruturado:
- CorrelationId para rastreamento
- Timestamps precisos
- Contexto do tenant
- Dados de auditoria

### Métricas de Domínio
- Contadores de eventos
- Histogramas de performance
- Alertas de negócio
- Dashboards específicos

## 🔒 Segurança

### Controle de Acesso
- RBAC (Role-Based Access Control)
- Controle por tenant
- Auditoria de alterações
- Validação de permissões

### Dados Sensíveis
- Hash de senhas
- Criptografia de dados sensíveis
- Logs sem informações pessoais
- Conformidade com LGPD

---

**Documentação atualizada em**: 06/09/2025
**Versão**: 1.0.0
