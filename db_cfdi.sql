-- DROP SCHEMA dbo;

CREATE SCHEMA dbo;
-- DescargaCfdiGFP.dbo.CFDI_Comprobante definition

-- Drop table

-- DROP TABLE DescargaCfdiGFP.dbo.CFDI_Comprobante;

CREATE TABLE DescargaCfdiGFP.dbo.CFDI_Comprobante (
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
	CONSTRAINT PK__CFDI_Com__7DE63600EA946DEA PRIMARY KEY (ID_Comprobante),
	CONSTRAINT UQ__CFDI_Com__65A475E687655BAE UNIQUE (UUID)
);


-- DescargaCfdiGFP.dbo.CFDI_Concepto definition

-- Drop table

-- DROP TABLE DescargaCfdiGFP.dbo.CFDI_Concepto;

CREATE TABLE DescargaCfdiGFP.dbo.CFDI_Concepto (
	ID_Concepto int IDENTITY(1,1) NOT NULL,
	ID_Comprobante int NOT NULL,
	ClaveProdServ varchar(8) COLLATE Modern_Spanish_CI_AS NOT NULL,
	Cantidad decimal(18,6) NOT NULL,
	ClaveUnidad varchar(3) COLLATE Modern_Spanish_CI_AS NOT NULL,
	Unidad varchar(20) COLLATE Modern_Spanish_CI_AS NULL,
	Descripcion varchar(MAX) COLLATE Modern_Spanish_CI_AS NOT NULL,
	ValorUnitario decimal(18,6) NOT NULL,
	Importe decimal(18,2) NOT NULL,
	Descuento decimal(18,2) NULL,
	NoIdentificacion varchar(50) COLLATE Modern_Spanish_CI_AS NULL,
	ObjetoImp varchar(2) COLLATE Modern_Spanish_CI_AS NULL,
	CONSTRAINT PK__CFDI_Con__3D604791D9892870 PRIMARY KEY (ID_Concepto),
	CONSTRAINT FK__CFDI_Conc__ID_Co__3F466844 FOREIGN KEY (ID_Comprobante) REFERENCES DescargaCfdiGFP.dbo.CFDI_Comprobante(ID_Comprobante)
);


-- DescargaCfdiGFP.dbo.CFDI_Emisor definition

-- Drop table

-- DROP TABLE DescargaCfdiGFP.dbo.CFDI_Emisor;

CREATE TABLE DescargaCfdiGFP.dbo.CFDI_Emisor (
	ID_Comprobante int NOT NULL,
	Rfc varchar(13) COLLATE Modern_Spanish_CI_AS NOT NULL,
	Nombre varchar(255) COLLATE Modern_Spanish_CI_AS NOT NULL,
	RegimenFiscal varchar(3) COLLATE Modern_Spanish_CI_AS NOT NULL,
	RegistroPatronal varchar(20) COLLATE Modern_Spanish_CI_AS NULL,
	CONSTRAINT PK__CFDI_Emi__7DE636003DE64ACD PRIMARY KEY (ID_Comprobante),
	CONSTRAINT FK__CFDI_Emis__ID_Co__398D8EEE FOREIGN KEY (ID_Comprobante) REFERENCES DescargaCfdiGFP.dbo.CFDI_Comprobante(ID_Comprobante)
);


-- DescargaCfdiGFP.dbo.CFDI_Receptor definition

-- Drop table

-- DROP TABLE DescargaCfdiGFP.dbo.CFDI_Receptor;

CREATE TABLE DescargaCfdiGFP.dbo.CFDI_Receptor (
	ID_Comprobante int NOT NULL,
	Rfc varchar(13) COLLATE Modern_Spanish_CI_AS NOT NULL,
	Nombre varchar(255) COLLATE Modern_Spanish_CI_AS NOT NULL,
	DomicilioFiscalReceptor varchar(5) COLLATE Modern_Spanish_CI_AS NOT NULL,
	RegimenFiscalReceptor varchar(3) COLLATE Modern_Spanish_CI_AS NOT NULL,
	UsoCFDI varchar(3) COLLATE Modern_Spanish_CI_AS NOT NULL,
	NumRegIdTrib varchar(40) COLLATE Modern_Spanish_CI_AS NULL,
	ResidenciaFiscal varchar(5) COLLATE Modern_Spanish_CI_AS NULL,
	CONSTRAINT PK__CFDI_Rec__7DE63600FAF23403 PRIMARY KEY (ID_Comprobante),
	CONSTRAINT FK__CFDI_Rece__ID_Co__3C69FB99 FOREIGN KEY (ID_Comprobante) REFERENCES DescargaCfdiGFP.dbo.CFDI_Comprobante(ID_Comprobante)
);


