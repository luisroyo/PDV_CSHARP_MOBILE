# PDV Multi-Vertical - Sistema de Ponto de Venda

Sistema completo de PDV (Ponto de Venda) multi-vertical desenvolvido em .NET 8, com suporte a diferentes tipos de negócio (farmácia, construção, supermercado, alimentação) através de plugins modulares.

## 🏗️ Arquitetura

### Estrutura da Solução
```
PDV_CSHARP_MOBILE/
├── Pos.Domain/                    # Entidades de domínio e regras de negócio
├── Pos.Application/               # Casos de uso e serviços de aplicação
├── Pos.Infrastructure/            # Implementações de infraestrutura
├── Pos.Api/                      # API REST com ASP.NET Core
├── Pos.Desktop.Wpf/              # Aplicação desktop Windows (PDV)
├── Pos.Mobile.Maui/              # Aplicação mobile multiplataforma
├── Pos.Plugins.Pharmacy/         # Plugin para farmácia
├── Pos.Plugins.Construction/     # Plugin para construção
├── Pos.Plugins.Grocery/          # Plugin para supermercado
├── Pos.Plugins.FoodService/      # Plugin para alimentação
├── Pos.Tests/                    # Testes unitários
└── scripts/                      # Scripts de configuração
```

### Tecnologias Utilizadas

**Backend:**
- ASP.NET Core 8.0
- Entity Framework Core 8.0
- PostgreSQL
- SignalR
- JWT Authentication
- Serilog
- xUnit

**Desktop (WPF):**
- WPF (.NET 8)
- MVVM Pattern
- SQLite (cache offline)
- ESC/POS Printing
- Peripheral Integration

**Mobile (MAUI):**
- .NET MAUI 8.0
- SQLite (cache offline)
- ZXing.Net.MAUI (scanner)
- LiveChartsCore (dashboards)

## 🚀 Configuração e Instalação

### Pré-requisitos

1. **.NET 8.0 SDK**
   ```bash
   # Verificar instalação
   dotnet --version
   ```

2. **PostgreSQL 15+**
   - Instalar PostgreSQL
   - Criar banco de dados `pos_multivertical`
   - Executar script de configuração

3. **Visual Studio 2022** ou **VS Code** com extensões C#

### Configuração do Banco de Dados

1. **Instalar PostgreSQL:**
   ```bash
   # Windows (via Chocolatey)
   choco install postgresql --params '/Password:postgres'
   
   # Ou baixar de: https://www.postgresql.org/download/
   ```

2. **Configurar banco:**
   ```bash
   # Executar script de configuração
   psql -U postgres -f scripts/setup-database.sql
   ```

3. **Configurar string de conexão:**
   ```json
   // Pos.Api/appsettings.json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Database=pos_multivertical;Username=postgres;Password=postgres;Port=5432"
     }
   }
   ```

### Executando o Projeto

1. **Clonar repositório:**
   ```bash
   git clone <repository-url>
   cd PDV_CSHARP_MOBILE
   ```

2. **Restaurar dependências:**
   ```bash
   dotnet restore
   ```

3. **Compilar solução:**
   ```bash
   dotnet build
   ```

4. **Executar API:**
   ```bash
   cd Pos.Api
   dotnet run
   ```

5. **Executar Desktop WPF:**
   ```bash
   cd Pos.Desktop.Wpf
   dotnet run
   ```

6. **Executar Mobile MAUI:**
   ```bash
   cd Pos.Mobile.Maui
   dotnet run --framework net8.0-windows10.0.19041.0
   ```

## 📱 Funcionalidades

### API (Backend)
- ✅ Autenticação JWT + Refresh Token
- ✅ Endpoints REST para produtos, pedidos, clientes
- ✅ Dashboard com métricas em tempo real
- ✅ SignalR para notificações em tempo real
- ✅ Logging estruturado com Serilog
- ✅ Swagger/OpenAPI documentation
- ✅ Multi-tenancy
- ✅ Arquitetura de plugins

### Desktop WPF (PDV)
- ✅ Interface moderna e responsiva
- ✅ Gestão de produtos e pedidos
- ✅ Carrinho de compras
- ✅ Impressão ESC/POS (simulada)
- ✅ Integração com periféricos (simulada)
- ✅ Cache offline com SQLite
- ✅ Sincronização automática
- ✅ Autenticação JWT

### Mobile MAUI
- ✅ Dashboard com métricas
- ✅ Lista de produtos e pedidos
- ✅ Scanner de código de barras
- ✅ Cache offline com SQLite
- ✅ Sincronização manual/automática
- ✅ Autenticação JWT
- ✅ Interface responsiva

