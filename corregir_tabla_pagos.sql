-- Corregir estructura de tabla CFDI_Pagos_Detalle
USE DescargaCfdiGFP;
GO

PRINT 'Corrigiendo tabla CFDI_Pagos_Detalle...';

-- Eliminar tabla existente si existe
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'CFDI_Pagos_DoctoRelacionado')
BEGIN
    DROP TABLE dbo.CFDI_Pagos_DoctoRelacionado;
    PRINT '✓ Tabla CFDI_Pagos_DoctoRelacionado eliminada';
END

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'CFDI_Pagos_Pago')
BEGIN
    DROP TABLE dbo.CFDI_Pagos_Pago;
    PRINT '✓ Tabla CFDI_Pagos_Pago eliminada';
END

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'CFDI_Pagos_Detalle')
BEGIN
    DROP TABLE dbo.CFDI_Pagos_Detalle;
    PRINT '✓ Tabla CFDI_Pagos_Detalle eliminada';
END
GO

-- Crear tabla CFDI_Pagos_Detalle con estructura correcta
CREATE TABLE dbo.CFDI_Pagos_Detalle (
    ID_Comprobante INT PRIMARY KEY,
    FormaDePago VARCHAR(2) NULL,
    MontoTotalPagos DECIMAL(18,2) NOT NULL,
    TotalTrasladosBaseIva16 DECIMAL(18,2) NULL,
    TotalTrasladosImpuestoIva16 DECIMAL(18,2) NULL,
    CONSTRAINT FK_Pagos_Detalle_Comprobante FOREIGN KEY (ID_Comprobante) 
        REFERENCES dbo.CFDI_Comprobante(ID_Comprobante)
);
PRINT '✓ Tabla CFDI_Pagos_Detalle creada correctamente';
GO

-- Crear tabla CFDI_Pagos_Pago
CREATE TABLE dbo.CFDI_Pagos_Pago (
    ID_Pago INT IDENTITY(1,1) PRIMARY KEY,
    ID_Comprobante INT NOT NULL,
    FechaPago DATETIME2 NOT NULL,
    FormaDePagoP VARCHAR(2) NOT NULL,
    MonedaP VARCHAR(3) NOT NULL,
    TipoCambioP DECIMAL(18,6) NULL,
    Monto DECIMAL(18,2) NOT NULL,
    NumOperacion VARCHAR(100) NULL,
    CONSTRAINT FK_Pagos_Pago_Comprobante FOREIGN KEY (ID_Comprobante) 
        REFERENCES dbo.CFDI_Comprobante(ID_Comprobante)
);
PRINT '✓ Tabla CFDI_Pagos_Pago creada correctamente';
GO

-- Crear tabla CFDI_Pagos_DoctoRelacionado
CREATE TABLE dbo.CFDI_Pagos_DoctoRelacionado (
    ID_DoctoRel INT IDENTITY(1,1) PRIMARY KEY,
    ID_Pago INT NOT NULL,
    IdDocumento VARCHAR(36) NOT NULL,
    Serie VARCHAR(25) NULL,
    Folio VARCHAR(40) NULL,
    MonedaDR VARCHAR(3) NOT NULL,
    EquivalenciaDR DECIMAL(18,6) NULL,
    NumParcialidad INT NULL,
    ImpSaldoAnt DECIMAL(18,2) NOT NULL,
    ImpPagado DECIMAL(18,2) NOT NULL,
    ImpSaldoInsoluto DECIMAL(18,2) NOT NULL,
    ObjetoImpDR VARCHAR(2) NULL,
    ObjetoImp VARCHAR(2) NULL,
    CONSTRAINT FK_Pagos_DoctoRel_Pago FOREIGN KEY (ID_Pago) 
        REFERENCES dbo.CFDI_Pagos_Pago(ID_Pago)
);
PRINT '✓ Tabla CFDI_Pagos_DoctoRelacionado creada correctamente';
GO

PRINT '';
PRINT '╔════════════════════════════════════════════════════════════╗';
PRINT '║  ✅ TABLAS DE PAGOS CORREGIDAS                            ║';
PRINT '╚════════════════════════════════════════════════════════════╝';
GO
