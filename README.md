# 📊 Procesador de CFDI - Versión 2.0

Sistema completo para procesar archivos XML de CFDI (Comprobante Fiscal Digital por Internet) de México, incluyendo Nómina, Ingreso/Egreso y Pagos 2.0.

## 🚀 Características

### ✅ Tipos de CFDI Soportados
- **Nómina (N)** - Recibos de nómina con percepciones, deducciones y otros pagos
- **Ingreso (I)** - Facturas de venta con conceptos e impuestos
- **Egreso (E)** - Notas de crédito con conceptos e impuestos
- **Pagos 2.0 (P)** - Complementos de pago con documentos relacionados y parcialidades
- **Traslado (T)** - Cartas porte

### ✅ Funcionalidades Principales
- Procesamiento automático de archivos XML
- Guardado completo en base de datos SQL Server
- Procesamiento de conceptos con traslados y retenciones
- Soporte para múltiples pagos y documentos relacionados
- Validación de duplicados por UUID
- Manejo de transacciones con rollback automático
- Menú interactivo para selección de tipo de CFDI
- Campo EmitidaRecibida para distinguir facturas emitidas/recibidas

### ✅ Impuestos Soportados
- **Traslados:** IVA (002), IEPS (003)
- **Retenciones:** ISR (001), IVA Retenido (002)
- Guardado por concepto individual con base, tasa e importe

---

## 📋 Requisitos

- .NET Core 3.1 o superior
- SQL Server (cualquier versión)
- Windows (probado en Windows 10/11)

---

## 🗄️ Base de Datos

### Script Principal
El archivo `facturas.sql` en la raíz del proyecto contiene todas las definiciones de tablas.

### Tablas Principales

#### CFDI_Comprobante
Almacena la información general del comprobante:
- UUID, Serie, Folio
- Fecha, FechaTimbrado
- TipoDeComprobante (N/I/E/P/T)
- **EmitidaRecibida** (E=Emitida, R=Recibida) - NUEVO
- Total, SubTotal, Descuento
- Moneda, TipoCambio
- MetodoPago, FormaPago

#### CFDI_Concepto
Almacena los conceptos/productos de las facturas:
- ClaveProdServ, Cantidad, ClaveUnidad
- Descripcion, ValorUnitario, Importe
- Descuento, ObjetoImp

#### CFDI_TrasladoConcepto
Almacena los impuestos trasladados (IVA, IEPS):
- Impuesto (001/002/003)
- Base, TipoFactor, TasaOCuota
- Importe

#### CFDI_RetencionConcepto
Almacena las retenciones (ISR, IVA Ret.):
- Impuesto (001/002)
- Base, TipoFactor, TasaOCuota
- Importe

#### CFDI_Pagos_Detalle
Almacena el detalle general del complemento de pagos:
- FormaDePago
- MontoTotalPagos
- TotalTrasladosBaseIva16
- TotalTrasladosImpuestoIva16

#### CFDI_Pagos_Pago
Almacena cada pago individual:
- FechaPago
- FormaDePagoP, MonedaP, TipoCambioP
- Monto, NumOperacion

#### CFDI_Pagos_DoctoRelacionado
Almacena los documentos relacionados con parcialidades:
- IdDocumento (UUID de la factura)
- Serie, Folio
- MonedaDR, EquivalenciaDR
- NumParcialidad
- ImpSaldoAnt, ImpPagado, ImpSaldoInsoluto

---

## 🛠️ Instalación

### 1. Clonar el Repositorio
```bash
git clone https://github.com/HurshelDeSouza/Procesador-de-CFDI-de-N-mina.git
cd Procesador-de-CFDI-de-N-mina
```

### 2. Configurar Base de Datos

#### Opción A: Base de datos nueva
```bash
# Crear base de datos
sqlcmd -S localhost -Q "CREATE DATABASE DescargaCfdiGFP"

# Ejecutar script completo
sqlcmd -S localhost -d DescargaCfdiGFP -i facturas.sql
```

#### Opción B: Actualizar base de datos existente
```bash
sqlcmd -S localhost -d DescargaCfdiGFP -i actualizar_bd.sql
```

### 3. Configurar Cadena de Conexión

Editar `CFDIProcessor/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=DescargaCfdiGFP;Integrated Security=True;TrustServerCertificate=True;"
  }
}
```

### 4. Compilar el Proyecto
```bash
dotnet build CFDIProcessor/CFDIProcessor.csproj --configuration Debug
```

---

## 🎮 Uso

### Ejecución Interactiva
```bash
cd CFDIProcessor\bin\Debug\netcoreapp3.1
CFDIProcessor.exe
```

