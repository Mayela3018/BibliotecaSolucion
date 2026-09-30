using System;

namespace Biblioteca.Entidades
{
    public class Prestamo
    {
        public int PrestamoId { get; set; }
        public int SocioId { get; set; }
        public DateTime FechaPrestamo { get; set; }
        public DateTime FechaLimite { get; set; }
        public string Estado { get; set; }

        // Propiedad extra para mostrar en la interfaz
        public string NombreSocio { get; set; }
    }
}