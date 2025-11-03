using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CFDIProcessor.Models
{
    [Table("CFDI_Pagos_DoctoRelacionado")]
    public partial class PagosDoctoRelacionado
    {
        [Key]
        [Column("ID_DoctoRel")]
        public int IdDoctoRel { get; set; }

        [Column("ID_Pago")]
        public int IdPago { get; set; }

        [Required]
        [StringLength(36)]
        public string IdDocumento { get; set; }

        [StringLength(25)]
        public string Serie { get; set; }

        [StringLength(40)]
        public string Folio { get; set; }

        [Required]
        [StringLength(3)]
        public string MonedaDr { get; set; }

        [Column(TypeName = "decimal(18, 6)")]
        public decimal? EquivalenciaDr { get; set; }

        public int? NumParcialidad { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal ImpSaldoAnt { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal ImpPagado { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal ImpSaldoInsoluto { get; set; }

        [StringLength(2)]
        public string ObjetoImpDr { get; set; }

        [StringLength(2)]
        public string ObjetoImp { get; set; }

        [ForeignKey(nameof(IdPago))]
        public virtual PagosPago IdPagoNavigation { get; set; }
    }
}
