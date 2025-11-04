using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using CFDIProcessor.Constants;
using CFDIProcessor.Data;
using CFDIProcessor.Helpers;
using CFDIProcessor.Models;
using Microsoft.Extensions.Configuration;

namespace CFDIProcessor.Services
{
    /// <summary>
    /// Procesador unificado que determina automáticamente el tipo de CFDI y lo procesa
    /// </summary>
    public class UnifiedCfdiProcessor
    {
        private readonly DescargaCfdiGfpContext _context;
        private readonly IConfiguration _configuration;
        private static readonly XNamespace Pagos20 = "http://www.sat.gob.mx/Pagos20";

        // Configuración de procesamiento
        private readonly bool _processNomina;
        private readonly bool _processIngreso;
        private readonly bool _processEgreso;
        private readonly bool _processPagos;

        public UnifiedCfdiProcessor(DescargaCfdiGfpContext context, IConfiguration configuration)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            // Cargar configuración
            _processNomina = _configuration.GetValue<bool>("ProcessingSettings:ProcessNomina", true);
            _processIngreso = _configuration.GetValue<bool>("ProcessingSettings:ProcessIngreso", true);
            _processEgreso = _configuration.GetValue<bool>("ProcessingSettings:ProcessEgreso", true);
            _processPagos = _configuration.GetValue<bool>("ProcessingSettings:ProcessPagos", true);
        }

