-- ═══════════════════════════════════════════════════════════════
-- CONSULTAS SQL ÚTILES - CFDI Processor v2.0
-- ═══════════════════════════════════════════════════════════════

-- 1. Ver todos los comprobantes con sus impuestos
SELECT 
    c.UUID,
    c.Serie,
    c.Folio,
    c.TipoDeComprobante,
    c.EmitidaRecibida,
    c.SubTotal,
    c.Total,
    c.Fecha
FROM CFDI_Comprobante c
ORDER BY c.Fecha DESC;

-- 2. Ver traslados (IVA, IEPS) de un comprobante
SELECT 
    comp.UUID,
    comp.Serie,
    comp.Folio,
    conc.Descripcion,
    t.Impuesto,
    t.TipoFactor,
    t.TasaOCuota,
    t.Base,
    t.Importe
FROM CFDI_TrasladoConcepto t
INNER JOIN CFDI_Concepto conc ON t.ID_Concepto = conc.ID_Concepto
INNER JOIN CFDI_Comprobante comp ON conc.ID_Comprobante = comp.ID_Comprobante
WHERE comp.UUID = 'TU-UUID-AQUI';

-- 3. Ver retenciones (ISR, IVA Ret.) de un comprobante
SELECT 
    comp.UUID,
    comp.Serie,
    comp.Folio,
    conc.Descripcion,
    r.Impuesto,
    r.TipoFactor,
    r.TasaOCuota,
    r.Base,
    r.Importe
FROM CFDI_RetencionConcepto r
INNER JOIN CFDI_Concepto conc ON r.ID_Concepto = conc.ID_Concepto
INNER JOIN CFDI_Comprobante comp ON conc.ID_Comprobante = comp.ID_Comprobante
WHERE comp.UUID = 'TU-UUID-AQUI';

-- 4. Ver pagos con sus documentos relacionados
SELECT 
    comp.UUID AS UUID_Pago,
    comp.Fecha AS Fecha_Comprobante,
    pp.FechaPago,
    pp.FormaDePagoP,
    pp.Monto,
    dr.IdDocumento AS UUID_Factura,
    dr.NumParcialidad,
    dr.ImpSaldoAnt,
    dr.ImpPagado,
    dr.ImpSaldoInsoluto
FROM CFDI_Comprobante comp
INNER JOIN CFDI_Pagos_Pago pp ON comp.ID_Comprobante = pp.ID_Comprobante
INNER JOIN CFDI_Pagos_DoctoRelacionado dr ON pp.ID_Pago = dr.ID_Pago
WHERE comp.TipoDeComprobante = 'P'
ORDER BY comp.Fecha DESC;

-- 5. Resumen de impuestos por tipo
SELECT 
    t.Impuesto,
    CASE t.Impuesto
        WHEN '001' THEN 'ISR'
        WHEN '002' THEN 'IVA'
        WHEN '003' THEN 'IEPS'
        ELSE 'Otro'
    END AS Nombre_Impuesto,
    COUNT(*) AS Cantidad,
    SUM(t.Base) AS Total_Base,
    SUM(t.Importe) AS Total_Importe
FROM CFDI_TrasladoConcepto t
GROUP BY t.Impuesto;

-- 6. Ver facturas emitidas vs recibidas
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
