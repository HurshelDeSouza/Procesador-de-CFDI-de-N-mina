@echo off
echo ========================================
echo   PRUEBA CON ARCHIVOS DE EJEMPLO
echo ========================================
echo.
echo Este script procesara los archivos XML de ejemplo
echo que se encuentran en la carpeta XMLs_Ejemplo
echo.
echo Archivos que se procesaran:
echo   - nomina_ejemplo_001.xml (Juan Perez - Vendedor)
echo   - nomina_ejemplo_002.xml (Maria Lopez - Contador)
echo   - nomina_ejemplo_003.xml (Carlos Rodriguez - Desarrollador)
echo   - factura_ejemplo_NO_NOMINA.xml (sera ignorado)
echo.
pause
echo.
echo Ejecutando aplicacion...
echo.
cd CFDIProcessor
echo %~dp0XMLs_Ejemplo | dotnet run
echo.
echo ========================================
echo   PROCESO COMPLETADO
echo ========================================
echo.
echo Para verificar los datos procesados:
echo.
echo 1. Abre SQL Server Management Studio
echo 2. Conecta a: localhost
echo 3. Usa la base de datos: DescargaCfdiGFP
echo 4. Ejecuta: SELECT * FROM CFDI_Comprobante
echo.
echo O ejecuta las consultas en: CONSULTAS_SQL_UTILES.sql
echo.
pause
