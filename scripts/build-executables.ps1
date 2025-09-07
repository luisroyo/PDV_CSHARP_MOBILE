# Script para gerar executáveis do Sistema PDV Multi-Vertical

Write-Host "🚀 Gerando executáveis do Sistema PDV Multi-Vertical..." -ForegroundColor Green

# Criar diretório de publicação
$publishDir = ".\publish"
if (Test-Path $publishDir) {
    Remove-Item $publishDir -Recurse -Force
}
New-Item -Path $publishDir -ItemType Directory | Out-Null

Write-Host "📦 Compilando projeto Desktop WPF..." -ForegroundColor Yellow

# Compilar Desktop WPF
try {
    dotnet build Pos.Desktop.Wpf -c Release --no-restore
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Desktop WPF compilado com sucesso!" -ForegroundColor Green
        
        # Publicar Desktop WPF
        Write-Host "📦 Publicando Desktop WPF..." -ForegroundColor Yellow
        dotnet publish Pos.Desktop.Wpf -c Release -r win-x64 --self-contained true -o "$publishDir\Desktop" --no-build
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ Desktop WPF publicado com sucesso!" -ForegroundColor Green
            Write-Host "📁 Localização: $publishDir\Desktop" -ForegroundColor Cyan
        } else {
            Write-Host "❌ Erro ao publicar Desktop WPF" -ForegroundColor Red
        }
    } else {
        Write-Host "❌ Erro ao compilar Desktop WPF" -ForegroundColor Red
    }
} catch {
    Write-Host "❌ Erro: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "🌐 Compilando projeto API..." -ForegroundColor Yellow

# Compilar API
try {
    dotnet build Pos.Api -c Release --no-restore
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ API compilada com sucesso!" -ForegroundColor Green
        
        # Publicar API
        Write-Host "📦 Publicando API..." -ForegroundColor Yellow
        dotnet publish Pos.Api -c Release -r win-x64 --self-contained true -o "$publishDir\Api" --no-build
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ API publicada com sucesso!" -ForegroundColor Green
            Write-Host "📁 Localização: $publishDir\Api" -ForegroundColor Cyan
        } else {
            Write-Host "❌ Erro ao publicar API" -ForegroundColor Red
        }
    } else {
        Write-Host "❌ Erro ao compilar API" -ForegroundColor Red
    }
} catch {
    Write-Host "❌ Erro: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "🎯 Resumo da publicação:" -ForegroundColor Green
Write-Host "📁 Desktop WPF: $publishDir\Desktop" -ForegroundColor Cyan
Write-Host "🌐 API: $publishDir\Api" -ForegroundColor Cyan

Write-Host "✅ Processo de publicação concluído!" -ForegroundColor Green