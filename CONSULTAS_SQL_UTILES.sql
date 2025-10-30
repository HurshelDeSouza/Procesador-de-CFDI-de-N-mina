-- =====================================================
-- CONSULTAS SQL ÚTILES PARA CFDI DE NÓMINA
-- =====================================================

USE DescargaCfdiGFP;
GO

-- =====================================================
-- 1. CONSULTAS BÁSICAS
-- =====================================================

-- Ver todos los comprobantes procesados
SELECT 
    UUID,
    Fecha,
    Total,
    TipoDeComprobante,
    Estatus
FROM dbo.CFDI_Comprobante
ORDER BY Fecha DESC;

-- Contar total de comprobantes
SELECT COUNT(*) AS TotalComprobantes
FROM dbo.CFDI_Comprobante;

-- =====================================================
-- 2. INFORMACIÓN COMPLETA DE NÓMINA
-- =====================================================

-- Vista completa de nóminas con emisor y receptor
SELECT 
    c.UUID,
    c.Fecha,
    c.Serie,
    c.Folio,
    e.Nombre AS Emisor,
    e.Rfc AS RfcEmisor,
    r.Nombre AS Receptor,
    r.Rfc AS RfcReceptor,
    n.NumEmpleado,
    n.FechaPago,
    n.NumDiasPagados,
    n.TotalPercepciones,
    n.TotalDeducciones,
    n.TotalOtrosPagos,
    c.Total AS TotalNeto
FROM dbo.CFDI_Comprobante c
INNER JOIN dbo.CFDI_Emisor e ON c.ID_Comprobante = e.ID_Comprobante
INNER JOIN dbo.CFDI_Receptor r ON c.ID_Comprobante = r.ID_Comprobante
INNER JOIN dbo.Nomina_Detalle n ON c.ID_Comprobante = n.ID_Comprobante
ORDER BY c.Fecha DESC;

-- =====================================================
-- 3. ANÁLISIS DE PERCEPCIONES
-- =====================================================

-- Total de percepciones por empleado
SELECT 
    r.Nombre AS Empleado,
    r.Rfc,
    n.NumEmpleado,
    COUNT(p.ID_Percepcion) AS NumPercepciones,
    SUM(ISNULL(p.ImporteGravado, 0) + ISNULL(p.ImporteExento, 0)) AS TotalPercepciones
FROM dbo.CFDI_Comprobante c
INNER JOIN dbo.CFDI_Receptor r ON c.ID_Comprobante = r.ID_Comprobante
INNER JOIN dbo.Nomina_Detalle n ON c.ID_Comprobante = n.ID_Comprobante
INNER JOIN dbo.Nomina_Percepciones p ON c.ID_Comprobante = p.ID_Comprobante
GROUP BY r.Nombre, r.Rfc, n.NumEmpleado
ORDER BY TotalPercepciones DESC;

-- Detalle de percepciones por tipo
SELECT 
    p.TipoPercepcion,
    p.Concepto,
    COUNT(*) AS Cantidad,
    SUM(ISNULL(p.ImporteGravado, 0)) AS TotalGravado,
    SUM(ISNULL(p.ImporteExento, 0)) AS TotalExento,
    SUM(ISNULL(p.ImporteGravado, 0) + ISNULL(p.ImporteExento, 0)) AS Total
FROM dbo.Nomina_Percepciones p
GROUP BY p.TipoPercepcion, p.Concepto
ORDER BY Total DESC;

-- =====================================================
-- 4. ANÁLISIS DE DEDUCCIONES
-- =====================================================

-- Total de deducciones por empleado
SELECT 
    r.Nombre AS Empleado,
    r.Rfc,
    n.NumEmpleado,
    COUNT(d.ID_Deduccion) AS NumDeducciones,
    SUM(d.Importe) AS TotalDeducciones
FROM dbo.CFDI_Comprobante c
INNER JOIN dbo.CFDI_Receptor r ON c.ID_Comprobante = r.ID_Comprobante
INNER JOIN dbo.Nomina_Detalle n ON c.ID_Comprobante = n.ID_Comprobante
INNER JOIN dbo.Nomina_Deducciones d ON c.ID_Comprobante = d.ID_Comprobante
GROUP BY r.Nombre, r.Rfc, n.NumEmpleado
ORDER BY TotalDeducciones DESC;

-- Detalle de deducciones por tipo
SELECT 
    d.TipoDeduccion,
    d.Concepto,
    COUNT(*) AS Cantidad,
    SUM(d.Importe) AS TotalImporte,
    AVG(d.Importe) AS PromedioImporte
FROM dbo.Nomina_Deducciones d
GROUP BY d.TipoDeduccion, d.Concepto
ORDER BY TotalImporte DESC;

-- =====================================================
-- 5. REPORTES POR PERÍODO
-- =====================================================

