-- Script para ejecutar facturas.sql correctamente
USE DescargaCfdiGFP;
GO

-- No crear schema, ya existe por defecto
-- CREATE SCHEMA dbo;

-- DescargaCfdiGFP.dbo.CFDI_Comprobante definition
CREATE TABLE dbo.CFDI_Comprobante (
	ID_Comprobante int IDENTITY(1,1) NOT NULL,
	UUID varchar(36) COLLATE Modern_Spanish_CI_AS NOT NULL,
	Version varchar(5) COLLATE Modern_Spanish_CI_AS NOT NULL,
	Serie varchar(25) COLLATE Modern_Spanish_CI_AS NULL,
	Folio varchar(40) COLLATE Modern_Spanish_CI_AS NULL,
	Fecha datetime2 NOT NULL,
	FechaTimbrado datetime2 NULL,
	TipoDeComprobante char(1) COLLATE Modern_Spanish_CI_AS NOT NULL,
	LugarExpedicion varchar(5) COLLATE Modern_Spanish_CI_AS NOT NULL,
	Moneda varchar(3) COLLATE Modern_Spanish_CI_AS NOT NULL,
	TipoCambio decimal(18,6) NULL,
	Total decimal(18,2) NOT NULL,
	SubTotal decimal(18,2) NOT NULL,
	Descuento decimal(18,2) NULL,
	MetodoPago varchar(3) COLLATE Modern_Spanish_CI_AS NULL,
	FormaPago varchar(2) COLLATE Modern_Spanish_CI_AS NULL,
	Estatus varchar(20) COLLATE Modern_Spanish_CI_AS NULL,
	SelloDigital varchar(MAX) COLLATE Modern_Spanish_CI_AS NULL,
	Certificado varchar(20) COLLATE Modern_Spanish_CI_AS NULL,
	EmitidaRecibida char(1) COLLATE Modern_Spanish_CI_AS DEFAULT 'E' NOT NULL,
	CONSTRAINT PK__CFDI_Com__7DE63600EA946DEA PRIMARY KEY (ID_Comprobante),
	CONSTRAINT UQ__CFDI_Com__65A475E687655BAE UNIQUE (UUID)
);

ALTER TABLE dbo.CFDI_Comprobante ADD CONSTRAINT CHK_CFDI_Total_NonNegative CHECK (([Total]>=(0)));
ALTER TABLE dbo.CFDI_Comprobante ADD CONSTRAINT CHK_CFDI_SubTotal_NonNegative CHECK (([SubTotal]>=(0)));
ALTER TABLE dbo.CFDI_Comprobante ADD CONSTRAINT CHK_CFDI_TipoDeComprobante_Values CHECK (([TipoDeComprobante]='N' OR [TipoDeComprobante]='T' OR [TipoDeComprobante]='E' OR [TipoDeComprobante]='I' OR [TipoDeComprobante]='P'));
ALTER TABLE dbo.CFDI_Comprobante ADD CONSTRAINT CHK_CFDI_EmitidaRecibida_Values CHECK (([EmitidaRecibida]='R' OR [EmitidaRecibida]='E'));
GO

PRINT '✓ Tabla CFDI_Comprobante creada';
GO
