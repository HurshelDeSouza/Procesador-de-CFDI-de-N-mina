using System;
using System.Collections.Generic;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace CFDIProcessor.Models
{
    public partial class NominaDetalle
    {
        public int IdComprobante { get; set; }
        public string TipoNomina { get; set; }
        public DateTime FechaPago { get; set; }
        public DateTime FechaInicialPago { get; set; }
        public DateTime FechaFinalPago { get; set; }
        public decimal NumDiasPagados { get; set; }
        public decimal? TotalPercepciones { get; set; }
        public decimal? TotalDeducciones { get; set; }
        public decimal? TotalOtrosPagos { get; set; }
        public string CurpEmpleado { get; set; }
        public string NumSeguridadSocial { get; set; }
        public DateTime? FechaInicioRelLaboral { get; set; }
        public string Antiguedad { get; set; }
        public string TipoContrato { get; set; }
        public string NumEmpleado { get; set; }
        public string Departamento { get; set; }
        public string Puesto { get; set; }
        public decimal? SalarioBaseCotApor { get; set; }
        public decimal? SalarioDiarioIntegrado { get; set; }
        public string PeriodicidadPago { get; set; }
        public string CuentaBancaria { get; set; }
        public string ClaveEntFed { get; set; }

        public virtual CfdiComprobante IdComprobanteNavigation { get; set; }
    }
}