-- DescargaCfdiGFP.dbo.CFDI_RetencionConcepto definition

-- Drop table

-- DROP TABLE DescargaCfdiGFP.dbo.CFDI_RetencionConcepto;

CREATE TABLE DescargaCfdiGFP.dbo.CFDI_RetencionConcepto (
	ID_Retencion int IDENTITY(1,1) NOT NULL,
	ID_Concepto int NOT NULL,
	Base decimal(18,6) NOT NULL,
	Impuesto varchar(3) COLLATE Modern_Spanish_CI_AS NOT NULL,
	TipoFactor varchar(8) COLLATE Modern_Spanish_CI_AS NOT NULL,
	TasaOCuota decimal(18,6) NOT NULL,
	Importe decimal(18,2) NOT NULL,
	CONSTRAINT PK__CFDI_Ret__9A009EBEDAAC0A0B PRIMARY KEY (ID_Retencion),
	CONSTRAINT FK__CFDI_Rete__ID_Co__44FF419A FOREIGN KEY (ID_Concepto) REFERENCES DescargaCfdiGFP.dbo.CFDI_Concepto(ID_Concepto)
);


-- DescargaCfdiGFP.dbo.CFDI_TrasladoConcepto definition

-- Drop table

-- DROP TABLE DescargaCfdiGFP.dbo.CFDI_TrasladoConcepto;

CREATE TABLE DescargaCfdiGFP.dbo.CFDI_TrasladoConcepto (
	ID_Traslado int IDENTITY(1,1) NOT NULL,
	ID_Concepto int NOT NULL,
	Base decimal(18,6) NOT NULL,
	Impuesto varchar(3) COLLATE Modern_Spanish_CI_AS NOT NULL,
	TipoFactor varchar(8) COLLATE Modern_Spanish_CI_AS NOT NULL,
	TasaOCuota decimal(18,6) NOT NULL,
	Importe decimal(18,2) NULL,
	CONSTRAINT PK__CFDI_Tra__8F674787AA91C027 PRIMARY KEY (ID_Traslado),
	CONSTRAINT FK__CFDI_Tras__ID_Co__4222D4EF FOREIGN KEY (ID_Concepto) REFERENCES DescargaCfdiGFP.dbo.CFDI_Concepto(ID_Concepto)
);


-- DescargaCfdiGFP.dbo.ComercioExterior_Detalle definition

-- Drop table

-- DROP TABLE DescargaCfdiGFP.dbo.ComercioExterior_Detalle;

CREATE TABLE DescargaCfdiGFP.dbo.ComercioExterior_Detalle (
	ID_Comprobante int NOT NULL,
	Incoterm varchar(3) COLLATE Modern_Spanish_CI_AS NOT NULL,
	ClaveDePedimento varchar(2) COLLATE Modern_Spanish_CI_AS NULL,
	TipoCambioUSD decimal(18,6) NOT NULL,
	TotalUSD decimal(18,2) NOT NULL,
	CONSTRAINT PK__Comercio__7DE63600AA95610B PRIMARY KEY (ID_Comprobante),
	CONSTRAINT FK__ComercioE__ID_Co__5BE2A6F2 FOREIGN KEY (ID_Comprobante) REFERENCES DescargaCfdiGFP.dbo.CFDI_Comprobante(ID_Comprobante)
);


-- DescargaCfdiGFP.dbo.ComercioExterior_Domicilio definition

-- Drop table

-- DROP TABLE DescargaCfdiGFP.dbo.ComercioExterior_Domicilio;