        /// <summary>
        /// Procesa todos los archivos XML de una carpeta, detectando automáticamente el tipo
        /// </summary>
        public void ProcessXmlFilesFromFolder(string folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath))
                throw new ArgumentException("La ruta de la carpeta no puede estar vacía", nameof(folderPath));

            if (!Directory.Exists(folderPath))
            {
                Console.WriteLine($"Error: La carpeta '{folderPath}' no existe.");
                return;
            }

            // Mostrar configuración activa
            Console.WriteLine("=== Configuración de Procesamiento ===");
            Console.WriteLine($"Nómina: {(_processNomina ? "✓ Activado" : "✗ Desactivado")}");
            Console.WriteLine($"Ingreso: {(_processIngreso ? "✓ Activado" : "✗ Desactivado")}");
            Console.WriteLine($"Egreso: {(_processEgreso ? "✓ Activado" : "✗ Desactivado")}");
            Console.WriteLine($"Pagos: {(_processPagos ? "✓ Activado" : "✗ Desactivado")}");
            Console.WriteLine();

            var xmlFiles = Directory.GetFiles(folderPath, "*.xml");
            Console.WriteLine($"Se encontraron {xmlFiles.Length} archivo(s) XML.");
            Console.WriteLine();

            int processedNomina = 0, processedIngreso = 0, processedEgreso = 0, processedPagos = 0;
            int skippedCount = 0, errorCount = 0;

            foreach (var xmlFile in xmlFiles)
            {
                try
                {
                    var result = ProcessXmlFile(xmlFile);
                    
                    switch (result)
                    {
                        case ProcessResult.ProcessedNomina:
                            processedNomina++;
                            break;
                        case ProcessResult.ProcessedIngreso:
                            processedIngreso++;
                            break;
                        case ProcessResult.ProcessedEgreso:
                            processedEgreso++;
                            break;
                        case ProcessResult.ProcessedPagos:
                            processedPagos++;
                            break;
                        case ProcessResult.Skipped:
                            skippedCount++;
                            break;
                    }
                }
                catch (Exception ex)
                {
                    errorCount++;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"✗ Error procesando {Path.GetFileName(xmlFile)}: {ex.Message}");
                    Console.ResetColor();
                    
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"  Detalle: {ex.InnerException.Message}");
                    }
                }
            }

            // Resumen
            Console.WriteLine();
            Console.WriteLine("=== Resumen del Procesamiento ===");
            Console.ForegroundColor = ConsoleColor.Green;
            if (processedNomina > 0) Console.WriteLine($"✓ Nómina procesados: {processedNomina}");
            if (processedIngreso > 0) Console.WriteLine($"✓ Ingreso procesados: {processedIngreso}");
            if (processedEgreso > 0) Console.WriteLine($"✓ Egreso procesados: {processedEgreso}");
            if (processedPagos > 0) Console.WriteLine($"✓ Pagos procesados: {processedPagos}");
            Console.ResetColor();
            
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"⊘ Omitidos (duplicados, desactivados o no válidos): {skippedCount}");
            Console.ResetColor();
            
            if (errorCount > 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"✗ Errores: {errorCount}");
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Procesa un archivo XML individual, detectando su tipo automáticamente
        /// </summary>
        private ProcessResult ProcessXmlFile(string xmlFilePath)
        {
            XDocument doc = XDocument.Load(xmlFilePath);
            var comprobanteElement = doc.Root;

            if (comprobanteElement == null)
            {
                Console.WriteLine($"⊘ {Path.GetFileName(xmlFilePath)}: XML inválido (sin elemento raíz)");
                return ProcessResult.Skipped;
            }

            // Obtener UUID
            var complemento = comprobanteElement.Element(XmlNamespaces.Cfdi + "Complemento");
            var timbre = complemento?.Element(XmlNamespaces.TimbreFiscalDigital + "TimbreFiscalDigital");
            string uuid = XmlHelper.GetAttributeValue(timbre, "UUID");

            if (string.IsNullOrEmpty(uuid))
            {
                Console.WriteLine($"⊘ {Path.GetFileName(xmlFilePath)}: No se encontró UUID");
                return ProcessResult.Skipped;
            }

            // Verificar duplicados
            if (_context.CfdiComprobante.Any(c => c.Uuid == uuid))
            {
                Console.WriteLine($"⊘ {Path.GetFileName(xmlFilePath)}: UUID {uuid} ya existe");
                return ProcessResult.Skipped;
            }

            // Determinar tipo de CFDI
            string tipoComprobante = XmlHelper.GetAttributeValue(comprobanteElement, "TipoDeComprobante");
            var nominaElement = complemento?.Element(XmlNamespaces.Nomina + "Nomina");
            var pagosElement = complemento?.Element(Pagos20 + "Pagos");

            // Procesar según el tipo detectado
            if (nominaElement != null && tipoComprobante == "N")
            {
                if (!_processNomina)
                {
                    Console.WriteLine($"⊘ {Path.GetFileName(xmlFilePath)}: Nómina desactivada en configuración");
                    return ProcessResult.Skipped;
                }
                return ProcessNomina(xmlFilePath, comprobanteElement, complemento, timbre, uuid, nominaElement);
            }
            else if (tipoComprobante == "P" && pagosElement != null)
            {
                if (!_processPagos)
                {
                    Console.WriteLine($"⊘ {Path.GetFileName(xmlFilePath)}: Pagos desactivado en configuración");
                    return ProcessResult.Skipped;
                }
                return ProcessPagos(xmlFilePath, comprobanteElement, complemento, timbre, uuid, pagosElement);
            }
            else if (tipoComprobante == "I")
            {
                if (!_processIngreso)
                {
                    Console.WriteLine($"⊘ {Path.GetFileName(xmlFilePath)}: Ingreso desactivado en configuración");
                    return ProcessResult.Skipped;
                }
                return ProcessIngresoEgreso(xmlFilePath, comprobanteElement, timbre, uuid, tipoComprobante);
            }
            else if (tipoComprobante == "E")
            {
                if (!_processEgreso)
                {
                    Console.WriteLine($"⊘ {Path.GetFileName(xmlFilePath)}: Egreso desactivado en configuración");
                    return ProcessResult.Skipped;
                }
                return ProcessIngresoEgreso(xmlFilePath, comprobanteElement, timbre, uuid, tipoComprobante);
            }
            else
            {
                Console.WriteLine($"⊘ {Path.GetFileName(xmlFilePath)}: Tipo de CFDI no soportado (Tipo: {tipoComprobante})");
                return ProcessResult.Skipped;
            }
        }

        #region Procesamiento de Nómina

        private ProcessResult ProcessNomina(string xmlFilePath, XElement comprobanteElement, 
            XElement complemento, XElement timbre, string uuid, XElement nominaElement)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var comprobante = CreateComprobanteNomina(comprobanteElement, timbre, uuid);
                    _context.CfdiComprobante.Add(comprobante);
                    _context.SaveChanges();

                    ProcessEmisorNomina(comprobanteElement, nominaElement, comprobante.IdComprobante);
                    ProcessReceptor(comprobanteElement, comprobante.IdComprobante);
                    ProcessConceptos(comprobanteElement, comprobante.IdComprobante);
                    ProcessNominaDetalle(nominaElement, comprobante.IdComprobante);
                    ProcessPercepciones(nominaElement, comprobante.IdComprobante);
                    ProcessDeducciones(nominaElement, comprobante.IdComprobante);
                    ProcessOtrosPagos(nominaElement, comprobante.IdComprobante);

                    _context.SaveChanges();
                    transaction.Commit();

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"✓ {Path.GetFileName(xmlFilePath)}: Nómina procesada (UUID: {uuid})");
                    Console.ResetColor();

                    return ProcessResult.ProcessedNomina;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        private CfdiComprobante CreateComprobanteNomina(XElement comprobanteElement, XElement timbre, string uuid)
        {
            return new CfdiComprobante
            {
                Uuid = uuid,
                Version = XmlHelper.GetAttributeValue(comprobanteElement, "Version") ?? "4.0",
                Serie = XmlHelper.GetAttributeValue(comprobanteElement, "Serie"),
                Folio = XmlHelper.GetAttributeValue(comprobanteElement, "Folio"),
                Fecha = XmlHelper.ParseDateTimeRequired(
                    XmlHelper.GetAttributeValue(comprobanteElement, "Fecha"), "Fecha"),
                FechaTimbrado = XmlHelper.ParseDateTimeRequired(
                    XmlHelper.GetAttributeValue(timbre, "FechaTimbrado"), "FechaTimbrado"),
                TipoDeComprobante = "N",
                LugarExpedicion = XmlHelper.GetAttributeValue(comprobanteElement, "LugarExpedicion"),
                Moneda = XmlHelper.GetAttributeValue(comprobanteElement, "Moneda") ?? "MXN",
                TipoCambio = XmlHelper.ParseDecimalOrNull(
                    XmlHelper.GetAttributeValue(comprobanteElement, "TipoCambio")),
                Total = XmlHelper.ParseDecimalRequired(
                    XmlHelper.GetAttributeValue(comprobanteElement, "Total"), "Total"),
                SubTotal = XmlHelper.ParseDecimalRequired(
                    XmlHelper.GetAttributeValue(comprobanteElement, "SubTotal"), "SubTotal"),
                Descuento = XmlHelper.ParseDecimalOrNull(
                    XmlHelper.GetAttributeValue(comprobanteElement, "Descuento")),
                MetodoPago = XmlHelper.GetAttributeValue(comprobanteElement, "MetodoPago"),
                FormaPago = XmlHelper.GetAttributeValue(comprobanteElement, "FormaPago"),
                Estatus = "Vigente",
                SelloDigital = XmlHelper.GetAttributeValue(comprobanteElement, "Sello"),
                Certificado = XmlHelper.GetAttributeValue(comprobanteElement, "NoCertificado"),
                EmitidaRecibida = "E"
            };
        }

        private void ProcessEmisorNomina(XElement comprobanteElement, XElement nominaElement, int idComprobante)
        {
            var emisorElement = comprobanteElement.Element(XmlNamespaces.Cfdi + "Emisor");
            if (emisorElement == null) return;

            var emisor = new CfdiEmisor
            {
                IdComprobante = idComprobante,
                Rfc = XmlHelper.GetAttributeValue(emisorElement, "Rfc"),
                Nombre = XmlHelper.GetAttributeValue(emisorElement, "Nombre"),
                RegimenFiscal = XmlHelper.GetAttributeValue(emisorElement, "RegimenFiscal"),
                RegistroPatronal = XmlHelper.GetAttributeValue(nominaElement, "RegistroPatronal")
            };

            _context.CfdiEmisor.Add(emisor);
        }

        private void ProcessNominaDetalle(XElement nominaElement, int idComprobante)
        {
            var receptorNomina = nominaElement.Element(XmlNamespaces.Nomina + "Receptor");
            if (receptorNomina == null) return;

            var nominaDetalle = new NominaDetalle
            {
                IdComprobante = idComprobante,
                TipoNomina = XmlHelper.GetAttributeValue(nominaElement, "TipoNomina"),
                FechaPago = XmlHelper.ParseDateTimeRequired(
                    XmlHelper.GetAttributeValue(nominaElement, "FechaPago"), "FechaPago"),
                FechaInicialPago = XmlHelper.ParseDateTimeRequired(
                    XmlHelper.GetAttributeValue(nominaElement, "FechaInicialPago"), "FechaInicialPago"),
                FechaFinalPago = XmlHelper.ParseDateTimeRequired(
                    XmlHelper.GetAttributeValue(nominaElement, "FechaFinalPago"), "FechaFinalPago"),
                NumDiasPagados = XmlHelper.ParseDecimalRequired(
                    XmlHelper.GetAttributeValue(nominaElement, "NumDiasPagados"), "NumDiasPagados"),
                TotalPercepciones = XmlHelper.ParseDecimalOrNull(
                    XmlHelper.GetAttributeValue(nominaElement, "TotalPercepciones")),
                TotalDeducciones = XmlHelper.ParseDecimalOrNull(
                    XmlHelper.GetAttributeValue(nominaElement, "TotalDeducciones")),
                TotalOtrosPagos = XmlHelper.ParseDecimalOrNull(
                    XmlHelper.GetAttributeValue(nominaElement, "TotalOtrosPagos")),
                CurpEmpleado = XmlHelper.GetAttributeValue(receptorNomina, "Curp"),
                NumSeguridadSocial = XmlHelper.GetAttributeValue(receptorNomina, "NumSeguridadSocial"),
                FechaInicioRelLaboral = XmlHelper.ParseDateTimeOrNull(
                    XmlHelper.GetAttributeValue(receptorNomina, "FechaInicioRelLaboral")),
                Antiguedad = XmlHelper.GetAttributeValue(receptorNomina, "Antigüedad"),
                TipoContrato = XmlHelper.GetAttributeValue(receptorNomina, "TipoContrato"),
                NumEmpleado = XmlHelper.GetAttributeValue(receptorNomina, "NumEmpleado"),
                Departamento = XmlHelper.GetAttributeValue(receptorNomina, "Departamento"),
                Puesto = XmlHelper.GetAttributeValue(receptorNomina, "Puesto"),
                SalarioBaseCotApor = XmlHelper.ParseDecimalOrNull(
                    XmlHelper.GetAttributeValue(receptorNomina, "SalarioBaseCotApor")),
                SalarioDiarioIntegrado = XmlHelper.ParseDecimalOrNull(
                    XmlHelper.GetAttributeValue(receptorNomina, "SalarioDiarioIntegrado")),
                PeriodicidadPago = XmlHelper.GetAttributeValue(receptorNomina, "PeriodicidadPago"),
                CuentaBancaria = XmlHelper.GetAttributeValue(receptorNomina, "CuentaBancaria"),
                ClaveEntFed = XmlHelper.GetAttributeValue(receptorNomina, "ClaveEntFed")
            };

            _context.NominaDetalle.Add(nominaDetalle);
        }

        private void ProcessPercepciones(XElement nominaElement, int idComprobante)
        {
            var percepcionesElement = nominaElement.Element(XmlNamespaces.Nomina + "Percepciones");
            if (percepcionesElement == null) return;

            foreach (var percepcionElement in percepcionesElement.Elements(XmlNamespaces.Nomina + "Percepcion"))
            {
                var percepcion = new NominaPercepciones
                {
                    IdComprobante = idComprobante,
                    TipoPercepcion = XmlHelper.GetAttributeValue(percepcionElement, "TipoPercepcion"),
                    Clave = XmlHelper.GetAttributeValue(percepcionElement, "Clave"),
                    Concepto = XmlHelper.GetAttributeValue(percepcionElement, "Concepto"),
                    ImporteGravado = XmlHelper.ParseDecimalOrNull(
                        XmlHelper.GetAttributeValue(percepcionElement, "ImporteGravado")),
                    ImporteExento = XmlHelper.ParseDecimalOrNull(
                        XmlHelper.GetAttributeValue(percepcionElement, "ImporteExento"))
                };

                _context.NominaPercepciones.Add(percepcion);
            }
        }

        private void ProcessDeducciones(XElement nominaElement, int idComprobante)
        {
            var deduccionesElement = nominaElement.Element(XmlNamespaces.Nomina + "Deducciones");
            if (deduccionesElement == null) return;

            foreach (var deduccionElement in deduccionesElement.Elements(XmlNamespaces.Nomina + "Deduccion"))
            {
                var deduccion = new NominaDeducciones
                {
                    IdComprobante = idComprobante,
                    TipoDeduccion = XmlHelper.GetAttributeValue(deduccionElement, "TipoDeduccion"),
                    Clave = XmlHelper.GetAttributeValue(deduccionElement, "Clave"),
                    Concepto = XmlHelper.GetAttributeValue(deduccionElement, "Concepto"),
                    Importe = XmlHelper.ParseDecimalRequired(
                        XmlHelper.GetAttributeValue(deduccionElement, "Importe"), "Importe")
                };

                _context.NominaDeducciones.Add(deduccion);
            }
        }

        private void ProcessOtrosPagos(XElement nominaElement, int idComprobante)
        {
            var otrosPagosElement = nominaElement.Element(XmlNamespaces.Nomina + "OtrosPagos");
            if (otrosPagosElement == null) return;

            foreach (var otroPagoElement in otrosPagosElement.Elements(XmlNamespaces.Nomina + "OtroPago"))
            {
                var subsidioElement = otroPagoElement.Element(XmlNamespaces.Nomina + "SubsidioAlEmpleo");

                var otroPago = new NominaOtrosPagos
                {
                    IdComprobante = idComprobante,
                    TipoOtroPago = XmlHelper.GetAttributeValue(otroPagoElement, "TipoOtroPago"),
                    Clave = XmlHelper.GetAttributeValue(otroPagoElement, "Clave"),
                    Concepto = XmlHelper.GetAttributeValue(otroPagoElement, "Concepto"),
                    Importe = XmlHelper.ParseDecimalRequired(
                        XmlHelper.GetAttributeValue(otroPagoElement, "Importe"), "Importe"),
                    SubsidioCausado = XmlHelper.ParseDecimalOrNull(
                        XmlHelper.GetAttributeValue(subsidioElement, "SubsidioCausado"))
                };

                _context.NominaOtrosPagos.Add(otroPago);
            }
        }

        #endregion

        #region Procesamiento de Ingreso/Egreso

        private ProcessResult ProcessIngresoEgreso(string xmlFilePath, XElement comprobanteElement, 
            XElement timbre, string uuid, string tipoComprobante)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var comprobante = CreateComprobanteIngresoEgreso(comprobanteElement, timbre, uuid, tipoComprobante);
                    _context.CfdiComprobante.Add(comprobante);
                    _context.SaveChanges();

                    ProcessEmisor(comprobanteElement, comprobante.IdComprobante);
                    ProcessReceptor(comprobanteElement, comprobante.IdComprobante);
                    ProcessConceptos(comprobanteElement, comprobante.IdComprobante);

                    _context.SaveChanges();
                    transaction.Commit();

                    string tipoNombre = tipoComprobante == "I" ? "Ingreso" : "Egreso";
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"✓ {Path.GetFileName(xmlFilePath)}: {tipoNombre} procesado (UUID: {uuid})");
                    Console.ResetColor();

                    return tipoComprobante == "I" ? ProcessResult.ProcessedIngreso : ProcessResult.ProcessedEgreso;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        private CfdiComprobante CreateComprobanteIngresoEgreso(XElement comprobanteElement, XElement timbre, 
            string uuid, string tipoComprobante)
        {
            return new CfdiComprobante
            {
                Uuid = uuid,
                Version = XmlHelper.GetAttributeValue(comprobanteElement, "Version") ?? "4.0",
                Serie = XmlHelper.GetAttributeValue(comprobanteElement, "Serie"),
                Folio = XmlHelper.GetAttributeValue(comprobanteElement, "Folio"),
                Fecha = XmlHelper.ParseDateTimeRequired(
                    XmlHelper.GetAttributeValue(comprobanteElement, "Fecha"), "Fecha"),
                FechaTimbrado = XmlHelper.ParseDateTimeRequired(
                    XmlHelper.GetAttributeValue(timbre, "FechaTimbrado"), "FechaTimbrado"),
                TipoDeComprobante = tipoComprobante,
                LugarExpedicion = XmlHelper.GetAttributeValue(comprobanteElement, "LugarExpedicion"),
                Moneda = XmlHelper.GetAttributeValue(comprobanteElement, "Moneda") ?? "MXN",
                TipoCambio = XmlHelper.ParseDecimalOrNull(
                    XmlHelper.GetAttributeValue(comprobanteElement, "TipoCambio")),
                Total = XmlHelper.ParseDecimalRequired(
                    XmlHelper.GetAttributeValue(comprobanteElement, "Total"), "Total"),
                SubTotal = XmlHelper.ParseDecimalRequired(
                    XmlHelper.GetAttributeValue(comprobanteElement, "SubTotal"), "SubTotal"),
                Descuento = XmlHelper.ParseDecimalOrNull(
                    XmlHelper.GetAttributeValue(comprobanteElement, "Descuento")),
                MetodoPago = XmlHelper.GetAttributeValue(comprobanteElement, "MetodoPago"),
                FormaPago = XmlHelper.GetAttributeValue(comprobanteElement, "FormaPago"),
                Estatus = "Vigente",
                SelloDigital = XmlHelper.GetAttributeValue(comprobanteElement, "Sello"),
                Certificado = XmlHelper.GetAttributeValue(comprobanteElement, "NoCertificado"),
                EmitidaRecibida = "E"
            };
        }

        #endregion

        #region Procesamiento de Pagos

        private ProcessResult ProcessPagos(string xmlFilePath, XElement comprobanteElement, 
            XElement complemento, XElement timbre, string uuid, XElement pagosElement)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var comprobante = CreateComprobantePagos(comprobanteElement, timbre, uuid);
                    _context.CfdiComprobante.Add(comprobante);
                    _context.SaveChanges();

                    ProcessEmisor(comprobanteElement, comprobante.IdComprobante);
                    ProcessReceptor(comprobanteElement, comprobante.IdComprobante);
                    ProcessPagosDetalle(pagosElement, comprobante.IdComprobante);

                    _context.SaveChanges();
                    transaction.Commit();

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"✓ {Path.GetFileName(xmlFilePath)}: Pago procesado (UUID: {uuid})");
                    Console.ResetColor();

                    return ProcessResult.ProcessedPagos;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        private CfdiComprobante CreateComprobantePagos(XElement comprobanteElement, XElement timbre, string uuid)
        {
            return new CfdiComprobante
            {
                Uuid = uuid,
                Version = XmlHelper.GetAttributeValue(comprobanteElement, "Version") ?? "4.0",
                Serie = XmlHelper.GetAttributeValue(comprobanteElement, "Serie"),
                Folio = XmlHelper.GetAttributeValue(comprobanteElement, "Folio"),
                Fecha = XmlHelper.ParseDateTimeRequired(
                    XmlHelper.GetAttributeValue(comprobanteElement, "Fecha"), "Fecha"),
                FechaTimbrado = XmlHelper.ParseDateTimeRequired(
                    XmlHelper.GetAttributeValue(timbre, "FechaTimbrado"), "FechaTimbrado"),
                TipoDeComprobante = "P",
                LugarExpedicion = XmlHelper.GetAttributeValue(comprobanteElement, "LugarExpedicion"),
                Moneda = XmlHelper.GetAttributeValue(comprobanteElement, "Moneda") ?? "XXX",
                TipoCambio = XmlHelper.ParseDecimalOrNull(
                    XmlHelper.GetAttributeValue(comprobanteElement, "TipoCambio")),
                Total = XmlHelper.ParseDecimalRequired(
                    XmlHelper.GetAttributeValue(comprobanteElement, "Total"), "Total"),
                SubTotal = XmlHelper.ParseDecimalRequired(
                    XmlHelper.GetAttributeValue(comprobanteElement, "SubTotal"), "SubTotal"),
                Descuento = XmlHelper.ParseDecimalOrNull(
                    XmlHelper.GetAttributeValue(comprobanteElement, "Descuento")),
                MetodoPago = XmlHelper.GetAttributeValue(comprobanteElement, "MetodoPago"),
                FormaPago = XmlHelper.GetAttributeValue(comprobanteElement, "FormaPago"),
                Estatus = "Vigente",
                SelloDigital = XmlHelper.GetAttributeValue(comprobanteElement, "Sello"),
                Certificado = XmlHelper.GetAttributeValue(comprobanteElement, "NoCertificado"),
                EmitidaRecibida = "E"
            };
        }

        private void ProcessPagosDetalle(XElement pagosElement, int idComprobante)
        {
            // Procesar Totales (opcional en Pagos 2.0)
            var totalesElement = pagosElement.Element(Pagos20 + "Totales");
            if (totalesElement != null)
            {
                var pagosDetalle = new PagosDetalle
                {
                    IdComprobante = idComprobante,
                    MontoTotalPagos = XmlHelper.ParseDecimalRequired(
                        XmlHelper.GetAttributeValue(totalesElement, "MontoTotalPagos"), "MontoTotalPagos"),
                    TotalTrasladosBaseIva16 = XmlHelper.ParseDecimalOrNull(
                        XmlHelper.GetAttributeValue(totalesElement, "TotalTrasladosBaseIVA16")),
                    TotalTrasladosImpuestoIva16 = XmlHelper.ParseDecimalOrNull(
                        XmlHelper.GetAttributeValue(totalesElement, "TotalTrasladosImpuestoIVA16"))
                };

                _context.PagosDetalle.Add(pagosDetalle);
            }

            // Procesar cada Pago
            foreach (var pagoElement in pagosElement.Elements(Pagos20 + "Pago"))
            {
                var pago = new PagosPago
                {
                    IdComprobante = idComprobante,
                    FechaPago = XmlHelper.ParseDateTimeRequired(
                        XmlHelper.GetAttributeValue(pagoElement, "FechaPago"), "FechaPago"),
                    FormaDePagoP = XmlHelper.GetAttributeValue(pagoElement, "FormaDePagoP"),
                    MonedaP = XmlHelper.GetAttributeValue(pagoElement, "MonedaP"),
                    TipoCambioP = XmlHelper.ParseDecimalOrNull(
                        XmlHelper.GetAttributeValue(pagoElement, "TipoCambioP")),
                    Monto = XmlHelper.ParseDecimalRequired(
                        XmlHelper.GetAttributeValue(pagoElement, "Monto"), "Monto"),
                    NumOperacion = XmlHelper.GetAttributeValue(pagoElement, "NumOperacion")
                };

                _context.PagosPago.Add(pago);
                _context.SaveChanges();

                ProcessDocumentosRelacionados(pagoElement, pago.IdPago);
            }
        }

        private void ProcessDocumentosRelacionados(XElement pagoElement, int idPago)
        {
            foreach (var doctoElement in pagoElement.Elements(Pagos20 + "DoctoRelacionado"))
            {
                var docto = new PagosDoctoRelacionado
                {
                    IdPago = idPago,
                    IdDocumento = XmlHelper.GetAttributeValue(doctoElement, "IdDocumento"),
                    Serie = XmlHelper.GetAttributeValue(doctoElement, "Serie"),
                    Folio = XmlHelper.GetAttributeValue(doctoElement, "Folio"),
                    MonedaDr = XmlHelper.GetAttributeValue(doctoElement, "MonedaDR"),
                    EquivalenciaDr = XmlHelper.ParseDecimalOrNull(
                        XmlHelper.GetAttributeValue(doctoElement, "EquivalenciaDR")),
                    NumParcialidad = string.IsNullOrEmpty(XmlHelper.GetAttributeValue(doctoElement, "NumParcialidad")) 
                        ? (int?)null 
                        : int.Parse(XmlHelper.GetAttributeValue(doctoElement, "NumParcialidad")),
                    ImpSaldoAnt = XmlHelper.ParseDecimalRequired(
                        XmlHelper.GetAttributeValue(doctoElement, "ImpSaldoAnt"), "ImpSaldoAnt"),
                    ImpPagado = XmlHelper.ParseDecimalRequired(
                        XmlHelper.GetAttributeValue(doctoElement, "ImpPagado"), "ImpPagado"),
                    ImpSaldoInsoluto = XmlHelper.ParseDecimalRequired(
                        XmlHelper.GetAttributeValue(doctoElement, "ImpSaldoInsoluto"), "ImpSaldoInsoluto"),
                    ObjetoImpDr = XmlHelper.GetAttributeValue(doctoElement, "ObjetoImpDR"),
                    ObjetoImp = XmlHelper.GetAttributeValue(doctoElement, "ObjetoImp")
                };

                _context.PagosDoctoRelacionado.Add(docto);
            }
        }

        #endregion

        #region Métodos Comunes

        private void ProcessEmisor(XElement comprobanteElement, int idComprobante)
        {
            var emisorElement = comprobanteElement.Element(XmlNamespaces.Cfdi + "Emisor");
            if (emisorElement == null) return;

            var emisor = new CfdiEmisor
            {
                IdComprobante = idComprobante,
                Rfc = XmlHelper.GetAttributeValue(emisorElement, "Rfc"),
                Nombre = XmlHelper.GetAttributeValue(emisorElement, "Nombre"),
                RegimenFiscal = XmlHelper.GetAttributeValue(emisorElement, "RegimenFiscal")
            };

            _context.CfdiEmisor.Add(emisor);
        }

        private void ProcessReceptor(XElement comprobanteElement, int idComprobante)
        {
            var receptorElement = comprobanteElement.Element(XmlNamespaces.Cfdi + "Receptor");
            if (receptorElement == null) return;

            var receptor = new CfdiReceptor
            {
                IdComprobante = idComprobante,
                Rfc = XmlHelper.GetAttributeValue(receptorElement, "Rfc"),
                Nombre = XmlHelper.GetAttributeValue(receptorElement, "Nombre"),
                DomicilioFiscalReceptor = XmlHelper.GetAttributeValue(receptorElement, "DomicilioFiscalReceptor"),
                RegimenFiscalReceptor = XmlHelper.GetAttributeValue(receptorElement, "RegimenFiscalReceptor"),
                UsoCfdi = XmlHelper.GetAttributeValue(receptorElement, "UsoCFDI"),
                NumRegIdTrib = XmlHelper.GetAttributeValue(receptorElement, "NumRegIdTrib"),
                ResidenciaFiscal = XmlHelper.GetAttributeValue(receptorElement, "ResidenciaFiscal")
            };

            _context.CfdiReceptor.Add(receptor);
        }

        private void ProcessConceptos(XElement comprobanteElement, int idComprobante)
        {
            var conceptosElement = comprobanteElement.Element(XmlNamespaces.Cfdi + "Conceptos");
            if (conceptosElement == null) return;

            foreach (var conceptoElement in conceptosElement.Elements(XmlNamespaces.Cfdi + "Concepto"))
            {
                var concepto = new CfdiConcepto
                {
                    IdComprobante = idComprobante,
                    ClaveProdServ = XmlHelper.GetAttributeValue(conceptoElement, "ClaveProdServ"),
                    Cantidad = XmlHelper.ParseDecimalRequired(
                        XmlHelper.GetAttributeValue(conceptoElement, "Cantidad"), "Cantidad"),
                    ClaveUnidad = XmlHelper.GetAttributeValue(conceptoElement, "ClaveUnidad"),
                    Unidad = XmlHelper.GetAttributeValue(conceptoElement, "Unidad"),
                    Descripcion = XmlHelper.GetAttributeValue(conceptoElement, "Descripcion"),
                    ValorUnitario = XmlHelper.ParseDecimalRequired(
                        XmlHelper.GetAttributeValue(conceptoElement, "ValorUnitario"), "ValorUnitario"),
                    Importe = XmlHelper.ParseDecimalRequired(
                        XmlHelper.GetAttributeValue(conceptoElement, "Importe"), "Importe"),
                    Descuento = XmlHelper.ParseDecimalOrNull(
                        XmlHelper.GetAttributeValue(conceptoElement, "Descuento")),
                    NoIdentificacion = XmlHelper.GetAttributeValue(conceptoElement, "NoIdentificacion"),
                    ObjetoImp = XmlHelper.GetAttributeValue(conceptoElement, "ObjetoImp")
                };

                _context.CfdiConcepto.Add(concepto);
                _context.SaveChanges();

                ProcessImpuestosConcepto(conceptoElement, concepto.IdConcepto);
            }
        }

        private void ProcessImpuestosConcepto(XElement conceptoElement, int idConcepto)
        {
            var impuestosElement = conceptoElement.Element(XmlNamespaces.Cfdi + "Impuestos");
            if (impuestosElement == null) return;

            // Procesar Traslados
            var trasladosElement = impuestosElement.Element(XmlNamespaces.Cfdi + "Traslados");
            if (trasladosElement != null)
            {
                foreach (var trasladoElement in trasladosElement.Elements(XmlNamespaces.Cfdi + "Traslado"))
                {
                    var traslado = new CfdiTrasladoConcepto
                    {
                        IdConcepto = idConcepto,
                        Base = XmlHelper.ParseDecimalRequired(
                            XmlHelper.GetAttributeValue(trasladoElement, "Base"), "Base"),
                        Impuesto = XmlHelper.GetAttributeValue(trasladoElement, "Impuesto"),
                        TipoFactor = XmlHelper.GetAttributeValue(trasladoElement, "TipoFactor"),
                        TasaOcuota = XmlHelper.ParseDecimalRequired(
                            XmlHelper.GetAttributeValue(trasladoElement, "TasaOCuota"), "TasaOCuota"),
                        Importe = XmlHelper.ParseDecimalOrNull(
                            XmlHelper.GetAttributeValue(trasladoElement, "Importe"))
                    };

                    _context.CfdiTrasladoConcepto.Add(traslado);
                }
            }

            // Procesar Retenciones
            var retencionesElement = impuestosElement.Element(XmlNamespaces.Cfdi + "Retenciones");
            if (retencionesElement != null)
            {
                foreach (var retencionElement in retencionesElement.Elements(XmlNamespaces.Cfdi + "Retencion"))
                {
                    var retencion = new CfdiRetencionConcepto
                    {
                        IdConcepto = idConcepto,
                        Base = XmlHelper.ParseDecimalRequired(
                            XmlHelper.GetAttributeValue(retencionElement, "Base"), "Base"),
                        Impuesto = XmlHelper.GetAttributeValue(retencionElement, "Impuesto"),
                        TipoFactor = XmlHelper.GetAttributeValue(retencionElement, "TipoFactor"),
                        TasaOcuota = XmlHelper.ParseDecimalRequired(
                            XmlHelper.GetAttributeValue(retencionElement, "TasaOCuota"), "TasaOCuota"),
                        Importe = XmlHelper.ParseDecimalRequired(
                            XmlHelper.GetAttributeValue(retencionElement, "Importe"), "Importe")
                    };

                    _context.CfdiRetencionConcepto.Add(retencion);
                }
            }
        }

        #endregion

        /// <summary>
        /// Enum para indicar el resultado del procesamiento
        /// </summary>
        private enum ProcessResult
        {
            Skipped,
            ProcessedNomina,
            ProcessedIngreso,
            ProcessedEgreso,
            ProcessedPagos
        }
    }
}
