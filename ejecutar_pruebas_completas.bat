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

REM Crear carpetas temporales para las pruebas
if exist Pruebas\Temp_Test1 rmdir /s /q Pruebas\Temp_Test1
if exist Pruebas\Temp_Test2 rmdir /s /q Pruebas\Temp_Test2
mkdir Pruebas\Temp_Test1
mkdir Pruebas\Temp_Test2

REM Copiar archivos de prueba
copy Pruebas\factura_ingreso_test.xml Pruebas\Temp_Test1\ >nul
copy Pruebas\pago_test.xml Pruebas\Temp_Test2\ >nul

REM Cambiar al directorio del ejecutable
cd CFDIProcessor\bin\Debug\netcoreapp3.1

echo ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
echo.
echo 📝 PRUEBA 1: CFDI de Ingreso con Impuestos
echo ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
echo.
echo Carpeta: Pruebas\Temp_Test1
echo Tipo: Ingreso y Egreso ^(opción 2^)
echo.

echo 2>temp_test1.txt
echo %~dp0..\..\..\..\Pruebas\Temp_Test1>>temp_test1.txt
echo.>>temp_test1.txt
type temp_test1.txt | CFDIProcessor.exe
del temp_test1.txt

echo.
echo ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
echo.
echo 📝 PRUEBA 2: CFDI de Pagos 2.0
echo ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
echo.
echo Carpeta: Pruebas\Temp_Test2
echo Tipo: Pagos 2.0 ^(opción 3^)
echo.

echo 3>temp_test2.txt
echo %~dp0..\..\..\..\Pruebas\Temp_Test2>>temp_test2.txt
echo.>>temp_test2.txt
type temp_test2.txt | CFDIProcessor.exe
del temp_test2.txt

echo.
echo ╔════════════════════════════════════════════════════════════╗
echo ║  ✅ PRUEBAS COMPLETADAS                                   ║
echo ╚════════════════════════════════════════════════════════════╝
echo.
echo 💡 Revisa los resultados arriba para verificar que todo funcionó correctamente
echo.

cd ..\..\..\..

REM Limpiar carpetas temporales
rmdir /s /q Pruebas\Temp_Test1
rmdir /s /q Pruebas\Temp_Test2

pause
