# 🧪 Instrucciones para Ejecutar Pruebas Reales

## ⚠️ Problema Detectado

El servicio **SQL Server (MSSQLSERVER)** está detenido y necesita iniciarse con permisos de administrador.

## 🔧 Solución Rápida

### Opción 1: Usar el script automático (RECOMENDADO)

1. **Haz clic derecho** en `iniciar_sql_server.bat`
2. Selecciona **"Ejecutar como administrador"**
3. Espera a que el servicio inicie
4. El script probará la conexión automáticamente

### Opción 2: Iniciar manualmente

1. Presiona `Win + R`
2. Escribe: `services.msc`
3. Busca **"SQL Server (MSSQLSERVER)"**
4. Clic derecho → **Iniciar**

### Opción 3: Desde PowerShell como Administrador

```powershell
Start-Service MSSQLSERVER
```

---

## 🧪 Ejecutar Pruebas Después de Iniciar SQL Server

Una vez que SQL Server esté corriendo, ejecuta:

```cmd
ejecutar_pruebas_completas.bat
```

O manualmente:

### Prueba 1: CFDI de Ingreso
```cmd
cd CFDIProcessor\bin\Debug\netcoreapp3.1
CFDIProcessor.exe 1 ..\..\..\..\Pruebas\factura_ingreso_test.xml
```

### Prueba 2: CFDI de Pagos 2.0
```cmd
cd CFDIProcessor\bin\Debug\netcoreapp3.1
CFDIProcessor.exe 3 ..\..\..\..\Pruebas\pago_test.xml
```

---

## 📊 Estado Actual

✅ **Compilación:** Exitosa  
✅ **Archivos de prueba:** Creados  
✅ **Código:** Verificado  
⚠️ **SQL Server:** Detenido (requiere inicio)  
⏳ **Pruebas reales:** Pendientes  

---

## 🔍 Verificar Estado de SQL Server

```powershell
Get-Service MSSQLSERVER
```

Debe mostrar: **Status: Running**

---

## 💡 Notas

- SQL Server debe estar corriendo antes de ejecutar las pruebas
- La base de datos `DescargaCfdiGFP` debe existir (ejecutar `facturas.sql` si no existe)
- Las pruebas insertarán datos reales en la base de datos
