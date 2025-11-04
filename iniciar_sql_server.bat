@echo off
echo.
echo ╔════════════════════════════════════════════════════════════╗
echo ║  🚀 INICIANDO SQL SERVER                                  ║
echo ╚════════════════════════════════════════════════════════════╝
echo.

REM Verificar permisos de administrador
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo ❌ Este script requiere permisos de administrador
    echo.
    echo 👉 Haz clic derecho en este archivo y selecciona "Ejecutar como administrador"
    echo.
    pause
    exit /b 1
)

echo ✅ Permisos de administrador verificados
echo.
echo 🔧 Iniciando servicio MSSQLSERVER...
net start MSSQLSERVER

if %errorLevel% equ 0 (
    echo.
    echo ✅ SQL Server iniciado correctamente
    echo.
    echo 🧪 Probando conexión...
    sqlcmd -S localhost -Q "SELECT @@VERSION" -l 5
    
    if %errorLevel% equ 0 (
        echo.
        echo ✅ Conexión exitosa - Listo para ejecutar pruebas
    ) else (
        echo.
        echo ⚠️ Servicio iniciado pero no responde - Espera 10 segundos
    )
) else (
    echo.
    echo ❌ Error al iniciar SQL Server
    echo.
    echo 💡 Posibles soluciones:
    echo    1. Verificar que SQL Server esté instalado correctamente
    echo    2. Revisar el Event Viewer para errores
    echo    3. Ejecutar SQL Server Configuration Manager
)

echo.
pause
