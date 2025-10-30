# Script para configurar la base de datos automáticamente

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Configurador de Base de Datos CFDI" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Verificar si SQL Server está ejecutándose
Write-Host "1. Verificando SQL Server..." -ForegroundColor Yellow
$sqlService = Get-Service -Name MSSQLSERVER -ErrorAction SilentlyContinue

if ($null -eq $sqlService) {
    Write-Host "   [ERROR] SQL Server no está instalado o no se encuentra." -ForegroundColor Red
    Write-Host "   Buscando SQL Server Express..." -ForegroundColor Yellow
    $sqlService = Get-Service -Name "MSSQL`$SQLEXPRESS" -ErrorAction SilentlyContinue
    if ($null -eq $sqlService) {
        Write-Host "   [ERROR] No se encontró ninguna instancia de SQL Server." -ForegroundColor Red
        exit 1
    }
    $serverName = "localhost\SQLEXPRESS"
} else {
    $serverName = "localhost"
}

if ($sqlService.Status -ne "Running") {
    Write-Host "   [AVISO] SQL Server no está ejecutándose. Intentando iniciar..." -ForegroundColor Yellow
    Start-Service $sqlService.Name
    Start-Sleep -Seconds 3
    $sqlService.Refresh()
    if ($sqlService.Status -eq "Running") {
        Write-Host "   [OK] SQL Server iniciado correctamente." -ForegroundColor Green
    } else {
        Write-Host "   [ERROR] No se pudo iniciar SQL Server." -ForegroundColor Red
        exit 1
    }
} else {
    Write-Host "   [OK] SQL Server está ejecutándose." -ForegroundColor Green
}

Write-Host "   Servidor: $serverName" -ForegroundColor Cyan
Write-Host ""

# Probar conexión
Write-Host "2. Probando conexión..." -ForegroundColor Yellow
$testQuery = "SELECT @@VERSION"
$result = sqlcmd -S $serverName -E -Q $testQuery -h -1 2>&1

if ($LASTEXITCODE -eq 0) {
    Write-Host "   [OK] Conexión exitosa con Autenticación de Windows." -ForegroundColor Green
} else {
    Write-Host "   [ERROR] No se pudo conectar a SQL Server." -ForegroundColor Red
    Write-Host "   Error: $result" -ForegroundColor Red
    Write-Host ""
    Write-Host "   Posibles soluciones:" -ForegroundColor Yellow
    Write-Host "   1. Verifica que tu usuario de Windows tenga permisos en SQL Server" -ForegroundColor White
    Write-Host "   2. O configura autenticación SQL Server en CfdiDbContext.cs" -ForegroundColor White
    exit 1
}
Write-Host ""

# Verificar si existe la base de datos
Write-Host "3. Verificando base de datos..." -ForegroundColor Yellow
$checkDbQuery = "SELECT name FROM sys.databases WHERE name = 'DescargaCfdiGFP'"
$dbExists = sqlcmd -S $serverName -E -Q $checkDbQuery -h -1 2>&1 | Select-String "DescargaCfdiGFP"

if ($dbExists) {
    Write-Host "   [OK] La base de datos 'DescargaCfdiGFP' ya existe." -ForegroundColor Green
    Write-Host ""
    $respuesta = Read-Host "   ¿Deseas recrearla? Esto eliminará todos los datos (S/N)"
    if ($respuesta -eq "S" -or $respuesta -eq "s") {
        Write-Host "   Eliminando base de datos existente..." -ForegroundColor Yellow
        sqlcmd -S $serverName -E -Q "DROP DATABASE DescargaCfdiGFP" 2>&1 | Out-Null
        $dbExists = $false
    } else {
        Write-Host "   Manteniendo base de datos existente." -ForegroundColor Cyan
        Write-Host ""
        Write-Host "========================================" -ForegroundColor Green
        Write-Host "  Configuración completada" -ForegroundColor Green
        Write-Host "========================================" -ForegroundColor Green
        Write-Host ""
        Write-Host "La aplicación está lista para usar." -ForegroundColor White
        Write-Host "Ejecuta: cd CFDIProcessor && dotnet run" -ForegroundColor Cyan
        Write-Host ""
        pause
        exit 0
    }
}

if (-not $dbExists) {
    Write-Host "   [AVISO] La base de datos no existe. Creándola..." -ForegroundColor Yellow
    
    if (-not (Test-Path "db_cfdi.sql")) {
        Write-Host "   [ERROR] No se encontró el archivo db_cfdi.sql" -ForegroundColor Red
        exit 1
    }
    
    Write-Host "   Ejecutando script SQL..." -ForegroundColor Yellow
    $output = sqlcmd -S $serverName -E -i "db_cfdi.sql" 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "   [OK] Base de datos creada exitosamente." -ForegroundColor Green
    } else {
        Write-Host "   [ERROR] Hubo un problema al crear la base de datos." -ForegroundColor Red
        Write-Host "   $output" -ForegroundColor Red
        exit 1
    }
}
Write-Host ""

# Verificar tablas creadas
Write-Host "4. Verificando tablas..." -ForegroundColor Yellow
$checkTablesQuery = "USE DescargaCfdiGFP; SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'"
$tableCount = sqlcmd -S $serverName -E -Q $checkTablesQuery -h -1 2>&1 | Select-String "\d+"

if ($tableCount) {
    $count = $tableCount.Matches[0].Value.Trim()
    Write-Host "   [OK] Se encontraron $count tablas en la base de datos." -ForegroundColor Green
} else {
    Write-Host "   [AVISO] No se pudieron verificar las tablas." -ForegroundColor Yellow
}
Write-Host ""

# Actualizar cadena de conexión si es necesario
if ($serverName -ne "localhost") {
    Write-Host "5. Actualizando cadena de conexión..." -ForegroundColor Yellow
    $dbContextPath = "CFDIProcessor\Data\CfdiDbContext.cs"
    
    if (Test-Path $dbContextPath) {
        $content = Get-Content $dbContextPath -Raw
        $content = $content -replace "Server=localhost;", "Server=$serverName;"
        Set-Content $dbContextPath -Value $content
        Write-Host "   [OK] Cadena de conexión actualizada a: $serverName" -ForegroundColor Green
    }
    Write-Host ""
}

# Resumen final
Write-Host "========================================" -ForegroundColor Green
Write-Host "  Configuración completada exitosamente" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Resumen:" -ForegroundColor Cyan
Write-Host "  - Servidor: $serverName" -ForegroundColor White
Write-Host "  - Base de datos: DescargaCfdiGFP" -ForegroundColor White
Write-Host "  - Autenticación: Windows (Integrated Security)" -ForegroundColor White
Write-Host ""
Write-Host "Para ejecutar la aplicación:" -ForegroundColor Cyan
Write-Host "  cd CFDIProcessor" -ForegroundColor White
Write-Host "  dotnet run" -ForegroundColor White
Write-Host ""
Write-Host "O simplemente ejecuta: ejecutar.bat" -ForegroundColor Cyan
Write-Host ""
pause
