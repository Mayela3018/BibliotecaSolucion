using Biblioteca.Datos;
using Biblioteca.Entidades;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Biblioteca.Negocio
{
    public class SocioNegocio
    {
        private SocioDatos socioDatos;

        public SocioNegocio()
        {
            socioDatos = new SocioDatos();
        }

        public async Task<List<Socio>> ListarActivos()
        {
            return await socioDatos.ListarActivos();
        }

        public async Task<Socio> ObtenerPorId(int socioId)
        {
            return await socioDatos.ObtenerPorId(socioId);
        }

        public async Task Insertar(Socio socio)
        {
            // Regla: No se permite repetir DNI
            if (await socioDatos.DNIExiste(socio.DNI))
            {
                throw new ReglaNegocioException($"El DNI '{socio.DNI}' ya está registrado.");
            }

            // Validar formato de DNI (8 dígitos)
            if (socio.DNI.Length != 8 || !long.TryParse(socio.DNI, out _))
            {
                throw new ReglaNegocioException("El DNI debe tener 8 dígitos numéricos.");
            }

            await socioDatos.Insertar(socio);
        }

        public async Task Actualizar(Socio socio)
        {
            // Regla: No se permite repetir DNI (excepto el mismo socio)
            if (await socioDatos.DNIExiste(socio.DNI, socio.SocioId))
            {
                throw new ReglaNegocioException($"El DNI '{socio.DNI}' ya está registrado en otro socio.");
            }

            await socioDatos.Actualizar(socio);
        }

        // Regla: Eliminación lógica, no se puede eliminar si tiene préstamos pendientes
        public async Task EliminarLogico(int socioId)
        {
            // Validar que no tenga préstamos pendientes
            if (await socioDatos.TienePrestamosPendientes(socioId))
            {
                throw new ReglaNegocioException("No se puede eliminar el socio porque tiene préstamos pendientes.");
            }

            await socioDatos.EliminarLogico(socioId);
        }
    }
}