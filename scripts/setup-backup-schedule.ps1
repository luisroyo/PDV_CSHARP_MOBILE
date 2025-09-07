# Script para configurar backup automático
# Sistema PDV Multi-Vertical

param(
    [string]$BackupPath = ".\backups",
    [int]$RetentionDays = 30,
    [string]$ScheduleTime = "02:00",
    [switch]$CreateTask = $true,
    [switch]$RemoveTask = $false
)

Write-Host "⏰ Configurando backup automático do Sistema PDV Multi-Vertical..." -ForegroundColor Green

# Verificar se está executando como administrador
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")
if (!$isAdmin) {
    Write-Host "⚠️ Execute como Administrador para configurar tarefas agendadas." -ForegroundColor Yellow
    Write-Host "💡 Alternativa: Execute manualmente o script de backup periodicamente." -ForegroundColor Cyan
}

if ($RemoveTask) {
    Write-Host "🗑️ Removendo tarefa de backup automático..." -ForegroundColor Yellow
    try {
        Unregister-ScheduledTask -TaskName "PDV_Backup_Automático" -Confirm:$false -ErrorAction SilentlyContinue
        Write-Host "✅ Tarefa removida com sucesso!" -ForegroundColor Green
    } catch {
        Write-Host "⚠️ Tarefa não encontrada ou erro ao remover: $_" -ForegroundColor Yellow
    }
    exit 0
}

# Criar diretório de backup se não existir
if (!(Test-Path $BackupPath)) {
    New-Item -ItemType Directory -Path $BackupPath -Force
    Write-Host "📁 Diretório de backup criado: $BackupPath" -ForegroundColor Green
}

# Criar script de backup wrapper
$BackupScriptPath = Join-Path $BackupPath "backup-automatico.ps1"
$BackupScriptContent = @"
# Script de backup automático gerado
# Sistema PDV Multi-Vertical

`$ErrorActionPreference = "Stop"

try {
    # Executar backup
    & ".\scripts\backup-database.ps1" -BackupPath "$BackupPath" -RetentionDays $RetentionDays -Compress
    
    # Log de sucesso
    `$LogEntry = "`$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss') - Backup automático executado com sucesso"
    Add-Content -Path ".\backups\backup.log" -Value `$LogEntry
    
} catch {
    # Log de erro
    `$LogEntry = "`$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss') - ERRO no backup automático: `$_"
    Add-Content -Path ".\backups\backup.log" -Value `$LogEntry
    throw
}
"@

Set-Content -Path $BackupScriptPath -Value $BackupScriptContent
Write-Host "📝 Script de backup automático criado: $BackupScriptPath" -ForegroundColor Green

if ($CreateTask -and $isAdmin) {
    try {
        # Remover tarefa existente se houver
        Unregister-ScheduledTask -TaskName "PDV_Backup_Automático" -Confirm:$false -ErrorAction SilentlyContinue

        # Criar nova tarefa agendada
        $Action = New-ScheduledTaskAction -Execute "PowerShell.exe" -Argument "-ExecutionPolicy Bypass -File `"$BackupScriptPath`""
        $Trigger = New-ScheduledTaskTrigger -Daily -At $ScheduleTime
        $Settings = New-ScheduledTaskSettingsSet -AllowStartIfOnBatteries -DontStopIfGoingOnBatteries -StartWhenAvailable
        $Principal = New-ScheduledTaskPrincipal -UserId "SYSTEM" -LogonType ServiceAccount -RunLevel Highest

        Register-ScheduledTask -TaskName "PDV_Backup_Automático" -Action $Action -Trigger $Trigger -Settings $Settings -Principal $Principal -Description "Backup automático diário do Sistema PDV Multi-Vertical"

        Write-Host "✅ Tarefa agendada criada com sucesso!" -ForegroundColor Green
        Write-Host "⏰ Horário: Diariamente às $ScheduleTime" -ForegroundColor Cyan
        Write-Host "📁 Backup em: $BackupPath" -ForegroundColor Cyan
        Write-Host "🗑️ Retenção: $RetentionDays dias" -ForegroundColor Cyan

    } catch {
        Write-Host "❌ Erro ao criar tarefa agendada: $_" -ForegroundColor Red
        Write-Host "💡 Execute manualmente: .\scripts\backup-database.ps1" -ForegroundColor Yellow
    }
} elseif ($CreateTask -and !$isAdmin) {
    Write-Host "⚠️ Não foi possível criar tarefa agendada (não é administrador)" -ForegroundColor Yellow
    Write-Host "💡 Execute manualmente: .\scripts\backup-database.ps1" -ForegroundColor Cyan
}

