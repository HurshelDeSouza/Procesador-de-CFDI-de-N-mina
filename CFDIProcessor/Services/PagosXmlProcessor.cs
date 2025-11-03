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
    /// Servicio para procesar archivos XML de CFDI de Pagos 2.0
    /// </summary>
    public class PagosXmlProcessor
    {
        private readonly DescargaCfdiGfpContext _context;
        private static readonly XNamespace Pagos20 = "http://www.sat.gob.mx/Pagos20";

        public PagosXmlProcessor(DescargaCfdiGfpContext context)
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

            // Verificar si es un comprobante de Pago (P)
            string tipoComprobante = XmlHelper.GetAttributeValue(comprobanteElement, "TipoDeComprobante");
            if (tipoComprobante != "P")
            {
                Console.WriteLine($"⊘ {Path.GetFileName(xmlFilePath)}: No es CFDI de Pago (Tipo: {tipoComprobante})");
                return false;
            }

            // Verificar complemento de Pagos 2.0
            var complemento = comprobanteElement.Element(XmlNamespaces.Cfdi + "Complemento");
            var pagosElement = complemento?.Element(Pagos20 + "Pagos");

            if (pagosElement == null)
            {
                Console.WriteLine($"⊘ {Path.GetFileName(xmlFilePath)}: No tiene complemento de Pagos 2.0");
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

                    ProcessEmisor(comprobanteElement, comprobante.IdComprobante);
                    ProcessReceptor(comprobanteElement, comprobante.IdComprobante);
                    ProcessPagos(pagosElement, comprobante.IdComprobante);

                    _context.SaveChanges();
                    transaction.Commit();

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"✓ {Path.GetFileName(xmlFilePath)}: CFDI de Pago procesado (UUID: {uuid})");
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

        private void ProcessPagos(XElement pagosElement, int idComprobante)
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
                _context.SaveChanges(); // Guardar para obtener el ID

                // Procesar Documentos Relacionados
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
    }
}
