# PROMPT-MÃE — PDV Desktop Windows + App Mobile (Pedidos & Dashboards) com API .NET 8 — **Multi-Vertical (farmácia, material de construção, supermercado, lanchonete, …)**

Você é um(a) **arquiteto(a) de software sênior**. Projete um sistema **PDV** com **aplicativo desktop Windows** (caixa) e **app mobile** (Android em .NET MAUI) para **pedidos** e **dashboards**, sustentado por **API central ASP.NET Core (.NET 8)**. O sistema deve ser **multi-tenant** e **multi-vertical**, adaptável a diferentes ramos (farmácia, depósito de material de construção, supermercado, lanchonete/food service, etc.), ativando/desativando capacidades por **perfil de negócio**.

Entregue: **arquitetura**, **modelo multi-vertical**, **domínio e dados**, **endpoints**, **padrões**, **qualidade/observabilidade**, **implantação**, **scripts e exemplos de código essenciais**, além de **roteiro de entrega**. Trate explicitamente **offline-first**, **impressão ESC/POS**, **integrações periféricas**, **segurança (JWT/Refresh)** e **extensibilidade por plugins**.

---

## 1) Objetivo e princípios
- **Objetivo:** Frente de caixa Windows **robusta e offline**; **app mobile** para **pedidos** (pré-venda/mesa/entrega) e **dashboards**; **API central** para orquestração, estoque, preços, promoções e auditoria.
- **Princípios:**  
  1) **Core comum** + **módulos verticais** plugáveis.  
  2) **Config por tenant/loja** (feature flags, políticas e fluxos).  
  3) **Offline-first** no desktop; **cache offline** no mobile.  
  4) **Extensível sem fork** (plugins/DI, Domain Events).  
  5) **Observável** (logs/traces/métricas).  

---

## 2) Mapa de capacidades (core vs verticais)
### Core (sempre disponível)
- Catálogo (produto, variações, atributos), estoque, preços, promoções, clientes, pedidos/vendas, cancelamento/devolução, caixa/fechamento, impressão ESC/POS, gaveta, usuários/perfis (RBAC), auditoria, relatórios/KPIs básicos, sincronização offline/online, notificações (SignalR).

### Verticais (ativados por perfil do negócio)
- **Farmácia**: produto com **lote/validade**, equivalência terapêutica/genéricos, marcação de **necessita prescrição**, anexo/foto de receita, **controle de lotes** e baixa por **FEFO**; (planejar integração posterior a sistemas regulatórios quando aplicável).  
- **Depósito/Construção**: **unidades de medida e conversão** (m, m², m³, caixa, peça), venda fracionada, **BOM/kit** (ex.: conjunto de peças), itens pesados/volumosos, orçamento e reserva de estoque.  
- **Supermercado**: produtos **pesáveis** (balança), **código de barras com peso/preço embutido**, integração com **balança etiquetadora** e **balança de checkout**, ruptura e reposição, promoções mix-and-match.  
- **Lanchonete/Food service**: **comandas**, mesas, produção por **KDS** (Kitchen Display) e **impressora de cozinha**, **combos** e modificadores (sem cebola, + queijo), **delivery/retirada**, “happy hour”.

> Cada vertical liga/desliga **módulos** e **políticas** sem alterar o core.

---

## 3) Arquitetura (visão)
```
Apps:
  - Desktop Windows (WPF, MVVM) -> periféricos (ESC/POS, gaveta, balança, leitores)
  - Mobile .NET MAUI (Android) -> pedidos, dashboards, scanner de barras (ZXing)

Backend:
  - API ASP.NET Core (.NET 8, Minimal APIs) + EF Core + PostgreSQL
  - SignalR para eventos em tempo real
  - Autenticação JWT + Refresh Tokens
  - Observabilidade: Serilog + OpenTelemetry

Extensibilidade:
  - Módulos Verticais (NuGet/plugins) + Feature Flags por Tenant
  - Domain Events + Outbox (sync confiável)
```

**Padrões-chave:** DDD leve (Domain/Application/Infrastructure), Outbox Pattern, Strategy/Policy por vertical, Specification para validações, Event Sourcing opcional para auditoria fina, Circuit Breakers/Retry (Polly).

---