# Criar script de monitoramento
$MonitorScriptPath = Join-Path $BackupPath "monitor-backup.ps1"
$MonitorScriptContent = @"
# Script de monitoramento de backup
# Sistema PDV Multi-Vertical

`$BackupPath = "$BackupPath"
`$LogFile = Join-Path `$BackupPath "backup.log"

Write-Host "📊 Monitoramento de Backup - Sistema PDV Multi-Vertical" -ForegroundColor Green
Write-Host "=" * 50

# Verificar último backup
`$LastBackup = Get-ChildItem -Path `$BackupPath -Filter "pos_multivertical_backup_*" | Sort-Object LastWriteTime -Descending | Select-Object -First 1

if (`$LastBackup) {
    `$Age = (Get-Date) - `$LastBackup.LastWriteTime
    `$AgeHours = [math]::Round(`$Age.TotalHours, 1)
    
    Write-Host "📁 Último backup: `$(`$LastBackup.Name)" -ForegroundColor Cyan
    Write-Host "⏰ Data/Hora: `$(`$LastBackup.LastWriteTime)" -ForegroundColor Cyan
    Write-Host "🕐 Idade: `$AgeHours horas" -ForegroundColor Cyan
    Write-Host "📏 Tamanho: `$([math]::Round(`$LastBackup.Length / 1MB, 2)) MB" -ForegroundColor Cyan
    
    if (`$AgeHours -gt 25) {
        Write-Host "⚠️ ATENÇÃO: Backup com mais de 25 horas!" -ForegroundColor Yellow
    } else {
        Write-Host "✅ Backup recente" -ForegroundColor Green
    }
} else {
    Write-Host "❌ Nenhum backup encontrado!" -ForegroundColor Red
}

# Verificar logs
if (Test-Path `$LogFile) {
    Write-Host "`n📝 Últimas 5 entradas do log:" -ForegroundColor Yellow
    Get-Content `$LogFile | Select-Object -Last 5 | ForEach-Object { Write-Host "  `$_" -ForegroundColor Gray }
} else {
    Write-Host "`n📝 Nenhum log encontrado" -ForegroundColor Yellow
}

# Estatísticas
`$TotalBackups = (Get-ChildItem -Path `$BackupPath -Filter "pos_multivertical_backup_*").Count
`$TotalSize = (Get-ChildItem -Path `$BackupPath -Filter "pos_multivertical_backup_*" | Measure-Object -Property Length -Sum).Sum
`$TotalSizeMB = [math]::Round(`$TotalSize / 1MB, 2)

Write-Host "`n📊 Estatísticas:" -ForegroundColor Yellow
Write-Host "  Total de backups: `$TotalBackups" -ForegroundColor White
Write-Host "  Tamanho total: `$TotalSizeMB MB" -ForegroundColor White
Write-Host "  Diretório: `$BackupPath" -ForegroundColor White
"@

Set-Content -Path $MonitorScriptPath -Value $MonitorScriptContent
Write-Host "📊 Script de monitoramento criado: $MonitorScriptPath" -ForegroundColor Green

Write-Host "`n🎉 Configuração de backup automático concluída!" -ForegroundColor Green
Write-Host "`n📋 Comandos úteis:" -ForegroundColor Yellow
Write-Host "  Backup manual: .\scripts\backup-database.ps1" -ForegroundColor White
Write-Host "  Restaurar: .\scripts\restore-database.ps1 -BackupFile 'arquivo.sql'" -ForegroundColor White
Write-Host "  Monitorar: .\backups\monitor-backup.ps1" -ForegroundColor White
Write-Host "  Remover tarefa: .\scripts\setup-backup-schedule.ps1 -RemoveTask" -ForegroundColor White
