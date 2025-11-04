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
    /// Servicio para procesar archivos XML de CFDI de Ingreso y Egreso
    /// </summary>
    public class IngresoEgresoXmlProcessor
    {
        private readonly DescargaCfdiGfpContext _context;

        public IngresoEgresoXmlProcessor(DescargaCfdiGfpContext context)
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
        private bool ProcessXmlFile(string xmlFilePath)
        {
            XDocument doc = XDocument.Load(xmlFilePath);
            var comprobanteElement = doc.Root;

            if (comprobanteElement == null)
            {
                Console.WriteLine($"⊘ {Path.GetFileName(xmlFilePath)}: XML inválido (sin elemento raíz)");
                return false;
            }

            // Obtener tipo de comprobante
            string tipoComprobante = XmlHelper.GetAttributeValue(comprobanteElement, "TipoDeComprobante");

            // Solo procesar Ingreso (I) o Egreso (E)
            if (tipoComprobante != "I" && tipoComprobante != "E")
            {
                Console.WriteLine($"⊘ {Path.GetFileName(xmlFilePath)}: No es CFDI de Ingreso o Egreso (Tipo: {tipoComprobante})");
                return false;
            }

            // Obtener UUID
            var complemento = comprobanteElement.Element(XmlNamespaces.Cfdi + "Complemento");
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
                    var comprobante = CreateComprobante(comprobanteElement, timbre, uuid, tipoComprobante);
                    _context.CfdiComprobante.Add(comprobante);
                    _context.SaveChanges();

                    ProcessEmisor(comprobanteElement, comprobante.IdComprobante);
                    ProcessReceptor(comprobanteElement, comprobante.IdComprobante);
                    ProcessConceptos(comprobanteElement, comprobante.IdComprobante);

                    _context.SaveChanges();
                    transaction.Commit();

                    string tipoNombre = tipoComprobante == "I" ? "Ingreso" : "Egreso";
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"✓ {Path.GetFileName(xmlFilePath)}: CFDI de {tipoNombre} procesado (UUID: {uuid})");
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

        private CfdiComprobante CreateComprobante(XElement comprobanteElement, XElement timbre, string uuid, string tipoComprobante)
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
                EmitidaRecibida = "E" // Por defecto Emitida
            };
        }

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
                _context.SaveChanges(); // Guardar para obtener el ID

                // Procesar impuestos del concepto
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
    }
}
