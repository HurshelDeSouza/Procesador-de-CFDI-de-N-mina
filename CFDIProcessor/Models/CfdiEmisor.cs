using System;
using System.Collections.Generic;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace CFDIProcessor.Models
{
    public partial class CfdiEmisor
    {
        public int IdComprobante { get; set; }
        public string Rfc { get; set; }
        public string Nombre { get; set; }
        public string RegimenFiscal { get; set; }
        public string RegistroPatronal { get; set; }

        public virtual CfdiComprobante IdComprobanteNavigation { get; set; }
    }
}
