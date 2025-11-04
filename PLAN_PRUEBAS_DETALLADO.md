# 📋 Plan de Pruebas Detallado - Procesador CFDI v3.0

## Objetivo
Verificar el cumplimiento de los dos requisitos principales:
1. Proceso de búsqueda unificado (una sola vez)
2. Configuración de activación/desactivación por tipo

---

## Requisito 1: Proceso de Búsqueda Unificado

### Criterios de Aceptación
- ✅ La carpeta se recorre UNA SOLA VEZ
- ✅ Cada archivo se analiza individualmente
- ✅ Se detecta automáticamente el tipo de cada CFDI
- ✅ Se aplica el procesamiento correspondiente

### Pruebas a Realizar
1. **Prueba 1.1:** Carpeta con 4 tipos diferentes (I, E, N, P)
2. **Prueba 1.2:** Carpeta con múltiples archivos del mismo tipo
3. **Prueba 1.3:** Carpeta con archivos mezclados

---

## Requisito 2: Configuración de Activación/Desactivación

### Criterios de Aceptación
- ✅ Configuración en appsettings.json
- ✅ 4 opciones independientes (Ingreso, Egreso, Nómina, Pagos)
- ✅ Tipos activados se procesan
- ✅ Tipos desactivados se omiten con mensaje claro
- ✅ Sin necesidad de recompilar

### Pruebas a Realizar
1. **Prueba 2.1:** Todos activados
2. **Prueba 2.2:** Todos desactivados
3. **Prueba 2.3:** Solo Ingreso activado
4. **Prueba 2.4:** Solo Egreso activado
5. **Prueba 2.5:** Ingreso y Egreso activados, Nómina y Pagos desactivados
6. **Prueba 2.6:** Cambio de configuración sin recompilar

---

## Ejecución de Pruebas
Fecha: 4 de Noviembre de 2025
