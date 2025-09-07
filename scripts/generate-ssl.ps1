# Script para gerar certificados SSL locais
# Sistema PDV Multi-Vertical

$SslPath = ".\nginx\ssl"
$CertFile = Join-Path $SslPath "cert.pem"
$KeyFile = Join-Path $SslPath "key.pem"

Write-Host "🔐 Gerando certificados SSL para servidor local..." -ForegroundColor Green

# Criar diretório SSL se não existir
if (!(Test-Path $SslPath)) {
    New-Item -ItemType Directory -Path $SslPath -Force
    Write-Host "📁 Diretório SSL criado: $SslPath" -ForegroundColor Green
}

# Verificar se OpenSSL está disponível
try {
    $opensslVersion = openssl version
    Write-Host "✅ OpenSSL encontrado: $opensslVersion" -ForegroundColor Green
} catch {
    Write-Host "❌ OpenSSL não encontrado. Instalando via Chocolatey..." -ForegroundColor Yellow
    
    # Tentar instalar via Chocolatey
    try {
        choco install openssl -y
        Write-Host "✅ OpenSSL instalado via Chocolatey" -ForegroundColor Green
    } catch {
        Write-Host "❌ Erro ao instalar OpenSSL. Instale manualmente:" -ForegroundColor Red
        Write-Host "1. Baixe de: https://slproweb.com/products/Win32OpenSSL.html" -ForegroundColor White
        Write-Host "2. Ou instale via Chocolatey: choco install openssl" -ForegroundColor White
        exit 1
    }
}

try {
    # Gerar chave privada
    Write-Host "🔑 Gerando chave privada..." -ForegroundColor Yellow
    openssl genrsa -out $KeyFile 2048

    # Gerar certificado auto-assinado
    Write-Host "📜 Gerando certificado auto-assinado..." -ForegroundColor Yellow
    openssl req -new -x509 -key $KeyFile -out $CertFile -days 365 -subj "/C=BR/ST=SP/L=SaoPaulo/O=PDV-Multi-Vertical/OU=IT/CN=localhost"

    if (Test-Path $CertFile -and Test-Path $KeyFile) {
        Write-Host "✅ Certificados SSL gerados com sucesso!" -ForegroundColor Green
        Write-Host "📁 Certificado: $CertFile" -ForegroundColor Cyan
        Write-Host "🔑 Chave: $KeyFile" -ForegroundColor Cyan
        Write-Host "⏰ Válido por: 365 dias" -ForegroundColor Cyan

        # Mostrar informações do certificado
        Write-Host "`n📋 Informações do certificado:" -ForegroundColor Yellow
        openssl x509 -in $CertFile -text -noout | Select-String "Subject:|Not Before:|Not After:"

    } else {
        Write-Host "❌ Erro ao gerar certificados" -ForegroundColor Red
        exit 1
    }

} catch {
    Write-Host "❌ Erro durante a geração dos certificados: $_" -ForegroundColor Red
    exit 1
}

Write-Host "`n🎉 Certificados SSL configurados!" -ForegroundColor Green
Write-Host "`n📋 Próximos passos:" -ForegroundColor Yellow
Write-Host "1. Execute: docker-compose -f docker-compose.production.yml up -d" -ForegroundColor White
Write-Host "2. Acesse: https://localhost:8443 (aceite o certificado auto-assinado)" -ForegroundColor White
Write-Host "3. API disponível em: https://localhost:8443/api" -ForegroundColor White
Write-Host "4. Swagger em: https://localhost:8443/swagger" -ForegroundColor White

Write-Host "`n⚠️ IMPORTANTE:" -ForegroundColor Yellow
Write-Host "- Este é um certificado auto-assinado para desenvolvimento local" -ForegroundColor White
Write-Host "- Para produção, use certificados de uma CA confiável" -ForegroundColor White
Write-Host "- O navegador mostrará aviso de segurança - aceite para continuar" -ForegroundColor White