CREATE TABLE DescargaCfdiGFP.dbo.ComercioExterior_Domicilio (
	ID_Domicilio int IDENTITY(1,1) NOT NULL,
	ID_Comprobante int NOT NULL,
	TipoDomicilio varchar(15) COLLATE Modern_Spanish_CI_AS NOT NULL,
	Calle varchar(100) COLLATE Modern_Spanish_CI_AS NULL,
	CodigoPostal varchar(10) COLLATE Modern_Spanish_CI_AS NULL,
	Colonia varchar(100) COLLATE Modern_Spanish_CI_AS NULL,
	Estado varchar(30) COLLATE Modern_Spanish_CI_AS NULL,
	Localidad varchar(50) COLLATE Modern_Spanish_CI_AS NULL,
	Municipio varchar(50) COLLATE Modern_Spanish_CI_AS NULL,
	NumeroExterior varchar(20) COLLATE Modern_Spanish_CI_AS NULL,
	Pais varchar(3) COLLATE Modern_Spanish_CI_AS NOT NULL,
	CONSTRAINT PK__Comercio__24312D3DEF9C3076 PRIMARY KEY (ID_Domicilio),
	CONSTRAINT FK__ComercioE__ID_Co__5EBF139D FOREIGN KEY (ID_Comprobante) REFERENCES DescargaCfdiGFP.dbo.CFDI_Comprobante(ID_Comprobante)
);


-- DescargaCfdiGFP.dbo.ComercioExterior_Mercancia definition

-- Drop table

-- DROP TABLE DescargaCfdiGFP.dbo.ComercioExterior_Mercancia;

CREATE TABLE DescargaCfdiGFP.dbo.ComercioExterior_Mercancia (
	ID_Mercancia int IDENTITY(1,1) NOT NULL,
	ID_Comprobante int NOT NULL,
	NoIdentificacion varchar(50) COLLATE Modern_Spanish_CI_AS NOT NULL,
	FraccionArancelaria varchar(10) COLLATE Modern_Spanish_CI_AS NOT NULL,
	UnidadAduana varchar(3) COLLATE Modern_Spanish_CI_AS NOT NULL,
	CantidadAduana decimal(18,6) NOT NULL,
	ValorDolares decimal(18,2) NOT NULL,
	ValorUnitarioAduana decimal(18,6) NULL,
	Descripcion varchar(255) COLLATE Modern_Spanish_CI_AS NULL,
	UnidadMedida varchar(10) COLLATE Modern_Spanish_CI_AS NULL,
	CONSTRAINT PK__Comercio__25F3CDDAC8C24915 PRIMARY KEY (ID_Mercancia),
	CONSTRAINT FK__ComercioE__ID_Co__619B8048 FOREIGN KEY (ID_Comprobante) REFERENCES DescargaCfdiGFP.dbo.CFDI_Comprobante(ID_Comprobante)
);


-- DescargaCfdiGFP.dbo.Nomina_Deducciones definition

-- Drop table

-- DROP TABLE DescargaCfdiGFP.dbo.Nomina_Deducciones;

CREATE TABLE DescargaCfdiGFP.dbo.Nomina_Deducciones (
	ID_Deduccion int IDENTITY(1,1) NOT NULL,
	ID_Comprobante int NOT NULL,
	TipoDeduccion varchar(3) COLLATE Modern_Spanish_CI_AS NOT NULL,
	Clave varchar(3) COLLATE Modern_Spanish_CI_AS NOT NULL,
	Concepto varchar(255) COLLATE Modern_Spanish_CI_AS NOT NULL,
	Importe decimal(18,2) NOT NULL,
	CONSTRAINT PK__Nomina_D__7CA6361AD5046F72 PRIMARY KEY (ID_Deduccion),
	CONSTRAINT FK__Nomina_De__ID_Co__4D94879B FOREIGN KEY (ID_Comprobante) REFERENCES DescargaCfdiGFP.dbo.CFDI_Comprobante(ID_Comprobante)
);


-- DescargaCfdiGFP.dbo.Nomina_Detalle definition

-- Drop table

-- DROP TABLE DescargaCfdiGFP.dbo.Nomina_Detalle;