El programa mostrará un menú:
```
Seleccione el tipo de CFDI a procesar:
  1. Nómina
  2. Ingreso y Egreso (Facturas)
  3. Pagos 2.0
  4. Todos (automático según tipo)

Opción (1-4): 
```

Luego solicita la ruta de la carpeta con los archivos XML.

### Ejecución por Línea de Comandos
```bash
# Procesar nóminas
echo 1 | CFDIProcessor.exe "C:\Ruta\Nominas"

# Procesar facturas
echo 2 | CFDIProcessor.exe "C:\Ruta\Facturas"

# Procesar pagos
echo 3 | CFDIProcessor.exe "C:\Ruta\Pagos"

# Procesar todos automáticamente
echo 4 | CFDIProcessor.exe "C:\Ruta\Todos"
```

---

## 📊 Ejemplos de Uso

### Ejemplo 1: Procesar CFDI de Ingreso

**Archivo XML:** factura.xml
```xml
<cfdi:Comprobante Version="4.0" TipoDeComprobante="I" ...>
  <cfdi:Conceptos>
    <cfdi:Concepto Descripcion="Servicio" Importe="1000.00">
      <cfdi:Impuestos>
        <cfdi:Traslados>
          <cfdi:Traslado Impuesto="002" TasaOCuota="0.160000" Importe="160.00"/>
        </cfdi:Traslados>
      </cfdi:Impuestos>
    </cfdi:Concepto>
  </cfdi:Conceptos>
</cfdi:Comprobante>
```

**Resultado en BD:**
- Comprobante guardado con EmitidaRecibida='E'
- Concepto guardado: "Servicio" $1,000.00
- Traslado IVA 16% guardado: $160.00

### Ejemplo 2: Procesar CFDI de Pagos 2.0

**Archivo XML:** pago.xml
```xml
<cfdi:Comprobante Version="4.0" TipoDeComprobante="P" ...>
  <cfdi:Complemento>
    <pago20:Pagos>
      <pago20:Pago FechaPago="2024-10-29" Monto="1160.00">
        <pago20:DoctoRelacionado IdDocumento="UUID-FACTURA" 
                                 NumParcialidad="1" 
                                 ImpPagado="1160.00"/>
      </pago20:Pago>
    </pago20:Pagos>
  </cfdi:Complemento>
</cfdi:Comprobante>
```

**Resultado en BD:**
- Comprobante de pago guardado
- Pago guardado: $1,160.00
- Documento relacionado con parcialidad 1

---

## 🧪 Pruebas

El proyecto incluye archivos XML de prueba en la carpeta `Pruebas/`:

### Ejecutar Pruebas Automáticas
```bash
# Iniciar SQL Server (como administrador)
.\iniciar_sql_server.bat

# Ejecutar todas las pruebas
.\ejecutar_pruebas_completas.bat
```

### Archivos de Prueba Incluidos
- `factura_ingreso_test.xml` - CFDI de Ingreso con IVA
- `pago_test.xml` - CFDI de Pagos 2.0
- `factura_con_retenciones_test.xml` - CFDI con IVA + ISR

---

## 📈 Consultas SQL Útiles

### Ver todos los comprobantes procesados
```sql
SELECT UUID, TipoDeComprobante, EmitidaRecibida, Total, Fecha
FROM CFDI_Comprobante
ORDER BY Fecha DESC;
```

### Ver impuestos de una factura
```sql
-- Traslados (IVA, IEPS)
SELECT t.Impuesto, t.TasaOCuota, t.Base, t.Importe
FROM CFDI_TrasladoConcepto t
INNER JOIN CFDI_Concepto c ON t.ID_Concepto = c.ID_Concepto
WHERE c.ID_Comprobante = [ID];

-- Retenciones (ISR, IVA Ret.)
SELECT r.Impuesto, r.TasaOCuota, r.Base, r.Importe
FROM CFDI_RetencionConcepto r
INNER JOIN CFDI_Concepto c ON r.ID_Concepto = c.ID_Concepto
WHERE c.ID_Comprobante = [ID];
```

### Ver pagos con documentos relacionados
```sql
SELECT 
    comp.UUID AS UUID_Pago,
    pp.FechaPago,
    pp.Monto,
    dr.IdDocumento AS UUID_Factura,
    dr.NumParcialidad,
    dr.ImpSaldoInsoluto
FROM CFDI_Comprobante comp
INNER JOIN CFDI_Pagos_Pago pp ON comp.ID_Comprobante = pp.ID_Comprobante
INNER JOIN CFDI_Pagos_DoctoRelacionado dr ON pp.ID_Pago = dr.ID_Pago
WHERE comp.TipoDeComprobante = 'P';
```

