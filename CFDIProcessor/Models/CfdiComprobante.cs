using System;
using System.Collections.Generic;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace CFDIProcessor.Models
{
    public partial class CfdiComprobante
    {
        public CfdiComprobante()
        {
            CfdiConcepto = new HashSet<CfdiConcepto>();
            NominaDeducciones = new HashSet<NominaDeducciones>();
            NominaOtrosPagos = new HashSet<NominaOtrosPagos>();
            NominaPercepciones = new HashSet<NominaPercepciones>();
            PagosPago = new HashSet<PagosPago>();
        }

        public int IdComprobante { get; set; }
        public string Uuid { get; set; }
        public string Version { get; set; }
        public string Serie { get; set; }
        public string Folio { get; set; }
        public DateTime Fecha { get; set; }
        public DateTime? FechaTimbrado { get; set; }
        public string TipoDeComprobante { get; set; }
        public string LugarExpedicion { get; set; }
        public string Moneda { get; set; }
        public decimal? TipoCambio { get; set; }
        public decimal Total { get; set; }
        public decimal SubTotal { get; set; }
        public decimal? Descuento { get; set; }
        public string MetodoPago { get; set; }
        public string FormaPago { get; set; }
        public string Estatus { get; set; }
        public string SelloDigital { get; set; }
        public string Certificado { get; set; }
        public string EmitidaRecibida { get; set; }

        public virtual CfdiEmisor CfdiEmisor { get; set; }
        public virtual CfdiReceptor CfdiReceptor { get; set; }
        public virtual NominaDetalle NominaDetalle { get; set; }
        public virtual PagosDetalle PagosDetalle { get; set; }
        public virtual ICollection<CfdiConcepto> CfdiConcepto { get; set; }
        public virtual ICollection<NominaDeducciones> NominaDeducciones { get; set; }
        public virtual ICollection<NominaOtrosPagos> NominaOtrosPagos { get; set; }
        public virtual ICollection<NominaPercepciones> NominaPercepciones { get; set; }
        public virtual ICollection<PagosPago> PagosPago { get; set; }
    }
}
