using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CFDIProcessor.Models
{
    [Table("CFDI_Pagos_Detalle")]
    public partial class PagosDetalle
    {
        [Key]
        [Column("ID_Comprobante")]
        public int IdComprobante { get; set; }

        [StringLength(2)]
        public string FormaDePago { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal MontoTotalPagos { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? TotalTrasladosBaseIva16 { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? TotalTrasladosImpuestoIva16 { get; set; }

        [ForeignKey(nameof(IdComprobante))]
        public virtual CfdiComprobante IdComprobanteNavigation { get; set; }
    }
}
