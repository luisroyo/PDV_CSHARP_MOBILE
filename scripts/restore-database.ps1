# Script de Restauração do PostgreSQL
# Sistema PDV Multi-Vertical

param(
    [Parameter(Mandatory=$true)]
    [string]$BackupFile,
    [string]$DatabaseName = "pos_multivertical",
    [string]$Host = "localhost",
    [int]$Port = 5432,
    [string]$Username = "postgres",
    [string]$Password = "postgres",
    [switch]$Force = $false
)

Write-Host "🔄 Iniciando restauração do banco de dados..." -ForegroundColor Green
Write-Host "📁 Arquivo: $BackupFile" -ForegroundColor Cyan
Write-Host "📊 Banco: $DatabaseName" -ForegroundColor Cyan

# Verificar se o arquivo de backup existe
if (!(Test-Path $BackupFile)) {
    Write-Host "❌ Arquivo de backup não encontrado: $BackupFile" -ForegroundColor Red
    exit 1
}

# Verificar se pg_restore está disponível
$pgRestorePath = Get-Command pg_restore -ErrorAction SilentlyContinue
if (!$pgRestorePath) {
    Write-Host "❌ pg_restore não encontrado. Instale o PostgreSQL client." -ForegroundColor Red
    exit 1
}

# Verificar se psql está disponível
$psqlPath = Get-Command psql -ErrorAction SilentlyContinue
if (!$psqlPath) {
    Write-Host "❌ psql não encontrado. Instale o PostgreSQL client." -ForegroundColor Red
    exit 1
}

try {
    $env:PGPASSWORD = $Password

    # Confirmar restauração se não for forçada
    if (!$Force) {
        Write-Host "⚠️ ATENÇÃO: Esta operação irá substituir o banco de dados '$DatabaseName'!" -ForegroundColor Yellow
        $confirmation = Read-Host "Deseja continuar? (s/N)"
        if ($confirmation -ne "s" -and $confirmation -ne "S") {
            Write-Host "❌ Operação cancelada pelo usuário." -ForegroundColor Red
            exit 0
        }
    }

    # Verificar se o banco existe
    Write-Host "🔍 Verificando se o banco existe..." -ForegroundColor Yellow
    $dbExists = psql -h $Host -p $Port -U $Username -d postgres -t -c "SELECT 1 FROM pg_database WHERE datname='$DatabaseName';"
    
    if ($dbExists.Trim() -eq "1") {
        Write-Host "⚠️ Banco '$DatabaseName' já existe. Será recriado." -ForegroundColor Yellow
        
        # Dropar o banco existente
        Write-Host "🗑️ Removendo banco existente..." -ForegroundColor Yellow
        psql -h $Host -p $Port -U $Username -d postgres -c "DROP DATABASE IF EXISTS $DatabaseName;"
    }

    # Criar novo banco
    Write-Host "🏗️ Criando novo banco de dados..." -ForegroundColor Yellow
    psql -h $Host -p $Port -U $Username -d postgres -c "CREATE DATABASE $DatabaseName;"

    # Determinar se é arquivo SQL ou dump customizado
    $fileExtension = [System.IO.Path]::GetExtension($BackupFile).ToLower()
    
    if ($fileExtension -eq ".sql") {
        # Restaurar arquivo SQL
        Write-Host "📥 Restaurando arquivo SQL..." -ForegroundColor Yellow
        psql -h $Host -p $Port -U $Username -d $DatabaseName -f $BackupFile
    } else {
        # Restaurar dump customizado
        Write-Host "📥 Restaurando dump customizado..." -ForegroundColor Yellow
        pg_restore -h $Host -p $Port -U $Username -d $DatabaseName -v $BackupFile
    }

    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Restauração concluída com sucesso!" -ForegroundColor Green
        
        # Verificar integridade
        Write-Host "🔍 Verificando integridade do banco..." -ForegroundColor Yellow
        $tableCount = psql -h $Host -p $Port -U $Username -d $DatabaseName -t -c "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'public';"
        Write-Host "📊 Tabelas restauradas: $($tableCount.Trim())" -ForegroundColor Cyan

        # Log da restauração
        $LogEntry = "$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss') - Restore: $BackupFile -> $DatabaseName"
        $LogFile = ".\backups\restore.log"
        Add-Content -Path $LogFile -Value $LogEntry

        Write-Host "🎉 Restauração finalizada com sucesso!" -ForegroundColor Green
        Write-Host "📊 Banco: $DatabaseName" -ForegroundColor Cyan
        Write-Host "📝 Log: $LogFile" -ForegroundColor Cyan

    } else {
        Write-Host "❌ Erro durante a restauração. Código de saída: $LASTEXITCODE" -ForegroundColor Red
        exit 1
    }

} catch {
    Write-Host "❌ Erro durante a restauração: $_" -ForegroundColor Red
    exit 1
} finally {
    # Limpar variável de ambiente
    Remove-Item Env:PGPASSWORD -ErrorAction SilentlyContinue
}

Write-Host "`n📋 Próximos passos:" -ForegroundColor Yellow
Write-Host "1. Verificar o banco: psql -h $Host -p $Port -U $Username -d $DatabaseName" -ForegroundColor White
Write-Host "2. Testar a API: http://localhost:7001/health" -ForegroundColor White
Write-Host "3. Verificar logs: docker-compose logs -f api" -ForegroundColor White
