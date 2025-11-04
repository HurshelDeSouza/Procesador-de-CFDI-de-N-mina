using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CFDIProcessor.Models
{
    [Table("CFDI_Pagos_Pago")]
    public partial class PagosPago
    {
        public PagosPago()
        {
            PagosDoctoRelacionado = new HashSet<PagosDoctoRelacionado>();
        }

        [Key]
        [Column("ID_Pago")]
        public int IdPago { get; set; }

        [Column("ID_Comprobante")]
        public int IdComprobante { get; set; }

        public DateTime FechaPago { get; set; }

        [Required]
        [StringLength(2)]
        public string FormaDePagoP { get; set; }

        [Required]
        [StringLength(3)]
        public string MonedaP { get; set; }

        [Column(TypeName = "decimal(18, 6)")]
        public decimal? TipoCambioP { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Monto { get; set; }

        [StringLength(100)]
        public string NumOperacion { get; set; }

        [ForeignKey(nameof(IdComprobante))]
        public virtual CfdiComprobante IdComprobanteNavigation { get; set; }

        public virtual ICollection<PagosDoctoRelacionado> PagosDoctoRelacionado { get; set; }
    }
}