-- Nóminas por mes
SELECT 
    YEAR(c.Fecha) AS Año,
    MONTH(c.Fecha) AS Mes,
    COUNT(*) AS NumNominas,
    SUM(c.Total) AS TotalPagado,
    AVG(c.Total) AS PromedioPorNomina
FROM dbo.CFDI_Comprobante c
INNER JOIN dbo.Nomina_Detalle n ON c.ID_Comprobante = n.ID_Comprobante
GROUP BY YEAR(c.Fecha), MONTH(c.Fecha)
ORDER BY Año DESC, Mes DESC;

-- Nóminas por rango de fechas
DECLARE @FechaInicio DATE = '2024-01-01';
DECLARE @FechaFin DATE = '2024-12-31';

SELECT 
    c.UUID,
    c.Fecha,
    r.Nombre AS Empleado,
    n.NumEmpleado,
    n.TotalPercepciones,
    n.TotalDeducciones,
    c.Total
FROM dbo.CFDI_Comprobante c
INNER JOIN dbo.CFDI_Receptor r ON c.ID_Comprobante = r.ID_Comprobante
INNER JOIN dbo.Nomina_Detalle n ON c.ID_Comprobante = n.ID_Comprobante
WHERE c.Fecha BETWEEN @FechaInicio AND @FechaFin
ORDER BY c.Fecha DESC;

-- =====================================================
-- 6. INFORMACIÓN DE EMPLEADOS
-- =====================================================

-- Lista de empleados únicos
SELECT DISTINCT
    r.Rfc,
    r.Nombre,
    n.NumEmpleado,
    n.NumSeguridadSocial,
    n.Puesto,
    n.Departamento,
    n.SalarioDiarioIntegrado
FROM dbo.CFDI_Receptor r
INNER JOIN dbo.Nomina_Detalle n ON r.ID_Comprobante = n.ID_Comprobante
ORDER BY r.Nombre;

-- Historial de nóminas por empleado
DECLARE @RfcEmpleado VARCHAR(13) = 'XAXX010101000'; -- Cambiar por RFC real

SELECT 
    c.Fecha,
    c.UUID,
    n.FechaPago,
    n.NumDiasPagados,
    n.TotalPercepciones,
    n.TotalDeducciones,
    c.Total AS Neto
FROM dbo.CFDI_Comprobante c
INNER JOIN dbo.CFDI_Receptor r ON c.ID_Comprobante = r.ID_Comprobante
INNER JOIN dbo.Nomina_Detalle n ON c.ID_Comprobante = n.ID_Comprobante
WHERE r.Rfc = @RfcEmpleado
ORDER BY c.Fecha DESC;

-- =====================================================
-- 7. ANÁLISIS POR EMISOR
-- =====================================================

-- Resumen por empresa emisora
SELECT 
    e.Rfc,
    e.Nombre,
    e.RegimenFiscal,
    COUNT(c.ID_Comprobante) AS NumNominas,
    SUM(c.Total) AS TotalPagado,
    MIN(c.Fecha) AS PrimeraNomina,
    MAX(c.Fecha) AS UltimaNomina
FROM dbo.CFDI_Emisor e
INNER JOIN dbo.CFDI_Comprobante c ON e.ID_Comprobante = c.ID_Comprobante
GROUP BY e.Rfc, e.Nombre, e.RegimenFiscal
ORDER BY TotalPagado DESC;

-- =====================================================
-- 8. VALIDACIONES Y AUDITORÍA
-- =====================================================

-- Verificar integridad: Comprobantes sin nómina
SELECT 
    c.UUID,
    c.Fecha,
    c.TipoDeComprobante
FROM dbo.CFDI_Comprobante c
LEFT JOIN dbo.Nomina_Detalle n ON c.ID_Comprobante = n.ID_Comprobante
WHERE n.ID_Comprobante IS NULL;

-- Verificar totales: Comparar total del comprobante vs suma de percepciones y deducciones
SELECT 
    c.UUID,
    c.Total AS TotalComprobante,
    n.TotalPercepciones,
    n.TotalDeducciones,
    (ISNULL(n.TotalPercepciones, 0) - ISNULL(n.TotalDeducciones, 0)) AS CalculoNeto,
    (c.Total - (ISNULL(n.TotalPercepciones, 0) - ISNULL(n.TotalDeducciones, 0))) AS Diferencia
FROM dbo.CFDI_Comprobante c
INNER JOIN dbo.Nomina_Detalle n ON c.ID_Comprobante = n.ID_Comprobante
WHERE ABS(c.Total - (ISNULL(n.TotalPercepciones, 0) - ISNULL(n.TotalDeducciones, 0))) > 0.01;

-- Buscar duplicados por UUID (no debería haber)
SELECT 
    UUID,
    COUNT(*) AS Cantidad
FROM dbo.CFDI_Comprobante
GROUP BY UUID
HAVING COUNT(*) > 1;