### Plugins Verticais
- ✅ **Farmácia**: Controle de medicamentos, prescrições
- ✅ **Construção**: Materiais, orçamentos, projetos
- ✅ **Supermercado**: Produtos perecíveis, promoções
- ✅ **Alimentação**: Cardápio, delivery, cozinha

## 🔧 Configuração Avançada

### Configuração de Impressão ESC/POS

1. **Instalar driver da impressora:**
   - Configurar impressora ESC/POS
   - Definir IP e porta (padrão: 192.168.1.100:9100)

2. **Configurar no código:**
   ```csharp
   // Pos.Desktop.Wpf/Services/PrinterService.cs
   // Descomentar linhas para usar impressora real
   ```

### Configuração de Periféricos

1. **Leitor de código de barras:**
   - Conectar via USB ou Bluetooth
   - Configurar como teclado virtual

2. **Balança:**
   - Conectar via USB ou serial
   - Configurar protocolo de comunicação

### Configuração de Cache Offline

1. **Desktop WPF:**
   - Banco SQLite: `pos_offline.db`
   - Sincronização automática a cada 5 minutos

2. **Mobile MAUI:**
   - Banco SQLite: `pos_mobile_offline.db`
   - Sincronização manual via botão

## 🧪 Testes

### Executar Testes Unitários
```bash
dotnet test Pos.Tests
```

### Testes Disponíveis
- ✅ JwtServiceTests
- ✅ AuthControllerTests
- ✅ ProductsControllerTests
- ✅ OrdersControllerTests

## 📊 Monitoramento e Logs

### Logs
- **API**: Arquivos em `logs/pos-api-*.txt`
- **Desktop**: Console + arquivo
- **Mobile**: Console

### Métricas
- Total de vendas
- Número de pedidos
- Ticket médio
- Produtos mais vendidos

## 🔐 Segurança

### Autenticação
- JWT tokens com expiração de 60 minutos
- Refresh tokens para renovação automática
- Armazenamento seguro de tokens

### Autorização
- RBAC (Role-Based Access Control)
- Permissões por tenant
- Controle de acesso por funcionalidade

## 🚀 Deploy

### API (Produção)
```bash
# Build para produção
dotnet publish Pos.Api -c Release -o ./publish

# Executar com Docker
docker build -t pos-api .
docker run -p 5000:80 pos-api
```

### Desktop WPF
```bash
# Build para produção
dotnet publish Pos.Desktop.Wpf -c Release -o ./publish
```

### Mobile MAUI
```bash
# Android
dotnet build Pos.Mobile.Maui -f net8.0-android -c Release

# iOS
dotnet build Pos.Mobile.Maui -f net8.0-ios -c Release
```

## 📝 Documentação da API

### Endpoints Principais

#### Autenticação
- `POST /api/auth/login` - Login
- `POST /api/auth/refresh` - Renovar token
- `POST /api/auth/logout` - Logout

#### Produtos
- `GET /api/products` - Listar produtos
- `GET /api/products/{id}` - Buscar produto
- `POST /api/products` - Criar produto
- `PUT /api/products/{id}` - Atualizar produto
- `DELETE /api/products/{id}` - Excluir produto

#### Pedidos
- `GET /api/orders` - Listar pedidos
- `GET /api/orders/{id}` - Buscar pedido
- `POST /api/orders` - Criar pedido
- `PUT /api/orders/{id}` - Atualizar pedido
- `DELETE /api/orders/{id}` - Excluir pedido

#### Dashboard
- `GET /api/dashboard/metrics` - Métricas do dashboard

### Swagger UI
Acesse `https://localhost:7001/swagger` para documentação interativa da API.

## 🤝 Contribuição

1. Fork o projeto
2. Crie uma branch para sua feature (`git checkout -b feature/AmazingFeature`)
3. Commit suas mudanças (`git commit -m 'Add some AmazingFeature'`)
4. Push para a branch (`git push origin feature/AmazingFeature`)
5. Abra um Pull Request

## 📄 Licença

Este projeto está sob a licença MIT. Veja o arquivo `LICENSE` para mais detalhes.

## 🆘 Suporte

Para suporte e dúvidas:
- Abra uma issue no GitHub
- Consulte a documentação da API
- Verifique os logs de erro

## 🎯 Roadmap

### Próximas Funcionalidades
- [ ] Cache Redis para performance
- [ ] Sistema de backup automático
- [ ] Relatórios avançados
- [ ] Integração com sistemas de pagamento
- [ ] App para cozinha (restaurantes)
- [ ] Dashboard web administrativo
- [ ] Integração com ERPs
- [ ] Sistema de notificações push

---

**Desenvolvido com ❤️ usando .NET 8, WPF e MAUI**#   P D V _ C S H A R P _ M O B I L E  
 