## 4) Organização da solution
```
/src
  Pos.Domain/            // entidades ricas, VOs, eventos
  Pos.Application/       // casos de uso, validações, orquestração
  Pos.Infrastructure/    // EF Core, repositórios, migrations
  Pos.Contracts/         // DTOs / clients compartilhados
  Pos.Api/               // ASP.NET Core (JWT, Swagger, SignalR)
  Pos.Desktop.Wpf/       // PDV Windows (MVVM, periféricos, offline)
  Pos.Mobile.Maui/       // Android (pedidos, dashboards, scanner)
  Pos.Plugins/           // módulos verticais (Farmacia, Mercado, Food, Construção)
```

---

## 5) Multi-vertical: modelo de extensão
### 5.1 Feature flags + perfil de negócio (por tenant/loja)
- Tabela `Tenant` com `BusinessProfile = {Pharmacy|Construction|Grocery|FoodService|Generic}`.  
- `TenantFeatureFlag( TenantId, Key, Enabled, ParamsJson )`.  
- UI de **Configuração** para ligar/desligar módulos e parametrizações (ex.: “usar balança”, “usar combos”, “usar lote/validade”).

### 5.2 Plugins (exemplo de contrato)
```csharp
public interface IVerticalPlugin
{
    string Key { get; } // "Pharmacy", "Grocery", ...
    void ConfigureServices(IServiceCollection services, IConfiguration cfg);
    void ConfigureModel(ModelBuilder mb); // EF extensões (ex.: Lote, Validade)
    void RegisterPolicies(IVerticalPolicyRegistry reg); // regras: preço, desconto, FEFO, etc.
}
```
- Plugins empacotados como **NuGet internos**, carregados por **DI**.  
- **Policy/Strategy** selecionada por tenant: `IPricingPolicy`, `IStockDecrementPolicy`, `IOrderWorkflow`.

### 5.3 Motor de regras e fluxo
- **Rules** (NRules, ou Specifications próprias) para:  
  - Farmácia: bloqueio sem prescrição, baixa por **FEFO**.  
  - Construção: conversões UoM (m² ↔ caixa), arredondamentos.  
  - Supermercado: parsing de **código pesável** (ex.: prefixo “2”, 5 dígitos SKU + 5 dígitos peso/preço).  
  - Food: **combos/modificadores**, roteamento para **cozinha**.

---

## 6) Domínio e dados (core + extensões)
### 6.1 Core (mínimo)
- **Product**(Id, Sku, Name, Price, Active, UpdatedAt)  
- **ProductAttribute** / **AttributeDef** (EAV leve por categoria/vertical)  
- **Stock**(ProductId, LocationId, Quantity, UpdatedAt)  
- **Customer**(Id, Name, Document, Phone, Email)  
- **Order**(Id, Number, CustomerId?, Status=[Draft|Placed|Cancelled|Fulfilled], Total, CreatedAt, UpdatedAt)  
- **OrderItem**(Id, OrderId, ProductId, Qty, UnitPrice, Subtotal, Notes?)  
- **Promotion**(Id, Type=[Percentage|Fixed|MixAndMatch|Combo|HappyHour], RulesJson, Validity)  
- **User**(Id, Login, Name, Role, Active)  
- **Outbox**(Id, EventType, PayloadJson, Attempts, NextRunAt, CreatedAt)

### 6.2 Extensões por vertical (tabelas opcionais)
- **Pharmacy**: `Batch`(Id, ProductId, LotCode, ExpiryDate, Qty), `Prescription`(Id, OrderId, ImageUrl, Number, IssuedAt), `DrugInfo`(ProductId, ActiveIngredient, RequiresPrescription)  
- **Construction**: `UoM`(Id, Code), `UoMConversion`(From, To, Factor), `BillOfMaterials`(ProductId, ComponentId, Qty)  
- **Grocery**: `ScaleTemplate`(Id, ProductId, LabelLayout), `WeighableConfig`(ProductId, Mode=[ByWeight|ByPrice])  
- **FoodService**: `Combo`(Id, ItemsJson, Price), `Modifier`(Id, ProductId, Name, PriceDelta), `KitchenRoute`(ProductId, Station)

> Use **UpdatedAt/rowversion** para **concorrência otimista**. No core, **idempotência** em integrações.

---

## 7) Endpoints (API) — contratos essenciais
- **Auth:** `POST /auth/login` → `{ accessToken, refreshToken, expiresIn }`, `POST /auth/refresh`.  
- **Products:**  
  - `GET /products?q=&page=&size=&tenant=`  
  - `GET /products/{id}`  
  - `POST /products` (respeita Policies de preço/atributos por vertical)