-- =====================================================
-- 9. ESTADÍSTICAS GENERALES
-- =====================================================

-- Dashboard general
SELECT 
    'Total Comprobantes' AS Metrica,
    COUNT(*) AS Valor
FROM dbo.CFDI_Comprobante
UNION ALL
SELECT 
    'Total Empleados Únicos',
    COUNT(DISTINCT r.Rfc)
FROM dbo.CFDI_Receptor r
INNER JOIN dbo.Nomina_Detalle n ON r.ID_Comprobante = n.ID_Comprobante
UNION ALL
SELECT 
    'Total Pagado',
    SUM(Total)
FROM dbo.CFDI_Comprobante
UNION ALL
SELECT 
    'Promedio por Nómina',
    AVG(Total)
FROM dbo.CFDI_Comprobante;

-- =====================================================
-- 10. EXPORTACIÓN PARA EXCEL
-- =====================================================

-- Formato para exportar a Excel - Resumen de nóminas
SELECT 
    c.UUID AS 'UUID',
    CONVERT(VARCHAR(10), c.Fecha, 103) AS 'Fecha',
    e.Nombre AS 'Empresa',
    e.Rfc AS 'RFC Empresa',
    r.Nombre AS 'Empleado',
    r.Rfc AS 'RFC Empleado',
    n.NumEmpleado AS 'Num. Empleado',
    n.Puesto AS 'Puesto',
    n.Departamento AS 'Departamento',
    n.NumDiasPagados AS 'Días Pagados',
    n.TotalPercepciones AS 'Percepciones',
    n.TotalDeducciones AS 'Deducciones',
    c.Total AS 'Neto'
FROM dbo.CFDI_Comprobante c
INNER JOIN dbo.CFDI_Emisor e ON c.ID_Comprobante = e.ID_Comprobante
INNER JOIN dbo.CFDI_Receptor r ON c.ID_Comprobante = r.ID_Comprobante
INNER JOIN dbo.Nomina_Detalle n ON c.ID_Comprobante = n.ID_Comprobante
ORDER BY c.Fecha DESC;

-- =====================================================
-- 11. LIMPIEZA Y MANTENIMIENTO
-- =====================================================

-- Eliminar un comprobante específico (y todos sus datos relacionados)
-- CUIDADO: Esto eliminará todos los datos relacionados
/*
DECLARE @UuidEliminar VARCHAR(36) = 'UUID-A-ELIMINAR';

DELETE FROM dbo.Nomina_OtrosPagos WHERE ID_Comprobante IN (SELECT ID_Comprobante FROM dbo.CFDI_Comprobante WHERE UUID = @UuidEliminar);
DELETE FROM dbo.Nomina_Deducciones WHERE ID_Comprobante IN (SELECT ID_Comprobante FROM dbo.CFDI_Comprobante WHERE UUID = @UuidEliminar);
DELETE FROM dbo.Nomina_Percepciones WHERE ID_Comprobante IN (SELECT ID_Comprobante FROM dbo.CFDI_Comprobante WHERE UUID = @UuidEliminar);
DELETE FROM dbo.Nomina_Detalle WHERE ID_Comprobante IN (SELECT ID_Comprobante FROM dbo.CFDI_Comprobante WHERE UUID = @UuidEliminar);
DELETE FROM dbo.CFDI_Concepto WHERE ID_Comprobante IN (SELECT ID_Comprobante FROM dbo.CFDI_Comprobante WHERE UUID = @UuidEliminar);
DELETE FROM dbo.CFDI_Receptor WHERE ID_Comprobante IN (SELECT ID_Comprobante FROM dbo.CFDI_Comprobante WHERE UUID = @UuidEliminar);
DELETE FROM dbo.CFDI_Emisor WHERE ID_Comprobante IN (SELECT ID_Comprobante FROM dbo.CFDI_Comprobante WHERE UUID = @UuidEliminar);
DELETE FROM dbo.CFDI_Comprobante WHERE UUID = @UuidEliminar;
*/

-- Ver tamaño de las tablas
SELECT 
    t.NAME AS TableName,
    p.rows AS RowCounts,
    SUM(a.total_pages) * 8 AS TotalSpaceKB, 
    SUM(a.used_pages) * 8 AS UsedSpaceKB
FROM sys.tables t
INNER JOIN sys.indexes i ON t.OBJECT_ID = i.object_id
INNER JOIN sys.partitions p ON i.object_id = p.OBJECT_ID AND i.index_id = p.index_id
INNER JOIN sys.allocation_units a ON p.partition_id = a.container_id
WHERE t.NAME NOT LIKE 'dt%' 
    AND t.is_ms_shipped = 0
    AND i.OBJECT_ID > 255 
GROUP BY t.Name, p.Rows
ORDER BY TotalSpaceKB DESC;
