# Script de Diagnóstico y Configuración de SQL Server
Write-Host "`n╔════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║  🔧 DIAGNÓSTICO Y CONFIGURACIÓN SQL SERVER               ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════════════════╝`n" -ForegroundColor Cyan

# 1. Verificar servicios
Write-Host "1️⃣ Servicios SQL Server:" -ForegroundColor Yellow
Get-Service -Name "*SQL*" | Where-Object {$_.Status -eq 'Running'} | Format-Table Name, DisplayName, Status

# 2. Verificar instancias
Write-Host "`n2️⃣ Instancias instaladas:" -ForegroundColor Yellow
$instances = Get-ItemProperty 'HKLM:\SOFTWARE\Microsoft\Microsoft SQL Server' -ErrorAction SilentlyContinue | 
    Select-Object -ExpandProperty InstalledInstances -ErrorAction SilentlyContinue
if($instances) {
    $instances | ForEach-Object { Write-Host "   • $_" -ForegroundColor Green }
} else {
    Write-Host "   ⚠️ No se encontraron instancias" -ForegroundColor Yellow
}

# 3. Probar conexiones
Write-Host "`n3️⃣ Probando conexiones:" -ForegroundColor Yellow
$servers = @("localhost", "(local)", ".", "localhost\MSSQLSERVER", ".\SQLEXPRESS")
foreach($server in $servers) {
    Write-Host "   Probando: $server" -ForegroundColor Gray -NoNewline
    $result = sqlcmd -S $server -Q "SELECT 1" -l 2 2>&1
    if($LASTEXITCODE -eq 0) {
        Write-Host " ✅ ÉXITO" -ForegroundColor Green
        Write-Host "   👉 Usar esta cadena: Server=$server;..." -ForegroundColor Cyan
        break
    } else {
        Write-Host " ❌ Falló" -ForegroundColor Red
    }
}

# 4. Soluciones sugeridas
Write-Host "`n4️⃣ SOLUCIONES SUGERIDAS:" -ForegroundColor Yellow
Write-Host "   A. Habilitar TCP/IP en SQL Server Configuration Manager" -ForegroundColor White
Write-Host "   B. Reiniciar servicio SQL Server" -ForegroundColor White
Write-Host "   C. Verificar firewall de Windows" -ForegroundColor White
Write-Host "   D. Usar SQL Server Management Studio para verificar conexión" -ForegroundColor White

Write-Host "`n5️⃣ COMANDOS ÚTILES:" -ForegroundColor Yellow
Write-Host "   # Reiniciar SQL Server:" -ForegroundColor Gray
Write-Host "   Restart-Service MSSQLSERVER" -ForegroundColor Cyan
Write-Host "`n   # Verificar puerto TCP:" -ForegroundColor Gray
Write-Host "   netstat -an | findstr 1433" -ForegroundColor Cyan

Write-Host "`n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━`n" -ForegroundColor Gray
