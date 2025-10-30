using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using CFDIProcessor.Constants;
using CFDIProcessor.Data;
using CFDIProcessor.Helpers;
using CFDIProcessor.Models;

namespace CFDIProcessor.Services
{
    /// <summary>
    /// Servicio para procesar archivos XML de CFDI de nómina
    /// </summary>
    public class NominaXmlProcessor
    {
        private readonly DescargaCfdiGfpContext _context;

        public NominaXmlProcessor(DescargaCfdiGfpContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Procesa todos los archivos XML de una carpeta
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

            var xmlFiles = Directory.GetFiles(folderPath, "*.xml");
            Console.WriteLine($"Se encontraron {xmlFiles.Length} archivo(s) XML.");
            Console.WriteLine();

            int processedCount = 0;
            int skippedCount = 0;
            int errorCount = 0;

            foreach (var xmlFile in xmlFiles)
            {
                try
                {
                    bool processed = ProcessXmlFile(xmlFile);
                    if (processed)
                        processedCount++;
                    else
                        skippedCount++;
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
            Console.WriteLine($"✓ Procesados exitosamente: {processedCount}");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"⊘ Omitidos (duplicados o no válidos): {skippedCount}");
            Console.ResetColor();
            if (errorCount > 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"✗ Errores: {errorCount}");
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Procesa un archivo XML individual
        /// </summary>
        /// <returns>True si se procesó exitosamente, False si se omitió</returns>
        private bool ProcessXmlFile(string xmlFilePath)
        {
            XDocument doc = XDocument.Load(xmlFilePath);
            var comprobanteElement = doc.Root;

            if (comprobanteElement == null)
            {
                Console.WriteLine($"⊘ {Path.GetFileName(xmlFilePath)}: XML inválido (sin elemento raíz)");
                return false;
            }

            // Verificar si es un comprobante de nómina
            var complemento = comprobanteElement.Element(XmlNamespaces.Cfdi + "Complemento");
            var nominaElement = complemento?.Element(XmlNamespaces.Nomina + "Nomina");

            if (nominaElement == null)
            {
                Console.WriteLine($"⊘ {Path.GetFileName(xmlFilePath)}: No es un CFDI de nómina");
                return false;
            }

            // Obtener UUID
            var timbre = complemento?.Element(XmlNamespaces.TimbreFiscalDigital + "TimbreFiscalDigital");
            string uuid = XmlHelper.GetAttributeValue(timbre, "UUID");

            if (string.IsNullOrEmpty(uuid))
            {
                Console.WriteLine($"⊘ {Path.GetFileName(xmlFilePath)}: No se encontró UUID");
                return false;
            }

            // Verificar duplicados
            if (_context.CfdiComprobante.Any(c => c.Uuid == uuid))
            {
                Console.WriteLine($"⊘ {Path.GetFileName(xmlFilePath)}: UUID {uuid} ya existe");
                return false;
            }

            // Procesar el CFDI en una transacción
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var comprobante = CreateComprobante(comprobanteElement, timbre, uuid);
                    _context.CfdiComprobante.Add(comprobante);
                    _context.SaveChanges();

                    ProcessEmisor(comprobanteElement, nominaElement, comprobante.IdComprobante);
                    ProcessReceptor(comprobanteElement, comprobante.IdComprobante);
                    ProcessConceptos(comprobanteElement, comprobante.IdComprobante);
                    ProcessNominaDetalle(nominaElement, comprobante.IdComprobante);
                    ProcessPercepciones(nominaElement, comprobante.IdComprobante);
                    ProcessDeducciones(nominaElement, comprobante.IdComprobante);
                    ProcessOtrosPagos(nominaElement, comprobante.IdComprobante);

                    _context.SaveChanges();
                    transaction.Commit();

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"✓ {Path.GetFileName(xmlFilePath)}: Procesado exitosamente (UUID: {uuid})");
                    Console.ResetColor();

                    return true;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        #region Métodos de Creación de Entidades

        /// <summary>
        /// Crea la entidad CfdiComprobante desde el XML
        /// </summary>
        private CfdiComprobante CreateComprobante(XElement comprobanteElement, XElement timbre, string uuid)
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
                TipoDeComprobante = XmlHelper.GetAttributeValue(comprobanteElement, "TipoDeComprobante") ?? "N",
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
                Certificado = XmlHelper.GetAttributeValue(comprobanteElement, "NoCertificado")
            };
        }

        /// <summary>
        /// Procesa el emisor del CFDI
        /// </summary>
        private void ProcessEmisor(XElement comprobanteElement, XElement nominaElement, int idComprobante)
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

        /// <summary>
        /// Procesa el receptor del CFDI
        /// </summary>
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

        /// <summary>
        /// Procesa los conceptos del CFDI
        /// </summary>
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
            }
        }

        /// <summary>
        /// Procesa el detalle de nómina
        /// </summary>
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

        /// <summary>
        /// Procesa las percepciones de nómina
        /// </summary>
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

        /// <summary>
        /// Procesa las deducciones de nómina
        /// </summary>
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

        /// <summary>
        /// Procesa otros pagos de nómina
        /// </summary>
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
    }
}
