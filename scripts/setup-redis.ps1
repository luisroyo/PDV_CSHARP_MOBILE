# Script para configurar Redis no Windows
# Execute como Administrador

Write-Host "🔧 Configurando Redis para o Sistema PDV Multi-Vertical..." -ForegroundColor Green

# Verificar se o Docker está instalado
try {
    $dockerVersion = docker --version
    Write-Host "✅ Docker encontrado: $dockerVersion" -ForegroundColor Green
} catch {
    Write-Host "❌ Docker não encontrado. Instale o Docker Desktop primeiro." -ForegroundColor Red
    exit 1
}

# Verificar se o Docker Compose está disponível
try {
    $composeVersion = docker-compose --version
    Write-Host "✅ Docker Compose encontrado: $composeVersion" -ForegroundColor Green
} catch {
    Write-Host "❌ Docker Compose não encontrado. Instale o Docker Desktop primeiro." -ForegroundColor Red
    exit 1
}

# Parar containers existentes
Write-Host "🛑 Parando containers existentes..." -ForegroundColor Yellow
docker-compose down

# Iniciar Redis
Write-Host "🚀 Iniciando Redis..." -ForegroundColor Green
docker-compose up -d redis

# Aguardar Redis inicializar
Write-Host "⏳ Aguardando Redis inicializar..." -ForegroundColor Yellow
Start-Sleep -Seconds 10

# Verificar se Redis está funcionando
Write-Host "🔍 Verificando conexão com Redis..." -ForegroundColor Green
try {
    $redisTest = docker exec pos_redis redis-cli ping
    if ($redisTest -eq "PONG") {
        Write-Host "✅ Redis está funcionando corretamente!" -ForegroundColor Green
    } else {
        Write-Host "❌ Redis não respondeu corretamente" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "❌ Erro ao conectar com Redis: $_" -ForegroundColor Red
    exit 1
}

# Configurar Redis para persistência
Write-Host "⚙️ Configurando Redis para persistência..." -ForegroundColor Green
docker exec pos_redis redis-cli CONFIG SET save "900 1 300 10 60 10000"

# Mostrar informações do Redis
Write-Host "📊 Informações do Redis:" -ForegroundColor Cyan
docker exec pos_redis redis-cli INFO server | Select-String "redis_version|uptime_in_seconds|connected_clients"

Write-Host "🎉 Redis configurado com sucesso!" -ForegroundColor Green
Write-Host "📍 Redis disponível em: localhost:6379" -ForegroundColor Cyan
Write-Host "🔗 Para conectar: redis-cli -h localhost -p 6379" -ForegroundColor Cyan

Write-Host "`n📋 Próximos passos:" -ForegroundColor Yellow
Write-Host "1. Execute: docker-compose up -d (para iniciar todos os serviços)" -ForegroundColor White
Write-Host "2. Execute: docker-compose logs -f api (para ver logs da API)" -ForegroundColor White
Write-Host "3. Acesse: http://localhost:7001 (API com cache Redis)" -ForegroundColor White
