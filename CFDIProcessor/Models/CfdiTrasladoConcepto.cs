using System;
using System.Collections.Generic;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace CFDIProcessor.Models
{
    public partial class CfdiTrasladoConcepto
    {
        public int IdTraslado { get; set; }
        public int IdConcepto { get; set; }
        public decimal Base { get; set; }
        public string Impuesto { get; set; }
        public string TipoFactor { get; set; }
        public decimal TasaOcuota { get; set; }
        public decimal? Importe { get; set; }

        public virtual CfdiConcepto IdConceptoNavigation { get; set; }
    }
}
