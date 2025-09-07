# Script de Deploy para Produção Local
# Sistema PDV Multi-Vertical

param(
    [switch]$SkipBackup = $false,
    [switch]$SkipSSL = $false,
    [switch]$Force = $false
)

Write-Host "🚀 Deploy para Produção Local - Sistema PDV Multi-Vertical" -ForegroundColor Green
Write-Host "=" * 60

# Verificar se está executando como administrador
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")
if (!$isAdmin) {
    Write-Host "⚠️ Execute como Administrador para melhor compatibilidade." -ForegroundColor Yellow
}

# Verificar Docker
try {
    $dockerVersion = docker --version
    Write-Host "✅ Docker encontrado: $dockerVersion" -ForegroundColor Green
} catch {
    Write-Host "❌ Docker não encontrado. Instale o Docker Desktop primeiro." -ForegroundColor Red
    exit 1
}

# Verificar Docker Compose
try {
    $composeVersion = docker-compose --version
    Write-Host "✅ Docker Compose encontrado: $composeVersion" -ForegroundColor Green
} catch {
    Write-Host "❌ Docker Compose não encontrado. Instale o Docker Desktop primeiro." -ForegroundColor Red
    exit 1
}

# Backup do banco existente
if (!$SkipBackup) {
    Write-Host "`n💾 Fazendo backup do banco existente..." -ForegroundColor Yellow
    try {
        & ".\scripts\backup-database.ps1" -BackupPath ".\backups" -RetentionDays 30 -Compress
        Write-Host "✅ Backup concluído" -ForegroundColor Green
    } catch {
        Write-Host "⚠️ Erro no backup: $_" -ForegroundColor Yellow
        if (!$Force) {
            $continue = Read-Host "Deseja continuar mesmo assim? (s/N)"
            if ($continue -ne "s" -and $continue -ne "S") {
                Write-Host "❌ Deploy cancelado" -ForegroundColor Red
                exit 0
            }
        }
    }
}

# Gerar certificados SSL
if (!$SkipSSL) {
    Write-Host "`n🔐 Gerando certificados SSL..." -ForegroundColor Yellow
    try {
        & ".\scripts\generate-ssl.ps1"
        Write-Host "✅ Certificados SSL gerados" -ForegroundColor Green
    } catch {
        Write-Host "⚠️ Erro ao gerar SSL: $_" -ForegroundColor Yellow
        if (!$Force) {
            $continue = Read-Host "Deseja continuar sem SSL? (s/N)"
            if ($continue -ne "s" -and $continue -ne "S") {
                Write-Host "❌ Deploy cancelado" -ForegroundColor Red
                exit 0
            }
        }
    }
}

# Parar containers existentes
Write-Host "`n🛑 Parando containers existentes..." -ForegroundColor Yellow
docker-compose down
docker-compose -f docker-compose.production.yml down

# Limpar containers órfãos
Write-Host "🧹 Limpando containers órfãos..." -ForegroundColor Yellow
docker system prune -f

# Construir imagens
Write-Host "`n🏗️ Construindo imagens..." -ForegroundColor Yellow
docker-compose -f docker-compose.production.yml build --no-cache

# Iniciar serviços
Write-Host "`n🚀 Iniciando serviços de produção..." -ForegroundColor Yellow
docker-compose -f docker-compose.production.yml up -d

# Aguardar serviços inicializarem
Write-Host "`n⏳ Aguardando serviços inicializarem..." -ForegroundColor Yellow
Start-Sleep -Seconds 30

# Verificar saúde dos serviços
Write-Host "`n🔍 Verificando saúde dos serviços..." -ForegroundColor Yellow

# Verificar PostgreSQL
try {
    $pgStatus = docker exec pos_postgres_prod pg_isready -U pos_user -d pos_multivertical
    if ($pgStatus -like "*accepting connections*") {
        Write-Host "✅ PostgreSQL: Funcionando" -ForegroundColor Green
    } else {
        Write-Host "❌ PostgreSQL: Problema" -ForegroundColor Red
    }
} catch {
    Write-Host "❌ PostgreSQL: Erro na verificação" -ForegroundColor Red
}

# Verificar Redis
try {
    $redisStatus = docker exec pos_redis_prod redis-cli ping
    if ($redisStatus -eq "PONG") {
        Write-Host "✅ Redis: Funcionando" -ForegroundColor Green
    } else {
        Write-Host "❌ Redis: Problema" -ForegroundColor Red
    }
} catch {
    Write-Host "❌ Redis: Erro na verificação" -ForegroundColor Red
}

# Verificar API
try {
    $apiStatus = Invoke-RestMethod -Uri "http://localhost/health" -TimeoutSec 10
    if ($apiStatus.Status -eq "Healthy") {
        Write-Host "✅ API: Funcionando" -ForegroundColor Green
    } else {
        Write-Host "❌ API: Problema" -ForegroundColor Red
    }
} catch {
    Write-Host "❌ API: Erro na verificação" -ForegroundColor Red
}

# Verificar Nginx
try {
    $nginxStatus = Invoke-WebRequest -Uri "https://localhost:8443" -SkipCertificateCheck -TimeoutSec 10
    if ($nginxStatus.StatusCode -eq 200) {
        Write-Host "✅ Nginx: Funcionando" -ForegroundColor Green
    } else {
        Write-Host "❌ Nginx: Problema" -ForegroundColor Red
    }
} catch {
    Write-Host "❌ Nginx: Erro na verificação" -ForegroundColor Red
}

# Mostrar status dos containers
Write-Host "`n📊 Status dos containers:" -ForegroundColor Yellow
docker-compose -f docker-compose.production.yml ps

# Mostrar logs da API
Write-Host "`n📝 Últimas linhas do log da API:" -ForegroundColor Yellow
docker-compose -f docker-compose.production.yml logs --tail=10 api

Write-Host "`n🎉 Deploy concluído!" -ForegroundColor Green
Write-Host "`n📋 URLs de acesso:" -ForegroundColor Yellow
Write-Host "  🌐 API HTTP: http://localhost" -ForegroundColor White
Write-Host "  🔒 API HTTPS: https://localhost:8443" -ForegroundColor White
Write-Host "  📚 Swagger: https://localhost:8443/swagger" -ForegroundColor White
Write-Host "  ❤️ Health: https://localhost:8443/health" -ForegroundColor White

Write-Host "`n📋 Comandos úteis:" -ForegroundColor Yellow
Write-Host "  Ver logs: docker-compose -f docker-compose.production.yml logs -f" -ForegroundColor White
Write-Host "  Parar: docker-compose -f docker-compose.production.yml down" -ForegroundColor White
Write-Host "  Reiniciar: docker-compose -f docker-compose.production.yml restart" -ForegroundColor White
Write-Host "  Backup: .\scripts\backup-database.ps1" -ForegroundColor White

Write-Host "`n⚠️ IMPORTANTE:" -ForegroundColor Yellow
Write-Host "- Configure firewall para portas 80, 443, 5432, 6379" -ForegroundColor White
Write-Host "- Configure backup automático: .\scripts\setup-backup-schedule.ps1" -ForegroundColor White
Write-Host "- Monitore logs: docker-compose -f docker-compose.production.yml logs -f" -ForegroundColor White
