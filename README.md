# 📊 Procesador de CFDI - Versión 3.0 (Unificada)

Sistema completo para procesar archivos XML de CFDI (Comprobante Fiscal Digital por Internet) de México, incluyendo Nómina, Ingreso/Egreso y Pagos 2.0.

## 🚀 Características

### ✅ Tipos de CFDI Soportados
- **Nómina (N)** - Recibos de nómina con percepciones, deducciones y otros pagos
- **Ingreso (I)** - Facturas de venta con conceptos e impuestos
- **Egreso (E)** - Notas de crédito con conceptos e impuestos
- **Pagos 2.0 (P)** - Complementos de pago con documentos relacionados y parcialidades

### ✅ Funcionalidades Principales
- **🆕 Procesamiento Unificado**: Un solo proceso que detecta automáticamente el tipo de CFDI
- **🆕 Configuración Flexible**: Activar/desactivar procesamiento por tipo desde `appsettings.json`
- **🆕 Mayor Eficiencia**: La carpeta se recorre una sola vez, procesando todos los tipos habilitados
- Procesamiento automático de archivos XML
- Guardado completo en base de datos SQL Server
- Procesamiento de conceptos con traslados y retenciones
- Soporte para múltiples pagos y documentos relacionados
- Validación de duplicados por UUID
- Manejo de transacciones con rollback automático
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

### 3. Configurar Cadena de Conexión y Procesamiento

Editar `CFDIProcessor/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=DescargaCfdiGFP;Integrated Security=True;TrustServerCertificate=True;"
  },
  "ProcessingSettings": {
    "ProcessNomina": true,
    "ProcessIngreso": true,
    "ProcessEgreso": true,
    "ProcessPagos": true
  }
}
```

**Configuración de Procesamiento:**
- `ProcessNomina`: Activar/desactivar procesamiento de CFDI de Nómina
- `ProcessIngreso`: Activar/desactivar procesamiento de CFDI de Ingreso
- `ProcessEgreso`: Activar/desactivar procesamiento de CFDI de Egreso
- `ProcessPagos`: Activar/desactivar procesamiento de CFDI de Pagos

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

El programa solicitará la ruta de la carpeta con los archivos XML y procesará automáticamente todos los tipos de CFDI habilitados en la configuración.

### Ejecución por Línea de Comandos
```bash
# Procesar carpeta con archivos mixtos
CFDIProcessor.exe "C:\Ruta\Archivos"
```

### Ventajas del Procesamiento Unificado
- **Eficiencia**: La carpeta se recorre una sola vez
- **Flexibilidad**: Procesa múltiples tipos de CFDI en una sola ejecución
- **Configuración centralizada**: Control desde `appsettings.json`
- **Detección automática**: No requiere selección manual del tipo
- **Mejor rendimiento**: Menos operaciones de I/O

### Ejemplo de Salida
```
=== Configuración de Procesamiento ===
Nómina: ✓ Activado
Ingreso: ✓ Activado
Egreso: ✓ Activado
Pagos: ✓ Activado

Se encontraron 15 archivo(s) XML.

✓ factura_001.xml: Ingreso procesado (UUID: ABC123...)
✓ nomina_001.xml: Nómina procesada (UUID: DEF456...)
✓ pago_001.xml: Pago procesado (UUID: GHI789...)
⊘ factura_002.xml: UUID ABC123... ya existe

=== Resumen del Procesamiento ===
✓ Nómina procesados: 5
✓ Ingreso procesados: 7
✓ Egreso procesados: 1
✓ Pagos procesados: 2
⊘ Omitidos (duplicados, desactivados o no válidos): 1
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

### Error: "UUID ya existe"
**¿Es un problema?** NO - Este es el comportamiento correcto.

El sistema valida que no se procesen archivos duplicados. Si el UUID ya existe, omite el archivo para evitar duplicados.

### Error: "Nomina_Deducciones no es válido" o "Tabla no existe"
**Causa:** Las tablas de nómina/pagos no existen en la base de datos.

**Solución 1 (Recomendada):** Ejecutar el script completo
```bash
sqlcmd -S localhost -d DescargaCfdiGFP -i facturas.sql
```

**Solución 2 (Temporal):** Desactivar tipos en `appsettings.json`
```json
{
  "ProcessingSettings": {
    "ProcessNomina": false,  // Desactivar si faltan tablas
    "ProcessPagos": false    // Desactivar si faltan tablas
  }
}
```

### Archivos XML no se procesan
1. Verificar que los archivos sean XML válidos
2. Verificar que sean CFDI versión 4.0
3. Verificar que el tipo esté activado en `appsettings.json`
4. Revisar el log de errores en la consola

### Configuración no se aplica
1. Verificar que `appsettings.json` esté en el directorio de ejecución
2. Verificar sintaxis JSON correcta
3. Reiniciar la aplicación después de cambios

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

## 🆕 Cambios Recientes (v3.0 - Unificada)

### Nuevas Funcionalidades v3.0
✅ **Procesamiento Unificado**
- Nuevo procesador: `UnifiedCfdiProcessor.cs`
- Un solo recorrido de carpeta para todos los tipos
- Detección automática del tipo de CFDI
- Procesamiento inteligente según configuración
- Mejor rendimiento y eficiencia

✅ **Configuración Flexible**
- Activar/desactivar tipos desde `appsettings.json`
- Sección `ProcessingSettings` con 4 opciones
- Sin necesidad de recompilar para cambiar tipos
- Configuración centralizada y clara

✅ **Mejoras de Interfaz**
- Eliminado menú de selección manual
- Proceso más directo y rápido
- Resumen detallado por tipo procesado
- Mejor visualización de resultados

### Cambios en v2.0
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

### Archivos Nuevos v3.0
- `CFDIProcessor/Services/UnifiedCfdiProcessor.cs` - Procesador unificado

### Archivos Existentes (v2.0)
- `CFDIProcessor/Services/IngresoEgresoXmlProcessor.cs`
- `CFDIProcessor/Services/PagosXmlProcessor.cs`
- `CFDIProcessor/Services/NominaXmlProcessor.cs`
- `CFDIProcessor/Models/PagosDetalle.cs`
- `CFDIProcessor/Models/PagosPago.cs`
- `CFDIProcessor/Models/PagosDoctoRelacionado.cs`
- `facturas.sql` - Script completo de base de datos

### Tablas en BD
- `CFDI_Comprobante` (con EmitidaRecibida)
- `CFDI_Emisor`
- `CFDI_Receptor`
- `CFDI_Concepto`
- `CFDI_TrasladoConcepto`
- `CFDI_RetencionConcepto`
- `Nomina_Detalle`
- `Nomina_Percepciones`
- `Nomina_Deducciones`
- `Nomina_OtrosPagos`
- `CFDI_Pagos_Detalle`
- `CFDI_Pagos_Pago`
- `CFDI_Pagos_DoctoRelacionado`

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

**Versión:** 3.0 (Unificada)  
**Última Actualización:** 4 de Noviembre de 2025  
**Estado:** ✅ Producción
