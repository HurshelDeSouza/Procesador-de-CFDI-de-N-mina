@echo off
echo ========================================
echo   EJEMPLO DE USO
echo   Procesador de CFDI de Nomina
echo ========================================
echo.
echo Este script te muestra como usar la aplicacion.
echo.
echo PASO 1: Preparar tus archivos XML
echo --------------------------------
echo.
echo Coloca todos tus archivos XML de nomina en una carpeta.
echo Ejemplo: C:\MisNominas\
echo.
echo PASO 2: Ejecutar la aplicacion
echo --------------------------------
echo.
echo Opcion A: Doble clic en ejecutar.bat
echo Opcion B: Desde terminal: cd CFDIProcessor ^&^& dotnet run
echo.
echo PASO 3: Ingresar la ruta
echo --------------------------------
echo.
echo Cuando te pida la ruta, escribe la carpeta donde estan tus XMLs
echo Ejemplo: C:\MisNominas
echo.
echo TIP: Puedes arrastrar la carpeta a la ventana de consola
echo      y se pegara la ruta automaticamente!
echo.
echo PASO 4: Ver el proceso
echo --------------------------------
echo.
echo La aplicacion mostrara:
echo - Cuantos archivos encontro
echo - Cual esta procesando
echo - Si hubo errores
echo - Cuando termino
echo.
echo ========================================
echo.
echo Deseas ejecutar la aplicacion ahora? (S/N)
set /p respuesta=
if /i "%respuesta%"=="S" (
    echo.
    echo Ejecutando aplicacion...
    echo.
    cd CFDIProcessor
    dotnet run
) else (
    echo.
    echo Puedes ejecutarla cuando quieras con: ejecutar.bat
)
echo.
pause