- **Orders:**  
  - `POST /orders` `{ customerId?, lines:[{productId, qty, unitPrice, notes?}] }`  
  - `GET /orders/{id}` | `GET /orders?status=&dateFrom=&dateTo=&page=`  
  - `PATCH /orders/{id}/cancel` | `PATCH /orders/{id}/fulfill`
- **Stock:** `POST /stock/movements` (entrada/saída), `GET /stock/{productId}`  
- **Promotions:** `GET /promotions/active` (pode retornar combos/happy hour)  
- **Metrics/Dash:** `GET /metrics/sales?range=today|week|month&tenant=`

**Tempo real:** `SignalR /hub` (mudança de preço/estoque, chamado de cozinha, status de pedido).

---

## 8) Desktop Windows (WPF) — frente de caixa
- **MVVM**, **SQLite local**, **Outbox** para eventos (OrderPlaced/Cancelled).  
- **Impressão ESC/POS** (USB/serial) + abertura de gaveta.  
- **Periféricos opcionais por vertical**:  
  - Leitor de código de barras (todos).  
  - **Balança** (supermercado): leitura serial/USB; modo **pré-peso** e **check-out**.  
  - **Impressora de cozinha/KDS** (food).  
- **Fiscal/TEF**: planejar integração só no desktop (módulos opcionais).  
- **Fluxo offline**: tudo persiste local → SyncWorker reenvia com **Polly** (retry/backoff/jitter).  

---

## 9) Mobile (MAUI) — pedidos & dashboards
- **Pedidos** com busca/scan (**ZXing.Net.MAUI**), carrinho local, envio à API.  
- **Dashboards** com **LiveChartsCore** (vendas do dia, ticket, top produtos).  
- **Cache offline**: `Products`, `OrdersDraft`.  
- **Perfis**: Vendedor (pedidos), Gestor (dash) — controlados por **RBAC**.

---

## 10) Políticas e regras por vertical (exemplos)
- **Pharmacy**  
  - Validação: `RequiresPrescription => bloquear/alertar` na finalização.  
  - **FEFO**: baixa automática do **lote com menor validade**.  
  - Bloqueio de venda de itens expirados.  
- **Construction**  
  - Conversões: por exemplo, **m² ↔ caixa** (fator, arredondamento “sempre pra cima”).  
  - **BOM/Kit**: baixa de componentes ao vender o kit.  
- **Grocery**  
  - **Código pesável**: reconhecer EAN com prefixo 2/20; extrair SKU + peso/preço.  
  - Promoções **mix-and-match** (leve 3 pague 2).  
- **FoodService**  
  - **Combos e modificadores** (regras de preço e de impressão por estação).  
  - **Roteamento de produção** (cozinha/bebidas/sobremesa).  

> Implementar como **Policies** injetadas por tenant (Strategy) + **Specifications** validando comandos.

---

## 11) Segurança e conformidade
- **JWT curto** (15 min) + **Refresh** (7–14 dias), **RBAC** por papel e tenant.  
- **Rate limiting**, **lockout** por tentativas, **CORS** mínimo.  
- Segredos em **KeyVault/variáveis**; logs sem dados sensíveis.  
- (Se necessário no futuro) Integrações regulatórias do setor implementadas como **plugins separados**.

---

## 12) Observabilidade e SRE
- **Serilog** (arquivo + Seq/ELK) com `CorrelationId` (propagado entre apps).  
- **OpenTelemetry** (traces + métricas: RPS, latência P95, erros, fila Outbox).  
- Alertas para **retries excessivos**, **fila Outbox atrasada**, **quedas de hub**.

---

## 13) Qualidade e testes
- **Unit** (Domain/Policies por vertical).  
- **Integração** (API com banco efêmero).  
- **End-to-End**:  
  - Farmácia: venda com lote/validade + prescrição.  
  - Construção: venda em m² convertendo para caixas.  
  - Supermercado: item pesável via código de balança.  
  - Food: pedido com combo + modificadores, impressão de cozinha.  
- **Carga**: 1.000 pedidos/10 min, latência API P95 < 500 ms.  
- **SAST/Dependabot/Sonar** (cobertura mínima 70% no core).

---

## 14) Implantação e distribuição
- **API**: Docker + Nginx (TLS), deploy em cloud (App Service/VM/K8s).  
- **Desktop**: **MSIX** assinado; canal interno de atualização.  
- **Mobile**: **AAB** (tracks: internal → closed → production).  
- **Backups** e **migrações EF** automatizadas.

---

