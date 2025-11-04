# Cambios Versión 3.0 - Procesamiento Unificado

## Resumen

Se ha refactorizado el sistema para implementar un **procesamiento unificado** que recorre la carpeta de archivos XML una sola vez, detectando automáticamente el tipo de CFDI y procesándolo según la configuración establecida.

## Cambios Principales

### 1. Nuevo Procesador Unificado

**Archivo:** `CFDIProcessor/Services/UnifiedCfdiProcessor.cs`

- Procesador único que maneja todos los tipos de CFDI (Nómina, Ingreso, Egreso, Pagos)
- Detección automática del tipo de CFDI mediante análisis del XML
- Procesamiento condicional basado en configuración
- Un solo recorrido de carpeta para mayor eficiencia

### 2. Configuración Flexible

**Archivo:** `CFDIProcessor/appsettings.json`

Nueva sección `ProcessingSettings`:
```json
{
  "ProcessingSettings": {
    "ProcessNomina": true,
    "ProcessIngreso": true,
    "ProcessEgreso": true,
    "ProcessPagos": true
  }
}
```

Permite activar/desactivar el procesamiento de cada tipo sin recompilar.

### 3. Modelo de Configuración

**Archivo:** `CFDIProcessor/Models/ProcessingSettings.cs`

Nuevo modelo para mapear la configuración de procesamiento.

### 4. Simplificación del Program.cs

**Cambios en:** `CFDIProcessor/Program.cs`

- Eliminado el menú de selección de tipo de CFDI
- Proceso más directo: solo solicita la ruta de la carpeta
- Usa el procesador unificado automáticamente
- Interfaz más simple y rápida

## Ventajas del Nuevo Enfoque

### Eficiencia
- **Antes:** Se recorría la carpeta múltiples veces (una por cada tipo)
- **Ahora:** Un solo recorrido procesa todos los tipos habilitados

### Flexibilidad
- **Antes:** Selección manual del tipo en cada ejecución
- **Ahora:** Configuración persistente en `appsettings.json`

### Mantenibilidad
- Código centralizado en un solo procesador
- Lógica de detección de tipo unificada
- Más fácil de mantener y extender

### Rendimiento
- Menos operaciones de I/O (lectura de archivos)
- Procesamiento más rápido en carpetas grandes
- Mejor uso de recursos del sistema

## Flujo de Procesamiento

```
1. Cargar configuración desde appsettings.json
2. Obtener ruta de carpeta
3. Listar todos los archivos XML
4. Para cada archivo:
   a. Cargar XML
   b. Detectar tipo de CFDI (N, I, E, P)
   c. Verificar si el tipo está habilitado
   d. Si está habilitado, procesar según el tipo
   e. Si no, omitir con mensaje
5. Mostrar resumen por tipo procesado
```

## Detección Automática de Tipo

El sistema detecta el tipo de CFDI mediante:

1. **Nómina (N):**
   - Atributo `TipoDeComprobante="N"`
   - Presencia de complemento `Nomina`

2. **Ingreso (I):**
   - Atributo `TipoDeComprobante="I"`

3. **Egreso (E):**
   - Atributo `TipoDeComprobante="E"`

4. **Pagos (P):**
   - Atributo `TipoDeComprobante="P"`
   - Presencia de complemento `Pagos20`

## Ejemplo de Salida

```
=== Configuración de Procesamiento ===
Nómina: ✓ Activado
Ingreso: ✓ Activado
Egreso: ✗ Desactivado
Pagos: ✓ Activado

Se encontraron 20 archivo(s) XML.

✓ factura_001.xml: Ingreso procesado (UUID: ABC123...)
✓ nomina_001.xml: Nómina procesada (UUID: DEF456...)
⊘ egreso_001.xml: Egreso desactivado en configuración
✓ pago_001.xml: Pago procesado (UUID: GHI789...)
⊘ factura_002.xml: UUID ABC123... ya existe

=== Resumen del Procesamiento ===
✓ Nómina procesados: 8
✓ Ingreso procesados: 10
✓ Pagos procesados: 1
⊘ Omitidos (duplicados, desactivados o no válidos): 2
```

## Compatibilidad

- ✅ Mantiene compatibilidad con la base de datos existente
- ✅ No requiere cambios en el esquema de BD
- ✅ Los procesadores anteriores siguen disponibles si se necesitan
- ✅ Funciona con archivos XML existentes

## Archivos Modificados

1. `CFDIProcessor/Program.cs` - Simplificado para usar procesador unificado
2. `CFDIProcessor/appsettings.json` - Agregada sección ProcessingSettings
3. `README.md` - Actualizado con nueva funcionalidad

## Archivos Nuevos

1. `CFDIProcessor/Services/UnifiedCfdiProcessor.cs` - Procesador unificado
2. `CFDIProcessor/Models/ProcessingSettings.cs` - Modelo de configuración
3. `CAMBIOS_V3.md` - Este documento

## Archivos Existentes (Sin Cambios)

Los procesadores individuales se mantienen sin cambios:
- `NominaXmlProcessor.cs`
- `IngresoEgresoXmlProcessor.cs`
- `PagosXmlProcessor.cs`

## Migración desde v2.0

No se requiere migración. El sistema es compatible con:
- Base de datos existente
- Archivos XML existentes
- Configuración existente (se agregan nuevas opciones)

## Pruebas Recomendadas

1. Probar con carpeta que contenga múltiples tipos de CFDI
2. Probar desactivando tipos específicos en configuración
3. Verificar que los duplicados se detecten correctamente
4. Validar que el resumen muestre contadores correctos por tipo

## Notas Técnicas

- El procesador unificado reutiliza la lógica de los procesadores individuales
- Cada tipo se procesa en su propia transacción
- Los errores en un archivo no afectan el procesamiento de otros
- La configuración se carga una sola vez al inicio

## Versión

- **Versión Anterior:** 2.0
- **Versión Nueva:** 3.0 (Unificada)
- **Fecha:** 4 de Noviembre de 2025
- **Estado:** ✅ Producción
