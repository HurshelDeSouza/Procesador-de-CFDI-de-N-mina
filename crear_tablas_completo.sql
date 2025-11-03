USE DescargaCfdiGFP;
GO

-- Crear tablas base
CREATE TABLE dbo.CFDI_Concepto (
	ID_Concepto int IDENTITY(1,1) NOT NULL,
	ID_Comprobante int NOT NULL,
	ClaveProdServ varchar(8) NOT NULL,
	Cantidad decimal(18,6) NOT NULL,
	ClaveUnidad varchar(3) NOT NULL,
	Unidad varchar(20) NULL,
	Descripcion varchar(MAX) NOT NULL,
	ValorUnitario decimal(18,6) NOT NULL,
	Importe decimal(18,2) NOT NULL,
	Descuento decimal(18,2) NULL,
	NoIdentificacion varchar(50) NULL,
	ObjetoImp varchar(2) NULL,
	CONSTRAINT PK_CFDI_Concepto PRIMARY KEY (ID_Concepto),
	CONSTRAINT FK_CFDI_Concepto_Comprobante FOREIGN KEY (ID_Comprobante) REFERENCES dbo.CFDI_Comprobante(ID_Comprobante)
);

CREATE TABLE dbo.CFDI_Emisor (
	ID_Comprobante int NOT NULL,
	Rfc varchar(13) NOT NULL,
	Nombre varchar(255) NOT NULL,
	RegimenFiscal varchar(3) NOT NULL,
	RegistroPatronal varchar(20) NULL,
	CONSTRAINT PK_CFDI_Emisor PRIMARY KEY (ID_Comprobante),
	CONSTRAINT FK_CFDI_Emisor_Comprobante FOREIGN KEY (ID_Comprobante) REFERENCES dbo.CFDI_Comprobante(ID_Comprobante)
);

CREATE TABLE dbo.CFDI_Receptor (
	ID_Comprobante int NOT NULL,
	Rfc varchar(13) NOT NULL,
	Nombre varchar(255) NOT NULL,
	DomicilioFiscalReceptor varchar(5) NOT NULL,
	RegimenFiscalReceptor varchar(3) NOT NULL,
	UsoCFDI varchar(4) NOT NULL,
	NumRegIdTrib varchar(40) NULL,
	ResidenciaFiscal varchar(5) NULL,
	CONSTRAINT PK_CFDI_Receptor PRIMARY KEY (ID_Comprobante),
	CONSTRAINT FK_CFDI_Receptor_Comprobante FOREIGN KEY (ID_Comprobante) REFERENCES dbo.CFDI_Comprobante(ID_Comprobante)
);

CREATE TABLE dbo.CFDI_TrasladoConcepto (
	ID_Traslado int IDENTITY(1,1) NOT NULL,
	ID_Concepto int NOT NULL,
	Base decimal(18,6) NOT NULL,
	Impuesto varchar(3) NOT NULL,
	TipoFactor varchar(8) NOT NULL,
	TasaOCuota decimal(18,6) NOT NULL,
	Importe decimal(18,2) NULL,
	CONSTRAINT PK_CFDI_TrasladoConcepto PRIMARY KEY (ID_Traslado),
	CONSTRAINT FK_CFDI_TrasladoConcepto_Concepto FOREIGN KEY (ID_Concepto) REFERENCES dbo.CFDI_Concepto(ID_Concepto)
);

CREATE TABLE dbo.CFDI_RetencionConcepto (
	ID_Retencion int IDENTITY(1,1) NOT NULL,
	ID_Concepto int NOT NULL,
	Base decimal(18,6) NOT NULL,
	Impuesto varchar(3) NOT NULL,
	TipoFactor varchar(8) NOT NULL,
	TasaOCuota decimal(18,6) NOT NULL,
	Importe decimal(18,2) NOT NULL,
	CONSTRAINT PK_CFDI_RetencionConcepto PRIMARY KEY (ID_Retencion),
	CONSTRAINT FK_CFDI_RetencionConcepto_Concepto FOREIGN KEY (ID_Concepto) REFERENCES dbo.CFDI_Concepto(ID_Concepto)
);

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

PRINT '✓ Todas las tablas creadas exitosamente';
GO
