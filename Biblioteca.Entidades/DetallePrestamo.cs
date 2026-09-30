using System;

namespace Biblioteca.Entidades
{
    public class DetallePrestamo
    {
        public int PrestamoId { get; set; }
        public int LibroId { get; set; }
        public DateTime? FechaDevolucion { get; set; } // El ? indica que permite nulos

        // Propiedad extra para mostrar en la interfaz
        public string TituloLibro { get; set; }
    }
}