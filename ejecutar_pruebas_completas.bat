@echo off
chcp 65001 >nul
echo.
echo ╔════════════════════════════════════════════════════════════╗
echo ║  🧪 EJECUTANDO PRUEBAS COMPLETAS                          ║
echo ╚════════════════════════════════════════════════════════════╝
echo.

REM Verificar que SQL Server esté corriendo
sc query MSSQLSERVER | find "RUNNING" >nul
if %errorLevel% neq 0 (
    echo ❌ SQL Server no está corriendo
    echo.
    echo 👉 Ejecuta primero: iniciar_sql_server.bat ^(como administrador^)
    echo.
    pause
    exit /b 1
)

echo ✅ SQL Server está corriendo
echo.

REM Cambiar al directorio del ejecutable
cd CFDIProcessor\bin\Debug\netcoreapp3.1

echo ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
echo.
echo 📝 PRUEBA 1: CFDI de Ingreso con Impuestos
echo ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
echo.
echo Archivo: Pruebas\factura_ingreso_test.xml
echo Tipo: Ingreso y Egreso ^(opción 2^)
echo.

CFDIProcessor.exe 2 ..\..\..\..\Pruebas\factura_ingreso_test.xml

echo.
echo ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
echo.
echo 📝 PRUEBA 2: CFDI de Pagos 2.0
echo ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
echo.
echo Archivo: Pruebas\pago_test.xml
echo Tipo: Pagos 2.0 ^(opción 3^)
echo.

CFDIProcessor.exe 3 ..\..\..\..\Pruebas\pago_test.xml

echo.
echo ╔════════════════════════════════════════════════════════════╗
echo ║  ✅ PRUEBAS COMPLETADAS                                   ║
echo ╚════════════════════════════════════════════════════════════╝
echo.
echo 💡 Revisa los resultados arriba para verificar que todo funcionó correctamente
echo.

cd ..\..\..\..

pause
