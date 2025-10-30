using System;
using System.Collections.Generic;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace CFDIProcessor.Models
{
    public partial class NominaOtrosPagos
    {
        public int IdOtroPago { get; set; }
        public int IdComprobante { get; set; }
        public string TipoOtroPago { get; set; }
        public string Clave { get; set; }
        public string Concepto { get; set; }
        public decimal Importe { get; set; }
        public decimal? SubsidioCausado { get; set; }

        public virtual CfdiComprobante IdComprobanteNavigation { get; set; }
    }
}
