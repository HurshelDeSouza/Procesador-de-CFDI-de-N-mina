-- Script para actualizar la base de datos con las nuevas funcionalidades
USE DescargaCfdiGFP;
GO

PRINT 'Iniciando actualización de base de datos...';
GO

-- 1. Agregar columna EmitidaRecibida si no existe
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.CFDI_Comprobante') AND name = 'EmitidaRecibida')
BEGIN
    PRINT 'Agregando columna EmitidaRecibida...';
    ALTER TABLE dbo.CFDI_Comprobante 
    ADD EmitidaRecibida CHAR(1) NOT NULL DEFAULT 'E';
    PRINT '✓ Columna EmitidaRecibida agregada';
END
ELSE
BEGIN
    PRINT '✓ Columna EmitidaRecibida ya existe';
END
GO

-- 2. Crear tabla CFDI_Concepto_Impuestos si no existe
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CFDI_Concepto_Impuestos')
BEGIN
    PRINT 'Creando tabla CFDI_Concepto_Impuestos...';
    CREATE TABLE dbo.CFDI_Concepto_Impuestos (
        ID_Concepto_Impuesto INT IDENTITY(1,1) PRIMARY KEY,
        ID_Concepto INT NOT NULL,
        TipoImpuesto VARCHAR(20) NOT NULL, -- 'Traslado' o 'Retencion'
        Impuesto VARCHAR(3) NOT NULL, -- '001' (ISR), '002' (IVA), '003' (IEPS)
        TipoFactor VARCHAR(10) NULL, -- 'Tasa', 'Cuota', 'Exento'
        TasaOCuota DECIMAL(18,6) NULL,
        Base DECIMAL(18,2) NOT NULL,
        Importe DECIMAL(18,2) NULL,
        CONSTRAINT FK_Concepto_Impuestos_Concepto FOREIGN KEY (ID_Concepto) 
            REFERENCES dbo.CFDI_Concepto(ID_Concepto)
    );
    PRINT '✓ Tabla CFDI_Concepto_Impuestos creada';
END
ELSE
BEGIN
    PRINT '✓ Tabla CFDI_Concepto_Impuestos ya existe';
END
GO

-- 3. Crear tabla CFDI_Pagos_Detalle si no existe
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CFDI_Pagos_Detalle')
BEGIN
    PRINT 'Creando tabla CFDI_Pagos_Detalle...';
    CREATE TABLE dbo.CFDI_Pagos_Detalle (
        ID_Pago_Detalle INT IDENTITY(1,1) PRIMARY KEY,
        ID_Comprobante INT NOT NULL,
        Version VARCHAR(5) NOT NULL,
        CONSTRAINT FK_Pagos_Detalle_Comprobante FOREIGN KEY (ID_Comprobante) 
            REFERENCES dbo.CFDI_Comprobante(ID_Comprobante)
    );
    PRINT '✓ Tabla CFDI_Pagos_Detalle creada';
END
ELSE
BEGIN
    PRINT '✓ Tabla CFDI_Pagos_Detalle ya existe';
END
GO

-- 4. Crear tabla CFDI_Pagos_Pago si no existe
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CFDI_Pagos_Pago')
BEGIN
    PRINT 'Creando tabla CFDI_Pagos_Pago...';
    CREATE TABLE dbo.CFDI_Pagos_Pago (
        ID_Pago INT IDENTITY(1,1) PRIMARY KEY,
        ID_Pago_Detalle INT NOT NULL,
        FechaPago DATETIME2 NOT NULL,
        FormaDePagoP VARCHAR(2) NOT NULL,
        MonedaP VARCHAR(3) NOT NULL,
        TipoCambioP DECIMAL(18,6) NULL,
        Monto DECIMAL(18,2) NOT NULL,
        NumOperacion VARCHAR(100) NULL,
        RfcEmisorCtaOrd VARCHAR(13) NULL,
        NomBancoOrdExt VARCHAR(300) NULL,
        CtaOrdenante VARCHAR(50) NULL,
        RfcEmisorCtaBen VARCHAR(13) NULL,
        CtaBeneficiario VARCHAR(50) NULL,
        TipoCadPago VARCHAR(2) NULL,
        CertPago VARCHAR(MAX) NULL,
        CadPago VARCHAR(MAX) NULL,
        SelloPago VARCHAR(MAX) NULL,
        CONSTRAINT FK_Pagos_Pago_Detalle FOREIGN KEY (ID_Pago_Detalle) 
            REFERENCES dbo.CFDI_Pagos_Detalle(ID_Pago_Detalle)
    );
    PRINT '✓ Tabla CFDI_Pagos_Pago creada';
END
ELSE
BEGIN
    PRINT '✓ Tabla CFDI_Pagos_Pago ya existe';
END
GO

-- 5. Crear tabla CFDI_Pagos_DoctoRelacionado si no existe
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CFDI_Pagos_DoctoRelacionado')
BEGIN
    PRINT 'Creando tabla CFDI_Pagos_DoctoRelacionado...';
    CREATE TABLE dbo.CFDI_Pagos_DoctoRelacionado (
        ID_Docto_Relacionado INT IDENTITY(1,1) PRIMARY KEY,
        ID_Pago INT NOT NULL,
        IdDocumento VARCHAR(36) NOT NULL,
        Serie VARCHAR(25) NULL,
        Folio VARCHAR(40) NULL,
        MonedaDR VARCHAR(3) NOT NULL,
        EquivalenciaDR DECIMAL(18,6) NULL,
        NumParcialidad INT NOT NULL,
        ImpSaldoAnt DECIMAL(18,2) NOT NULL,
        ImpPagado DECIMAL(18,2) NOT NULL,
        ImpSaldoInsoluto DECIMAL(18,2) NOT NULL,
        ObjetoImpDR VARCHAR(2) NULL,
        CONSTRAINT FK_Pagos_DoctoRel_Pago FOREIGN KEY (ID_Pago) 
            REFERENCES dbo.CFDI_Pagos_Pago(ID_Pago)
    );
    PRINT '✓ Tabla CFDI_Pagos_DoctoRelacionado creada';
END
ELSE
BEGIN
    PRINT '✓ Tabla CFDI_Pagos_DoctoRelacionado ya existe';
END
GO

PRINT '';
PRINT '╔════════════════════════════════════════════════════════════╗';
PRINT '║  ✅ ACTUALIZACIÓN COMPLETADA EXITOSAMENTE                 ║';
PRINT '╚════════════════════════════════════════════════════════════╝';
PRINT '';
GO
