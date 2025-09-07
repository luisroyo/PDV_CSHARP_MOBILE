# Script PowerShell para configurar PostgreSQL no Windows
# Execute como Administrador

param(
    [string]$PostgresPassword = "postgres",
    [string]$DatabaseName = "pos_multivertical",
    [string]$Username = "postgres"
)

Write-Host "=== Configuração do PostgreSQL para PDV Multi-Vertical ===" -ForegroundColor Green

# Verificar se o PostgreSQL está instalado
$postgresPath = Get-Command psql -ErrorAction SilentlyContinue
if (-not $postgresPath) {
    Write-Host "PostgreSQL não encontrado. Instalando via Chocolatey..." -ForegroundColor Yellow
    
    # Verificar se Chocolatey está instalado
    $choco = Get-Command choco -ErrorAction SilentlyContinue
    if (-not $choco) {
        Write-Host "Instalando Chocolatey..." -ForegroundColor Yellow
        Set-ExecutionPolicy Bypass -Scope Process -Force
        [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072
        iex ((New-Object System.Net.WebClient).DownloadString('https://community.chocolatey.org/install.ps1'))
    }
    
    # Instalar PostgreSQL
    choco install postgresql --params '/Password:postgres' -y
    refreshenv
}

# Configurar variáveis de ambiente
$env:PATH = [System.Environment]::GetEnvironmentVariable("PATH", "Machine") + ";" + [System.Environment]::GetEnvironmentVariable("PATH", "User")

# Aguardar o serviço iniciar
Write-Host "Aguardando serviço PostgreSQL iniciar..." -ForegroundColor Yellow
Start-Sleep -Seconds 10

# Criar banco de dados
Write-Host "Criando banco de dados $DatabaseName..." -ForegroundColor Yellow
$createDbCommand = "CREATE DATABASE $DatabaseName;"
$createDbCommand | psql -U $Username -h localhost

# Executar script de configuração
Write-Host "Executando script de configuração..." -ForegroundColor Yellow
Get-Content "setup-database.sql" | psql -U $Username -h localhost -d $DatabaseName

# Testar conexão
Write-Host "Testando conexão..." -ForegroundColor Yellow
$testQuery = "SELECT version();"
$testQuery | psql -U $Username -h localhost -d $DatabaseName

Write-Host "=== Configuração concluída! ===" -ForegroundColor Green
Write-Host "String de conexão: Host=localhost;Database=$DatabaseName;Username=$Username;Password=$PostgresPassword;Port=5432" -ForegroundColor Cyan