CREATE TABLE DescargaCfdiGFP.dbo.Nomina_Detalle (
	ID_Comprobante int NOT NULL,
	TipoNomina char(1) COLLATE Modern_Spanish_CI_AS NOT NULL,
	FechaPago date NOT NULL,
	FechaInicialPago date NOT NULL,
	FechaFinalPago date NOT NULL,
	NumDiasPagados decimal(18,3) NOT NULL,
	TotalPercepciones decimal(18,2) NULL,
	TotalDeducciones decimal(18,2) NULL,
	TotalOtrosPagos decimal(18,2) NULL,
	CurpEmpleado varchar(18) COLLATE Modern_Spanish_CI_AS NOT NULL,
	NumSeguridadSocial varchar(15) COLLATE Modern_Spanish_CI_AS NULL,
	FechaInicioRelLaboral date NULL,
	Antiguedad varchar(10) COLLATE Modern_Spanish_CI_AS NULL,
	TipoContrato varchar(2) COLLATE Modern_Spanish_CI_AS NULL,
	NumEmpleado varchar(15) COLLATE Modern_Spanish_CI_AS NULL,
	Departamento varchar(100) COLLATE Modern_Spanish_CI_AS NULL,
	Puesto varchar(100) COLLATE Modern_Spanish_CI_AS NULL,
	SalarioBaseCotApor decimal(18,2) NULL,
	SalarioDiarioIntegrado decimal(18,2) NULL,
	PeriodicidadPago varchar(2) COLLATE Modern_Spanish_CI_AS NULL,
	CuentaBancaria varchar(20) COLLATE Modern_Spanish_CI_AS NULL,
	ClaveEntFed varchar(2) COLLATE Modern_Spanish_CI_AS NULL,
	CONSTRAINT PK__Nomina_D__7DE63600D33A1D84 PRIMARY KEY (ID_Comprobante),
	CONSTRAINT FK__Nomina_De__ID_Co__47DBAE45 FOREIGN KEY (ID_Comprobante) REFERENCES DescargaCfdiGFP.dbo.CFDI_Comprobante(ID_Comprobante)
);


-- DescargaCfdiGFP.dbo.Nomina_OtrosPagos definition

-- Drop table

-- DROP TABLE DescargaCfdiGFP.dbo.Nomina_OtrosPagos;

CREATE TABLE DescargaCfdiGFP.dbo.Nomina_OtrosPagos (
	ID_OtroPago int IDENTITY(1,1) NOT NULL,
	ID_Comprobante int NOT NULL,
	TipoOtroPago varchar(3) COLLATE Modern_Spanish_CI_AS NOT NULL,
	Clave varchar(3) COLLATE Modern_Spanish_CI_AS NOT NULL,
	Concepto varchar(255) COLLATE Modern_Spanish_CI_AS NOT NULL,
	Importe decimal(18,2) NOT NULL,
	SubsidioCausado decimal(18,2) NULL,
	CONSTRAINT PK__Nomina_O__F43ECFD58912DB59 PRIMARY KEY (ID_OtroPago),
	CONSTRAINT FK__Nomina_Ot__ID_Co__5070F446 FOREIGN KEY (ID_Comprobante) REFERENCES DescargaCfdiGFP.dbo.CFDI_Comprobante(ID_Comprobante)
);


-- DescargaCfdiGFP.dbo.Nomina_Percepciones definition

-- Drop table

-- DROP TABLE DescargaCfdiGFP.dbo.Nomina_Percepciones;

CREATE TABLE DescargaCfdiGFP.dbo.Nomina_Percepciones (
	ID_Percepcion int IDENTITY(1,1) NOT NULL,
	ID_Comprobante int NOT NULL,
	TipoPercepcion varchar(3) COLLATE Modern_Spanish_CI_AS NOT NULL,
	Clave varchar(3) COLLATE Modern_Spanish_CI_AS NOT NULL,
	Concepto varchar(255) COLLATE Modern_Spanish_CI_AS NOT NULL,
	ImporteGravado decimal(18,2) NULL,
	ImporteExento decimal(18,2) NULL,
	CONSTRAINT PK__Nomina_P__64652ED4EE4C50C0 PRIMARY KEY (ID_Percepcion),
	CONSTRAINT FK__Nomina_Pe__ID_Co__4AB81AF0 FOREIGN KEY (ID_Comprobante) REFERENCES DescargaCfdiGFP.dbo.CFDI_Comprobante(ID_Comprobante)
);


