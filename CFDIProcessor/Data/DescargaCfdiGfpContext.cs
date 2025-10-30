using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using CFDIProcessor.Models;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace CFDIProcessor.Data
{
    public partial class DescargaCfdiGfpContext : DbContext
    {
        public DescargaCfdiGfpContext()
        {
        }

        public DescargaCfdiGfpContext(DbContextOptions<DescargaCfdiGfpContext> options)
            : base(options)
        {
        }

        public virtual DbSet<CfdiComprobante> CfdiComprobante { get; set; }
        public virtual DbSet<CfdiConcepto> CfdiConcepto { get; set; }
        public virtual DbSet<CfdiEmisor> CfdiEmisor { get; set; }
        public virtual DbSet<CfdiReceptor> CfdiReceptor { get; set; }
        public virtual DbSet<CfdiRetencionConcepto> CfdiRetencionConcepto { get; set; }
        public virtual DbSet<CfdiTrasladoConcepto> CfdiTrasladoConcepto { get; set; }
        public virtual DbSet<NominaDeducciones> NominaDeducciones { get; set; }
        public virtual DbSet<NominaDetalle> NominaDetalle { get; set; }
        public virtual DbSet<NominaOtrosPagos> NominaOtrosPagos { get; set; }
        public virtual DbSet<NominaPercepciones> NominaPercepciones { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // La cadena de conexión se configura desde Program.cs usando appsettings.json
                // Si no se proporciona, usa la cadena por defecto
                optionsBuilder.UseSqlServer("Server=localhost;Database=DescargaCfdiGFP;Integrated Security=True;TrustServerCertificate=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CfdiComprobante>(entity =>
            {
                entity.HasKey(e => e.IdComprobante);

                entity.ToTable("CFDI_Comprobante");

                entity.HasIndex(e => e.Uuid)
                    .HasName("UQ_CFDI_Comprobante_UUID")
                    .IsUnique();

                entity.Property(e => e.IdComprobante).HasColumnName("ID_Comprobante");

                entity.Property(e => e.Certificado)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.Descuento).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.Estatus)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.Folio)
                    .HasMaxLength(40)
                    .IsUnicode(false);

                entity.Property(e => e.FormaPago)
                    .HasMaxLength(2)
                    .IsUnicode(false);

                entity.Property(e => e.LugarExpedicion)
                    .IsRequired()
                    .HasMaxLength(5)
                    .IsUnicode(false);

                entity.Property(e => e.MetodoPago)
                    .HasMaxLength(3)
                    .IsUnicode(false);

                entity.Property(e => e.Moneda)
                    .IsRequired()
                    .HasMaxLength(3)
                    .IsUnicode(false);

                entity.Property(e => e.SelloDigital).IsUnicode(false);

                entity.Property(e => e.Serie)
                    .HasMaxLength(25)
                    .IsUnicode(false);

                entity.Property(e => e.SubTotal).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.TipoCambio).HasColumnType("decimal(18, 6)");

                entity.Property(e => e.TipoDeComprobante)
                    .IsRequired()
                    .HasMaxLength(1)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.Total).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.Uuid)
                    .IsRequired()
                    .HasColumnName("UUID")
                    .HasMaxLength(36)
                    .IsUnicode(false);

                entity.Property(e => e.Version)
                    .IsRequired()
                    .HasMaxLength(5)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<CfdiConcepto>(entity =>
            {
                entity.HasKey(e => e.IdConcepto);

                entity.ToTable("CFDI_Concepto");

                entity.Property(e => e.IdConcepto).HasColumnName("ID_Concepto");

                entity.Property(e => e.Cantidad).HasColumnType("decimal(18, 6)");

                entity.Property(e => e.ClaveProdServ)
                    .IsRequired()
                    .HasMaxLength(8)
                    .IsUnicode(false);

                entity.Property(e => e.ClaveUnidad)
                    .IsRequired()
                    .HasMaxLength(3)
                    .IsUnicode(false);

                entity.Property(e => e.Descripcion)
                    .IsRequired()
                    .IsUnicode(false);

                entity.Property(e => e.Descuento).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.IdComprobante).HasColumnName("ID_Comprobante");

                entity.Property(e => e.Importe).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.NoIdentificacion)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ObjetoImp)
                    .HasMaxLength(2)
                    .IsUnicode(false);

                entity.Property(e => e.Unidad)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.ValorUnitario).HasColumnType("decimal(18, 6)");

                entity.HasOne(d => d.IdComprobanteNavigation)
                    .WithMany(p => p.CfdiConcepto)
                    .HasForeignKey(d => d.IdComprobante)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CFDI_Concepto_Comprobante");
            });

            modelBuilder.Entity<CfdiEmisor>(entity =>
            {
                entity.HasKey(e => e.IdComprobante);

                entity.ToTable("CFDI_Emisor");

                entity.Property(e => e.IdComprobante)
                    .HasColumnName("ID_Comprobante")
                    .ValueGeneratedNever();

                entity.Property(e => e.Nombre)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.RegimenFiscal)
                    .IsRequired()
                    .HasMaxLength(3)
                    .IsUnicode(false);

                entity.Property(e => e.RegistroPatronal)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.Rfc)
                    .IsRequired()
                    .HasMaxLength(13)
                    .IsUnicode(false);

                entity.HasOne(d => d.IdComprobanteNavigation)
                    .WithOne(p => p.CfdiEmisor)
                    .HasForeignKey<CfdiEmisor>(d => d.IdComprobante)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CFDI_Emisor_Comprobante");
            });

            modelBuilder.Entity<CfdiReceptor>(entity =>
            {
                entity.HasKey(e => e.IdComprobante);

                entity.ToTable("CFDI_Receptor");

                entity.Property(e => e.IdComprobante)
                    .HasColumnName("ID_Comprobante")
                    .ValueGeneratedNever();

                entity.Property(e => e.DomicilioFiscalReceptor)
                    .IsRequired()
                    .HasMaxLength(5)
                    .IsUnicode(false);

                entity.Property(e => e.Nombre)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.NumRegIdTrib)
                    .HasMaxLength(40)
                    .IsUnicode(false);

                entity.Property(e => e.RegimenFiscalReceptor)
                    .IsRequired()
                    .HasMaxLength(3)
                    .IsUnicode(false);

                entity.Property(e => e.ResidenciaFiscal)
                    .HasMaxLength(5)
                    .IsUnicode(false);

                entity.Property(e => e.Rfc)
                    .IsRequired()
                    .HasMaxLength(13)
                    .IsUnicode(false);

                entity.Property(e => e.UsoCfdi)
                    .IsRequired()
                    .HasColumnName("UsoCFDI")
                    .HasMaxLength(4)
                    .IsUnicode(false);

                entity.HasOne(d => d.IdComprobanteNavigation)
                    .WithOne(p => p.CfdiReceptor)
                    .HasForeignKey<CfdiReceptor>(d => d.IdComprobante)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CFDI_Receptor_Comprobante");
            });

            modelBuilder.Entity<CfdiRetencionConcepto>(entity =>
            {
                entity.HasKey(e => e.IdRetencion);

                entity.ToTable("CFDI_RetencionConcepto");

                entity.Property(e => e.IdRetencion).HasColumnName("ID_Retencion");

                entity.Property(e => e.Base).HasColumnType("decimal(18, 6)");

                entity.Property(e => e.IdConcepto).HasColumnName("ID_Concepto");

                entity.Property(e => e.Importe).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.Impuesto)
                    .IsRequired()
                    .HasMaxLength(3)
                    .IsUnicode(false);

                entity.Property(e => e.TasaOcuota)
                    .HasColumnName("TasaOCuota")
                    .HasColumnType("decimal(18, 6)");

                entity.Property(e => e.TipoFactor)
                    .IsRequired()
                    .HasMaxLength(8)
                    .IsUnicode(false);

                entity.HasOne(d => d.IdConceptoNavigation)
                    .WithMany(p => p.CfdiRetencionConcepto)
                    .HasForeignKey(d => d.IdConcepto)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CFDI_RetencionConcepto_Concepto");
            });

            modelBuilder.Entity<CfdiTrasladoConcepto>(entity =>
            {
                entity.HasKey(e => e.IdTraslado);

                entity.ToTable("CFDI_TrasladoConcepto");

                entity.Property(e => e.IdTraslado).HasColumnName("ID_Traslado");

                entity.Property(e => e.Base).HasColumnType("decimal(18, 6)");

                entity.Property(e => e.IdConcepto).HasColumnName("ID_Concepto");

                entity.Property(e => e.Importe).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.Impuesto)
                    .IsRequired()
                    .HasMaxLength(3)
                    .IsUnicode(false);

                entity.Property(e => e.TasaOcuota)
                    .HasColumnName("TasaOCuota")
                    .HasColumnType("decimal(18, 6)");

                entity.Property(e => e.TipoFactor)
                    .IsRequired()
                    .HasMaxLength(8)
                    .IsUnicode(false);

                entity.HasOne(d => d.IdConceptoNavigation)
                    .WithMany(p => p.CfdiTrasladoConcepto)
                    .HasForeignKey(d => d.IdConcepto)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CFDI_TrasladoConcepto_Concepto");
            });

            modelBuilder.Entity<NominaDeducciones>(entity =>
            {
                entity.HasKey(e => e.IdDeduccion);

                entity.ToTable("Nomina_Deducciones");

                entity.Property(e => e.IdDeduccion).HasColumnName("ID_Deduccion");

                entity.Property(e => e.Clave)
                    .IsRequired()
                    .HasMaxLength(3)
                    .IsUnicode(false);

                entity.Property(e => e.Concepto)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.IdComprobante).HasColumnName("ID_Comprobante");

                entity.Property(e => e.Importe).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.TipoDeduccion)
                    .IsRequired()
                    .HasMaxLength(3)
                    .IsUnicode(false);

                entity.HasOne(d => d.IdComprobanteNavigation)
                    .WithMany(p => p.NominaDeducciones)
                    .HasForeignKey(d => d.IdComprobante)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Nomina_Deducciones_Comprobante");
            });

            modelBuilder.Entity<NominaDetalle>(entity =>
            {
                entity.HasKey(e => e.IdComprobante);

                entity.ToTable("Nomina_Detalle");

                entity.Property(e => e.IdComprobante)
                    .HasColumnName("ID_Comprobante")
                    .ValueGeneratedNever();

                entity.Property(e => e.Antiguedad)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.ClaveEntFed)
                    .HasMaxLength(3)
                    .IsUnicode(false);

                entity.Property(e => e.CuentaBancaria)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.CurpEmpleado)
                    .IsRequired()
                    .HasMaxLength(18)
                    .IsUnicode(false);

                entity.Property(e => e.Departamento)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.FechaFinalPago).HasColumnType("date");

                entity.Property(e => e.FechaInicialPago).HasColumnType("date");

                entity.Property(e => e.FechaInicioRelLaboral).HasColumnType("date");

                entity.Property(e => e.FechaPago).HasColumnType("date");

                entity.Property(e => e.NumDiasPagados).HasColumnType("decimal(18, 3)");

                entity.Property(e => e.NumEmpleado)
                    .HasMaxLength(15)
                    .IsUnicode(false);

                entity.Property(e => e.NumSeguridadSocial)
                    .HasMaxLength(15)
                    .IsUnicode(false);

                entity.Property(e => e.PeriodicidadPago)
                    .HasMaxLength(2)
                    .IsUnicode(false);

                entity.Property(e => e.Puesto)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.SalarioBaseCotApor).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.SalarioDiarioIntegrado).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.TipoContrato)
                    .HasMaxLength(2)
                    .IsUnicode(false);

                entity.Property(e => e.TipoNomina)
                    .IsRequired()
                    .HasMaxLength(1)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.TotalDeducciones).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.TotalOtrosPagos).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.TotalPercepciones).HasColumnType("decimal(18, 2)");

                entity.HasOne(d => d.IdComprobanteNavigation)
                    .WithOne(p => p.NominaDetalle)
                    .HasForeignKey<NominaDetalle>(d => d.IdComprobante)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Nomina_Detalle_Comprobante");
            });

            modelBuilder.Entity<NominaOtrosPagos>(entity =>
            {
                entity.HasKey(e => e.IdOtroPago);

                entity.ToTable("Nomina_OtrosPagos");

                entity.Property(e => e.IdOtroPago).HasColumnName("ID_OtroPago");

                entity.Property(e => e.Clave)
                    .IsRequired()
                    .HasMaxLength(3)
                    .IsUnicode(false);

                entity.Property(e => e.Concepto)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.IdComprobante).HasColumnName("ID_Comprobante");

                entity.Property(e => e.Importe).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.SubsidioCausado).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.TipoOtroPago)
                    .IsRequired()
                    .HasMaxLength(3)
                    .IsUnicode(false);

                entity.HasOne(d => d.IdComprobanteNavigation)
                    .WithMany(p => p.NominaOtrosPagos)
                    .HasForeignKey(d => d.IdComprobante)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Nomina_OtrosPagos_Comprobante");
            });

            modelBuilder.Entity<NominaPercepciones>(entity =>
            {
                entity.HasKey(e => e.IdPercepcion);

                entity.ToTable("Nomina_Percepciones");

                entity.Property(e => e.IdPercepcion).HasColumnName("ID_Percepcion");

                entity.Property(e => e.Clave)
                    .IsRequired()
                    .HasMaxLength(3)
                    .IsUnicode(false);

                entity.Property(e => e.Concepto)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.IdComprobante).HasColumnName("ID_Comprobante");

                entity.Property(e => e.ImporteExento).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.ImporteGravado).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.TipoPercepcion)
                    .IsRequired()
                    .HasMaxLength(3)
                    .IsUnicode(false);

                entity.HasOne(d => d.IdComprobanteNavigation)
                    .WithMany(p => p.NominaPercepciones)
                    .HasForeignKey(d => d.IdComprobante)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Nomina_Percepciones_Comprobante");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
