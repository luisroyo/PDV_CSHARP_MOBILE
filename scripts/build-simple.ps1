Write-Host "Gerando executáveis do Sistema PDV..." -ForegroundColor Green

# Criar diretório de publicação
if (Test-Path ".\publish") {
    Remove-Item ".\publish" -Recurse -Force
}
New-Item -Path ".\publish" -ItemType Directory | Out-Null

# Compilar e publicar API
Write-Host "Compilando API..." -ForegroundColor Yellow
dotnet publish Pos.Api -c Release -r win-x64 --self-contained true -o ".\publish\Api"

# Compilar e publicar Desktop
Write-Host "Compilando Desktop..." -ForegroundColor Yellow
dotnet publish Pos.Desktop.Wpf -c Release -r win-x64 --self-contained true -o ".\publish\Desktop"

Write-Host "Executáveis gerados em .\publish\" -ForegroundColor Green
