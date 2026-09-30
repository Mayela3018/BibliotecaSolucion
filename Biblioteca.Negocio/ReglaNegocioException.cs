using System;

namespace Biblioteca.Negocio
{
    public class ReglaNegocioException : Exception
    {
        public ReglaNegocioException(string mensaje) : base(mensaje)
        {
        }

        public ReglaNegocioException(string mensaje, Exception innerException) : base(mensaje, innerException)
        {
        }
    }
}