-- DescargaCfdiGFP.dbo.Pagos_Detalle definition

-- Drop table

-- DROP TABLE DescargaCfdiGFP.dbo.Pagos_Detalle;

CREATE TABLE DescargaCfdiGFP.dbo.Pagos_Detalle (
	ID_Comprobante int NOT NULL,
	FormaDePago varchar(2) COLLATE Modern_Spanish_CI_AS NULL,
	MontoTotalPagos decimal(18,2) NOT NULL,
	TotalTrasladosBaseIVA16 decimal(18,2) NULL,
	TotalTrasladosImpuestoIVA16 decimal(18,2) NULL,
	CONSTRAINT PK__Pagos_De__7DE63600F52BA6BE PRIMARY KEY (ID_Comprobante),
	CONSTRAINT FK__Pagos_Det__ID_Co__534D60F1 FOREIGN KEY (ID_Comprobante) REFERENCES DescargaCfdiGFP.dbo.CFDI_Comprobante(ID_Comprobante)
);


-- DescargaCfdiGFP.dbo.Pagos_Pago definition

-- Drop table

-- DROP TABLE DescargaCfdiGFP.dbo.Pagos_Pago;

CREATE TABLE DescargaCfdiGFP.dbo.Pagos_Pago (
	ID_Pago int IDENTITY(1,1) NOT NULL,
	ID_Comprobante int NOT NULL,
	FechaPago datetime2 NOT NULL,
	FormaDePagoP varchar(2) COLLATE Modern_Spanish_CI_AS NOT NULL,
	MonedaP varchar(3) COLLATE Modern_Spanish_CI_AS NOT NULL,
	TipoCambioP decimal(18,6) NULL,
	Monto decimal(18,2) NOT NULL,
	NumOperacion varchar(100) COLLATE Modern_Spanish_CI_AS NULL,
	CONSTRAINT PK__Pagos_Pa__AE88B429F325A7A9 PRIMARY KEY (ID_Pago),
	CONSTRAINT FK__Pagos_Pag__ID_Co__5629CD9C FOREIGN KEY (ID_Comprobante) REFERENCES DescargaCfdiGFP.dbo.CFDI_Comprobante(ID_Comprobante)
);


-- DescargaCfdiGFP.dbo.Pagos_DoctoRelacionado definition

-- Drop table

-- DROP TABLE DescargaCfdiGFP.dbo.Pagos_DoctoRelacionado;

CREATE TABLE DescargaCfdiGFP.dbo.Pagos_DoctoRelacionado (
	ID_DoctoRel int IDENTITY(1,1) NOT NULL,
	ID_Pago int NOT NULL,
	IdDocumento varchar(36) COLLATE Modern_Spanish_CI_AS NOT NULL,
	Serie varchar(25) COLLATE Modern_Spanish_CI_AS NULL,
	Folio varchar(40) COLLATE Modern_Spanish_CI_AS NULL,
	MonedaDR varchar(3) COLLATE Modern_Spanish_CI_AS NOT NULL,
	EquivalenciaDR decimal(18,6) NULL,
	NumParcialidad int NULL,
	ImpSaldoAnt decimal(18,2) NOT NULL,
	ImpPagado decimal(18,2) NOT NULL,
	ImpSaldoInsoluto decimal(18,2) NOT NULL,
	ObjetoImpDR varchar(2) COLLATE Modern_Spanish_CI_AS NULL,
	ObjetoImp varchar(2) COLLATE Modern_Spanish_CI_AS NULL,
	CONSTRAINT PK__Pagos_Do__357D1442DEE9E205 PRIMARY KEY (ID_DoctoRel),
	CONSTRAINT FK__Pagos_Doc__ID_Pa__59063A47 FOREIGN KEY (ID_Pago) REFERENCES DescargaCfdiGFP.dbo.Pagos_Pago(ID_Pago)
);