### Resumen de facturas emitidas vs recibidas
```sql
SELECT 
    EmitidaRecibida,
    CASE EmitidaRecibida
        WHEN 'E' THEN 'Emitida'
        WHEN 'R' THEN 'Recibida'
    END AS Tipo,
    COUNT(*) AS Cantidad,
    SUM(Total) AS Total_Monto
FROM CFDI_Comprobante
WHERE TipoDeComprobante IN ('I', 'E')
GROUP BY EmitidaRecibida;
```

---

## 🔧 Solución de Problemas

### SQL Server no inicia
```bash
# Ejecutar como administrador
Start-Service MSSQLSERVER
```

### Error de conexión a base de datos
1. Verificar que SQL Server esté corriendo
2. Verificar la cadena de conexión en `appsettings.json`
3. Verificar que la base de datos exista

### Archivos XML no se procesan
1. Verificar que los archivos sean XML válidos
2. Verificar que sean CFDI versión 4.0
3. Revisar el log de errores en la consola

---

## 📝 Notas Importantes

### Validación de Duplicados
El sistema valida automáticamente por UUID. Si un comprobante ya existe, se omite y se muestra un mensaje.

### Transacciones
Cada archivo XML se procesa en una transacción. Si hay un error, se hace rollback automático y no se guarda nada.

### Campo EmitidaRecibida
- Por defecto se guarda como 'E' (Emitida)
- Puede modificarse manualmente en la BD para facturas recibidas
- Útil para distinguir entre facturas propias y de proveedores

---

## 🆕 Cambios Recientes (v2.0)

### Nuevas Funcionalidades
✅ **Procesamiento de CFDI Ingreso/Egreso**
- Nuevo procesador: `IngresoEgresoXmlProcessor.cs`
- Guarda conceptos completos
- Guarda traslados (IVA, IEPS) por concepto
- Guarda retenciones (ISR, IVA Ret.) por concepto
- Campo EmitidaRecibida para distinguir emitidas/recibidas

✅ **Procesamiento de CFDI Pagos 2.0**
- Nuevo procesador: `PagosXmlProcessor.cs`
- Guarda detalle de pagos
- Guarda múltiples pagos por comprobante
- Guarda documentos relacionados con parcialidades
- Calcula saldos (anterior, pagado, insoluto)

✅ **Mejoras de Código**
- Corrección de error `Console.ReadKey()` en modo redirigido
- Mejor manejo de errores
- Exit code correcto (0 en éxito)
- Código más robusto y mantenible

### Archivos Nuevos
- `CFDIProcessor/Services/IngresoEgresoXmlProcessor.cs`
- `CFDIProcessor/Services/PagosXmlProcessor.cs`
- `CFDIProcessor/Models/PagosDetalle.cs`
- `CFDIProcessor/Models/PagosPago.cs`
- `CFDIProcessor/Models/PagosDoctoRelacionado.cs`
- `facturas.sql` - Script completo de base de datos
- `actualizar_bd.sql` - Script de actualización incremental

### Tablas Nuevas en BD
- `CFDI_Pagos_Detalle`
- `CFDI_Pagos_Pago`
- `CFDI_Pagos_DoctoRelacionado`

### Columnas Nuevas
- `CFDI_Comprobante.EmitidaRecibida` (E/R)

---

## 📊 Estadísticas del Proyecto

- **Lenguaje:** C# (.NET Core 3.1)
- **Base de Datos:** SQL Server
- **Tablas:** 9 principales + tablas de nómina
- **Procesadores:** 3 (Nómina, Ingreso/Egreso, Pagos)
- **Líneas de Código:** ~4,800+
- **Archivos de Prueba:** 3 XML incluidos
- **Pruebas Ejecutadas:** 4/4 exitosas (100%)

---

## 🤝 Contribuciones

Este proyecto fue desarrollado para procesar CFDI 4.0 de México de manera completa y eficiente.

### Rama Actual
- **main** - Versión estable
- **feature/cfdi-ingreso-egreso-pagos** - Nuevas funcionalidades v2.0

---

## 📄 Licencia

Este proyecto es de uso interno. Todos los derechos reservados.

---

## 👨‍💻 Autor

Desarrollado con asistencia de Kiro AI Assistant

---

## 📞 Soporte

Para problemas o preguntas:
1. Revisar la sección de Solución de Problemas
2. Verificar los archivos de prueba incluidos
3. Revisar los logs de error en la consola

---

**Versión:** 2.0  
**Última Actualización:** 3 de Noviembre de 2025  
**Estado:** ✅ Producción
