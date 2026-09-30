using Biblioteca.Datos;
using Biblioteca.Entidades;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Biblioteca.Negocio
{
    public class LibroNegocio
    {
        private LibroDatos libroDatos;
        private AutorDatos autorDatos;

        public LibroNegocio()
        {
            libroDatos = new LibroDatos();
            autorDatos = new AutorDatos();
        }

        // Regla: No se permite repetir ISBN
        public async Task<List<Libro>> ListarActivos()
        {
            return await libroDatos.ListarActivos();
        }

        public async Task<Libro> ObtenerPorId(int libroId)
        {
            return await libroDatos.ObtenerPorId(libroId);
        }

        public async Task<List<Autor>> ListarAutoresActivos()
        {
            return await autorDatos.ListarActivos();
        }

        public async Task Insertar(Libro libro)
        {
            // Validar que no exista el ISBN
            if (await libroDatos.ISBNExiste(libro.ISBN))
            {
                throw new ReglaNegocioException($"El ISBN '{libro.ISBN}' ya está registrado.");
            }

            // Validar que tenga ejemplares
            if (libro.Ejemplares <= 0)
            {
                throw new ReglaNegocioException("El libro debe tener al menos 1 ejemplar.");
            }

            await libroDatos.Insertar(libro);
        }

        public async Task Actualizar(Libro libro)
        {
            // Validar que no exista el ISBN (excepto el mismo libro)
            if (await libroDatos.ISBNExiste(libro.ISBN, libro.LibroId))
            {
                throw new ReglaNegocioException($"El ISBN '{libro.ISBN}' ya está registrado en otro libro.");
            }

            await libroDatos.Actualizar(libro);
        }

        // Regla: Eliminación lógica, no se puede eliminar si tiene préstamos pendientes
        public async Task EliminarLogico(int libroId)
        {
            // Validar que no tenga préstamos pendientes
            if (await libroDatos.TienePrestamosPendientes(libroId))
            {
                throw new ReglaNegocioException("No se puede eliminar el libro porque tiene préstamos pendientes.");
            }

            await libroDatos.EliminarLogico(libroId);
        }
    }
}