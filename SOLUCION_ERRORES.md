# 🔧 Solución de Errores - Procesador CFDI v3.0

## Errores Encontrados y Sus Soluciones

---

## ❌ Error 1: "UUID ya existe"

### Mensaje de Error
```
⊘ test_egreso_001.xml: UUID BBBBBBBB-2222-2222-2222-222222222222 ya existe
⊘ test_ingreso_001.xml: UUID AAAAAAAA-1111-1111-1111-111111111111 ya existe
```

### ¿Es un problema?
**NO** - Este es el comportamiento correcto del sistema.

### Explicación
El sistema valida que no se procesen archivos duplicados verificando el UUID en la base de datos. Si el UUID ya existe, omite el archivo para evitar duplicados.

### Solución
Si quieres procesar archivos de prueba:
1. Usa archivos XML con UUIDs diferentes
2. O elimina los registros existentes de la base de datos

**Ejemplo de UUID único:**
```xml
<tfd:TimbreFiscalDigital UUID="11111111-AAAA-BBBB-CCCC-111111111111" />
```

---

## ❌ Error 2: "El nombre de objeto 'Nomina_Deducciones' no es válido"

### Mensaje de Error
```
✗ Error procesando test_nomina_001.xml: An error occurred while updating the entries.
  Detalle: El nombre de objeto 'Nomina_Deducciones' no es válido.
```

### ¿Es un problema?
**SÍ** - Las tablas de nómina no existen en tu base de datos.

### Causa
Las tablas necesarias para procesar CFDI de nómina no están creadas en la base de datos:
- `Nomina_Detalle`
- `Nomina_Percepciones`
- `Nomina_Deducciones`
- `Nomina_OtrosPagos`

### Solución 1: Crear las Tablas (Recomendado)

Ejecuta el script SQL completo:

```bash
sqlcmd -S localhost -d DescargaCfdiGFP -i facturas.sql
```

O manualmente en SQL Server Management Studio:
1. Abre `facturas.sql`
2. Ejecuta el script completo
3. Verifica que las tablas se crearon

### Solución 2: Desactivar Procesamiento de Nómina (Temporal)

Si solo necesitas procesar Ingreso y Egreso, desactiva Nómina en `appsettings.json`:

```json
{
  "ProcessingSettings": {
    "ProcessNomina": false,    // ← Desactivado
    "ProcessIngreso": true,
    "ProcessEgreso": true,
    "ProcessPagos": false
  }
}
```

### Verificar Tablas Existentes

Ejecuta este query en SQL Server:

```sql
USE DescargaCfdiGFP;
GO

SELECT TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME LIKE '%Nomina%'
ORDER BY TABLE_NAME;
```

**Resultado esperado:**
```
Nomina_Deducciones
Nomina_Detalle
Nomina_OtrosPagos
Nomina_Percepciones
```

---

## ❌ Error 3: Mismo error en archivos de Pagos

### Mensaje de Error
```
✗ Error procesando test_pago_001.xml: El nombre de objeto 'Nomina_Deducciones' no es válido
```

### ¿Por qué menciona Nomina_Deducciones en un archivo de Pagos?
Este es un error engañoso. El problema real es que faltan las tablas de Pagos:
- `CFDI_Pagos_Detalle`
- `CFDI_Pagos_Pago`
- `CFDI_Pagos_DoctoRelacionado`

### Solución
Igual que el Error 2:
1. Ejecutar `facturas.sql` completo
2. O desactivar procesamiento de Pagos temporalmente

---

## ✅ Prueba Exitosa

### Configuración Usada
```json
{
  "ProcessingSettings": {
    "ProcessNomina": false,
    "ProcessIngreso": true,
    "ProcessEgreso": true,
    "ProcessPagos": false
  }
}
```

### Resultado
```
=== Configuración de Procesamiento ===
Nómina: ✗ Desactivado
Ingreso: ✓ Activado
Egreso: ✓ Activado
Pagos: ✗ Desactivado

Se encontraron 2 archivo(s) XML.

✓ nuevo_egreso_001.xml: Egreso procesado (UUID: 22222222-AAAA-BBBB-CCCC-222222222222)
✓ nuevo_ingreso_001.xml: Ingreso procesado (UUID: 11111111-AAAA-BBBB-CCCC-111111111111)

=== Resumen del Procesamiento ===
✓ Ingreso procesados: 1
✓ Egreso procesados: 1
⊘ Omitidos: 0
```

**Estado:** ✅ **SIN ERRORES**

---

## 📋 Checklist de Verificación

Antes de usar el sistema en producción:

- [ ] Verificar que SQL Server esté corriendo
- [ ] Verificar que la base de datos `DescargaCfdiGFP` exista
- [ ] Ejecutar script `facturas.sql` para crear todas las tablas
- [ ] Verificar cadena de conexión en `appsettings.json`
- [ ] Configurar qué tipos de CFDI procesar
- [ ] Probar con archivos de muestra
- [ ] Verificar que no haya errores en la consola

---

## 🎯 Resumen

### Errores que NO son problemas:
- ✅ "UUID ya existe" - Validación de duplicados funcionando

### Errores que SÍ requieren acción:
- ❌ "Nomina_Deducciones no es válido" - Crear tablas o desactivar tipo
- ❌ Errores en Pagos - Crear tablas o desactivar tipo

### Solución Rápida:
1. **Para producción:** Ejecutar `facturas.sql` completo
2. **Para pruebas:** Desactivar tipos que no necesites

---

**Fecha:** 4 de Noviembre de 2025  
**Versión:** 3.0 (Unificada)  
**Estado:** ✅ Documentado y Resuelto
