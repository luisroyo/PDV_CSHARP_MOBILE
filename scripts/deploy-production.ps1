# Script de Deploy para Produção - PDV Multi-Vertical
# Execute como Administrador

param(
    [Parameter(Mandatory = $true)]
    [string]$DatabaseServer,
    
    [Parameter(Mandatory = $true)]
    [string]$DatabaseName,
    
    [Parameter(Mandatory = $true)]
    [string]$DatabaseUser,
    
    [Parameter(Mandatory = $true)]
    [string]$DatabasePassword,
    
    [Parameter(Mandatory = $true)]
    [string]$JwtSecretKey,
    
    [string]$ApiPort = "5000",
    [string]$HttpsPort = "5001"
)

Write-Host "=== DEPLOY PDV MULTI-VERTICAL - PRODUÇÃO ===" -ForegroundColor Green

# Verificar se o .NET 8 está instalado
$dotnetVersion = dotnet --version
if ($LASTEXITCODE -ne 0) {
    Write-Host "Erro: .NET 8 SDK não encontrado!" -ForegroundColor Red
    exit 1
}

Write-Host "Versão do .NET: $dotnetVersion" -ForegroundColor Cyan

# Criar arquivo de configuração de produção
$productionConfig = @"
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Host=$DatabaseServer;Database=$DatabaseName;Username=$DatabaseUser;Password=$DatabasePassword;Port=5432"
  },
  "Jwt": {
    "SecretKey": "$JwtSecretKey",
    "Issuer": "PDV-Multi-Vertical-Production",
    "Audience": "PDV-Users",
    "ExpirationMinutes": 60
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Warning",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      { "Name": "Console" },
      {
        "Name": "File",
        "Args": {
          "path": "/var/log/pos-api-.txt",
          "rollingInterval": "Day"
        }
      }
    ],
    "Enrich": [ "FromLogContext", "WithMachineName", "WithProcessId", "WithThreadId" ]
  }
}
"@

# Salvar configuração
$productionConfig | Out-File -FilePath "Pos.Api/appsettings.Production.json" -Encoding UTF8

Write-Host "Configuração de produção criada" -ForegroundColor Green

# Compilar em modo Release
Write-Host "Compilando aplicação..." -ForegroundColor Yellow
dotnet build -c Release

if ($LASTEXITCODE -ne 0) {
    Write-Host "Erro na compilação!" -ForegroundColor Red
    exit 1
}

# Executar migrações do banco
Write-Host "Executando migrações do banco de dados..." -ForegroundColor Yellow
cd Pos.Api
dotnet ef database update --environment Production

if ($LASTEXITCODE -ne 0) {
    Write-Host "Erro nas migrações do banco!" -ForegroundColor Red
    exit 1
}

cd ..

# Publicar aplicação
Write-Host "Publicando aplicação..." -ForegroundColor Yellow
dotnet publish Pos.Api -c Release -o ./publish/api
dotnet publish Pos.Desktop.Wpf -c Release -o ./publish/desktop
dotnet publish Pos.Mobile.Maui -c Release -o ./publish/mobile

# Criar script de inicialização
$startScript = @"
#!/bin/bash
cd /opt/pos-multivertical
export ASPNETCORE_ENVIRONMENT=Production
export ASPNETCORE_URLS="http://0.0.0.0:$ApiPort;https://0.0.0.0:$HttpsPort"
dotnet Pos.Api.dll
"@

$startScript | Out-File -FilePath "./publish/start-api.sh" -Encoding UTF8

Write-Host "=== DEPLOY CONCLUÍDO! ===" -ForegroundColor Green
Write-Host "Arquivos publicados em: ./publish/" -ForegroundColor Cyan
Write-Host "Para iniciar a API: ./publish/start-api.sh" -ForegroundColor Cyan
Write-Host "" -ForegroundColor White
Write-Host "Configurações aplicadas:" -ForegroundColor Yellow
Write-Host "- Servidor DB: $DatabaseServer" -ForegroundColor White
Write-Host "- Banco: $DatabaseName" -ForegroundColor White
Write-Host "- Porta HTTP: $ApiPort" -ForegroundColor White
Write-Host "- Porta HTTPS: $HttpsPort" -ForegroundColor White
