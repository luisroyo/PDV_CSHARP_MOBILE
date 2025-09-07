# Script PowerShell para iniciar o ambiente de desenvolvimento
# Execute como Administrador

Write-Host "=== PDV Multi-Vertical - Ambiente de Desenvolvimento ===" -ForegroundColor Green

# Verificar se o Docker está instalado
$docker = Get-Command docker -ErrorAction SilentlyContinue
if (-not $docker) {
    Write-Host "Docker não encontrado. Instalando via Chocolatey..." -ForegroundColor Yellow
    
    # Verificar se Chocolatey está instalado
    $choco = Get-Command choco -ErrorAction SilentlyContinue
    if (-not $choco) {
        Write-Host "Instalando Chocolatey..." -ForegroundColor Yellow
        Set-ExecutionPolicy Bypass -Scope Process -Force
        [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072
        iex ((New-Object System.Net.WebClient).DownloadString('https://community.chocolatey.org/install.ps1'))
    }
    
    # Instalar Docker Desktop
    choco install docker-desktop -y
    Write-Host "Docker Desktop instalado. Reinicie o computador e execute novamente." -ForegroundColor Red
    exit 1
}

# Verificar se o Docker está rodando
try {
    docker version | Out-Null
}
catch {
    Write-Host "Docker não está rodando. Inicie o Docker Desktop e execute novamente." -ForegroundColor Red
    exit 1
}

# Parar containers existentes
Write-Host "Parando containers existentes..." -ForegroundColor Yellow
docker-compose down

# Iniciar serviços
Write-Host "Iniciando serviços..." -ForegroundColor Yellow
docker-compose up -d postgres redis

# Aguardar PostgreSQL inicializar
Write-Host "Aguardando PostgreSQL inicializar..." -ForegroundColor Yellow
Start-Sleep -Seconds 10

# Executar migrações (se necessário)
Write-Host "Executando migrações..." -ForegroundColor Yellow
# dotnet ef database update --project Pos.Api

# Iniciar API
Write-Host "Iniciando API..." -ForegroundColor Yellow
Start-Process powershell -ArgumentList "-Command", "cd Pos.Api; dotnet run"

# Aguardar API inicializar
Write-Host "Aguardando API inicializar..." -ForegroundColor Yellow
Start-Sleep -Seconds 15

# Iniciar Desktop WPF
Write-Host "Iniciando Desktop WPF..." -ForegroundColor Yellow
Start-Process powershell -ArgumentList "-Command", "cd Pos.Desktop.Wpf; dotnet run"

Write-Host "=== Ambiente iniciado com sucesso! ===" -ForegroundColor Green
Write-Host "API: https://localhost:7001" -ForegroundColor Cyan
Write-Host "Swagger: https://localhost:7001/swagger" -ForegroundColor Cyan
Write-Host "PostgreSQL: localhost:5432" -ForegroundColor Cyan
Write-Host "Redis: localhost:6379" -ForegroundColor Cyan
Write-Host "" -ForegroundColor White
Write-Host "Para parar os serviços, execute: docker-compose down" -ForegroundColor Yellow
