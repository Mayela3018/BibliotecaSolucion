using System;

namespace Biblioteca.Entidades
{
    public class ReportePrestamo
    {
        public string NombreSocio { get; set; }
        public string TituloLibro { get; set; }
        public DateTime FechaPrestamo { get; set; }
        public DateTime FechaLimite { get; set; }
        public string Estado { get; set; }
    }
}