using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CFDIProcessor.Models
{
    [Table("CFDI_Concepto_Impuestos")]
    public partial class CfdiConceptoImpuesto
    {
        [Key]
        [Column("ID_Concepto_Impuesto")]
        public int IdConceptoImpuesto { get; set; }

        [Column("ID_Concepto")]
        public int IdConcepto { get; set; }

        [Required]
        [StringLength(20)]
        public string TipoImpuesto { get; set; }

        [Required]
        [StringLength(3)]
        public string Impuesto { get; set; }

        [StringLength(10)]
        public string TipoFactor { get; set; }

        [Column(TypeName = "decimal(18, 6)")]
        public decimal? TasaOCuota { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Base { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? Importe { get; set; }

        [ForeignKey(nameof(IdConcepto))]
        public virtual CfdiConcepto IdConceptoNavigation { get; set; }
    }
}