## 15) Scripts e trechos essenciais
**Criação da solution**
```bash
dotnet new sln -n Pos
dotnet new classlib -n Pos.Domain
dotnet new classlib -n Pos.Application
dotnet new classlib -n Pos.Infrastructure
dotnet new classlib -n Pos.Contracts
dotnet new webapi -n Pos.Api
dotnet new wpf -n Pos.Desktop.Wpf
dotnet new maui -n Pos.Mobile.Maui
dotnet new classlib -n Pos.Plugins # solution folder para subprojetos
dotnet sln add **/*
```

**Interfaces de Policies (exemplo)**
```csharp
public interface IPricingPolicy { decimal GetPrice(Product p, Customer? c, DateTime when); }
public interface IStockDecrementPolicy { Task ApplyAsync(Order order, IStockRepo stock, CancellationToken ct); }
public interface IOrderWorkflow { Order Next(Order order, string action); } // Draft->Placed->Fulfilled...
```

**Registro por tenant (Strategy)**
```csharp
public static class VerticalRegistry
{
  public static void AddVerticalServices(this IServiceCollection s, string profile)
  {
    s.AddScoped<IPricingPolicy>(sp => profile switch {
       "Grocery" => sp.GetRequiredService<GroceryPricing>(),
       "FoodService" => sp.GetRequiredService<FoodPricing>(),
       "Pharmacy" => sp.GetRequiredService<PharmacyPricing>(),
       "Construction" => sp.GetRequiredService<ConstructionPricing>(),
       _ => sp.GetRequiredService<DefaultPricing>()
    });
    // idem para IStockDecrementPolicy, IOrderWorkflow, etc.
  }
}
```

**API mínima com SignalR (trecho)**
```csharp
var b = WebApplication.CreateBuilder(args);
b.Services.AddDbContext<AppDb>(o => o.UseNpgsql(b.Configuration.GetConnectionString("db")));
b.Services.AddAuthentication().AddJwtBearer();
b.Services.AddAuthorization();
b.Services.AddSignalR();
b.Services.AddEndpointsApiExplorer().AddSwaggerGen();
var app = b.Build();
app.UseSwagger().UseSwaggerUI();
app.UseAuthentication().UseAuthorization();
app.MapHub<PosHub>("/hub");
app.MapGet("/products", /* paginação + filtro + tenant */).RequireAuthorization();
app.MapPost("/orders", /* cria pedido e publica DomainEvent */).RequireAuthorization();
app.Run();
```

---

## 16) Roadmap incremental (multi-vertical)
**M1 — Núcleo executável (2–3 sem)**  
API (Auth/Products/Orders), Desktop (venda offline + impressão), Mobile (login + pedidos básicos), Observabilidade inicial.

**M2 — Verticalização base (2–3 sem)**  
- Farmácia: lote/validade + FEFO + prescrição (cadastro/anexo).  
- Construção: UoM + conversões + BOM/kit.  
- Grocery: parser de código **pesável** + integração balança básica.  
- Food: combos/modificadores + impressão de cozinha/KDS.

**M3 — Robustez e promo**  
Promo engine (mix-and-match, happy hour, combos), dashboards completos, SignalR para preço/estoque, hardening de sync.

**M4 — Integrações opcionais**  
TEF/fiscal no desktop, delivery agregadores (food), rotas de entrega simples (construção/supermercado).

---

## 17) Critérios de aceite por vertical (amostras)
- **Farmácia:** bloqueia item que exige prescrição sem anexo; baixa por **FEFO**; impede venda de lote vencido.  
- **Construção:** vende **3,2 m²** convertendo para **2 caixas** (arredondamento pra cima) e baixa estoque dos componentes (BOM).  
- **Supermercado:** lê código de balança e compõe item por **peso**; aplica **mix-and-match** corretamente.  
- **Food:** cria **combo** com **modificadores**, imprime ticket nas **estações corretas**.

---

## 18) Riscos e mitigação
- **Drivers/periféricos heterogêneos** → Homologar 2–3 marcas; camada `IReceiptPrinter`/`IScale`.  
- **Conflitos de estoque** → transações na API + concorrência otimista; eventos idempotentes.  
- **Complexidade de regras** → Policies/Specifications por vertical + testes de contrato.  
- **Offline prolongado** → Outbox com retenção/compactação; reconciliação assistida.

---

## 19) Entregáveis esperados
- Repositório com **solution** e projetos organizados + **pipelines CI/CD**.  
- **Documento de configurações por tenant** (feature flags, policies, perfis).  
- **Guia de homologação de periféricos** (impressora, balança, cozinha).  
- Testes automatizados cobrindo **políticas verticais** e **fluxos críticos**.
