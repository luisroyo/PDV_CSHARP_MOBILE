# Script de Backup Automático do PostgreSQL
# Sistema PDV Multi-Vertical

param(
    [string]$BackupPath = ".\backups",
    [int]$RetentionDays = 30,
    [switch]$Compress = $true,
    [switch]$UploadToCloud = $false
)

# Configurações
$DatabaseName = "pos_multivertical"
$Host = "localhost"
$Port = "5432"
$Username = "postgres"
$Password = "postgres"

# Criar diretório de backup se não existir
if (!(Test-Path $BackupPath)) {
    New-Item -ItemType Directory -Path $BackupPath -Force
    Write-Host "📁 Diretório de backup criado: $BackupPath" -ForegroundColor Green
}

# Gerar nome do arquivo de backup
$Timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$BackupFileName = "pos_multivertical_backup_$Timestamp.sql"
$BackupFilePath = Join-Path $BackupPath $BackupFileName

Write-Host "🚀 Iniciando backup do banco de dados..." -ForegroundColor Green
Write-Host "📊 Banco: $DatabaseName" -ForegroundColor Cyan
Write-Host "📁 Destino: $BackupFilePath" -ForegroundColor Cyan

try {
    # Verificar se pg_dump está disponível
    $pgDumpPath = Get-Command pg_dump -ErrorAction SilentlyContinue
    if (!$pgDumpPath) {
        Write-Host "❌ pg_dump não encontrado. Instale o PostgreSQL client." -ForegroundColor Red
        exit 1
    }

    # Executar backup
    $env:PGPASSWORD = $Password
    $pgDumpArgs = @(
        "--host=$Host",
        "--port=$Port",
        "--username=$Username",
        "--dbname=$DatabaseName",
        "--verbose",
        "--clean",
        "--create",
        "--if-exists",
        "--file=$BackupFilePath"
    )

    Write-Host "⏳ Executando pg_dump..." -ForegroundColor Yellow
    & pg_dump @pgDumpArgs

    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Backup concluído com sucesso!" -ForegroundColor Green
        
        # Comprimir backup se solicitado
        if ($Compress) {
            $CompressedFile = "$BackupFilePath.zip"
            Write-Host "🗜️ Comprimindo backup..." -ForegroundColor Yellow
            
            Compress-Archive -Path $BackupFilePath -DestinationPath $CompressedFile -Force
            Remove-Item $BackupFilePath
            
            $BackupFilePath = $CompressedFile
            Write-Host "✅ Backup comprimido: $BackupFilePath" -ForegroundColor Green
        }

        # Obter tamanho do arquivo
        $FileSize = (Get-Item $BackupFilePath).Length
        $FileSizeMB = [math]::Round($FileSize / 1MB, 2)
        Write-Host "📏 Tamanho do backup: $FileSizeMB MB" -ForegroundColor Cyan

        # Limpar backups antigos
        Write-Host "🧹 Limpando backups antigos (mais de $RetentionDays dias)..." -ForegroundColor Yellow
        $CutoffDate = (Get-Date).AddDays(-$RetentionDays)
        
        Get-ChildItem -Path $BackupPath -Filter "pos_multivertical_backup_*" | 
            Where-Object { $_.LastWriteTime -lt $CutoffDate } | 
            ForEach-Object {
                Write-Host "🗑️ Removendo: $($_.Name)" -ForegroundColor Gray
                Remove-Item $_.FullName -Force
            }

        # Upload para cloud se solicitado
        if ($UploadToCloud) {
            Write-Host "☁️ Upload para cloud não implementado ainda." -ForegroundColor Yellow
        }

        # Log do backup
        $LogEntry = "$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss') - Backup: $BackupFileName - Size: $FileSizeMB MB"
        $LogFile = Join-Path $BackupPath "backup.log"
        Add-Content -Path $LogFile -Value $LogEntry

        Write-Host "🎉 Backup finalizado com sucesso!" -ForegroundColor Green
        Write-Host "📁 Arquivo: $BackupFilePath" -ForegroundColor Cyan
        Write-Host "📊 Tamanho: $FileSizeMB MB" -ForegroundColor Cyan
        Write-Host "📝 Log: $LogFile" -ForegroundColor Cyan

    } else {
        Write-Host "❌ Erro durante o backup. Código de saída: $LASTEXITCODE" -ForegroundColor Red
        exit 1
    }

} catch {
    Write-Host "❌ Erro durante o backup: $_" -ForegroundColor Red
    exit 1
} finally {
    # Limpar variável de ambiente
    Remove-Item Env:PGPASSWORD -ErrorAction SilentlyContinue
}

Write-Host "`n📋 Próximos passos:" -ForegroundColor Yellow
Write-Host "1. Verificar o arquivo de backup: $BackupFilePath" -ForegroundColor White
Write-Host "2. Testar restauração: .\scripts\restore-database.ps1 -BackupFile '$BackupFilePath'" -ForegroundColor White
Write-Host "3. Configurar backup automático: .\scripts\setup-backup-schedule.ps1" -ForegroundColor White
