namespace CFDIProcessor.Models
{
    /// <summary>
    /// Configuración de procesamiento de CFDI
    /// </summary>
    public class ProcessingSettings
    {
        /// <summary>
        /// Indica si se deben procesar CFDI de Nómina (N)
        /// </summary>
        public bool ProcessNomina { get; set; } = true;

        /// <summary>
        /// Indica si se deben procesar CFDI de Ingreso (I)
        /// </summary>
        public bool ProcessIngreso { get; set; } = true;

        /// <summary>
        /// Indica si se deben procesar CFDI de Egreso (E)
        /// </summary>
        public bool ProcessEgreso { get; set; } = true;

        /// <summary>
        /// Indica si se deben procesar CFDI de Pagos (P)
        /// </summary>
        public bool ProcessPagos { get; set; } = true;
    }
}