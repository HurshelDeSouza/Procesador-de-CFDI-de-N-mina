USE DescargaCfdiGFP;
GO

-- Recrear tablas de Pagos con estructura correcta
CREATE TABLE dbo.CFDI_Pagos_Detalle (
    ID_Comprobante INT PRIMARY KEY,
    FormaDePago VARCHAR(2) NULL,
    MontoTotalPagos DECIMAL(18,2) NOT NULL,
    TotalTrasladosBaseIva16 DECIMAL(18,2) NULL,
    TotalTrasladosImpuestoIva16 DECIMAL(18,2) NULL,
    CONSTRAINT FK_Pagos_Detalle_Comprobante FOREIGN KEY (ID_Comprobante) 
        REFERENCES dbo.CFDI_Comprobante(ID_Comprobante)
);

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

PRINT '✓ Tablas de Pagos recreadas correctamente';
GO
