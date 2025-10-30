using System;
using System.Collections.Generic;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace CFDIProcessor.Models
{
    public partial class CfdiConcepto
    {
        public CfdiConcepto()
        {
            CfdiRetencionConcepto = new HashSet<CfdiRetencionConcepto>();
            CfdiTrasladoConcepto = new HashSet<CfdiTrasladoConcepto>();
        }

        public int IdConcepto { get; set; }
        public int IdComprobante { get; set; }
        public string ClaveProdServ { get; set; }
        public decimal Cantidad { get; set; }
        public string ClaveUnidad { get; set; }
        public string Unidad { get; set; }
        public string Descripcion { get; set; }
        public decimal ValorUnitario { get; set; }
        public decimal Importe { get; set; }
        public decimal? Descuento { get; set; }
        public string NoIdentificacion { get; set; }
        public string ObjetoImp { get; set; }

        public virtual CfdiComprobante IdComprobanteNavigation { get; set; }
        public virtual ICollection<CfdiRetencionConcepto> CfdiRetencionConcepto { get; set; }
        public virtual ICollection<CfdiTrasladoConcepto> CfdiTrasladoConcepto { get; set; }
    }
}
