using Biblioteca.Datos;
using Biblioteca.Entidades;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;

namespace Biblioteca.Negocio
{
    public class PrestamoNegocio
    {
        private PrestamoDatos prestamoDatos;
        private DetallePrestamoDatos detalleDatos;
        private LibroDatos libroDatos;
        private SocioDatos socioDatos;

        // Multa por día de retraso (Regla de negocio)
        private const decimal MULTA_POR_DIA = 1.50m;

        public PrestamoNegocio()
        {
            prestamoDatos = new PrestamoDatos();
            detalleDatos = new DetallePrestamoDatos();
            libroDatos = new LibroDatos();
            socioDatos = new SocioDatos();
        }

        public async Task<List<Prestamo>> ListarPorFechas(DateTime fechaInicio, DateTime fechaFin)
        {
            return await prestamoDatos.ListarPorFechas(fechaInicio, fechaFin);
        }

        public async Task<List<ReportePrestamo>> ListarReportePorFechas(DateTime fechaInicio, DateTime fechaFin)
        {
            return await prestamoDatos.ListarReportePorFechas(fechaInicio, fechaFin);
        }

        // Regla: Un socio no puede tener más de 3 libros pendientes
        // Regla: No se presta un libro sin ejemplares
        // Regla: No se presta a socios o libros con Activo = 0
        // Regla: Transacción (cabecera, detalles y descuento de stock van juntos)
        public async Task RegistrarPrestamo(int socioId, List<int> libroIds, DateTime fechaLimite)
        {
            // Validar que el socio esté activo
            Socio socio = await socioDatos.ObtenerPorId(socioId);
            if (socio == null || !socio.Activo)
            {
                throw new ReglaNegocioException("El socio no está activo o no existe.");
            }

            // Validar que no tenga más de 2 préstamos pendientes (para que al agregar este no pase de 3)
            int prestamosPendientes = await socioDatos.ContarPrestamosPendientes(socioId);
            if (prestamosPendientes >= 3)
            {
                throw new ReglaNegocioException("El socio ya tiene 3 libros pendientes. No puede solicitar más préstamos.");
            }

            // Validar cada libro
            foreach (int libroId in libroIds)
            {
                Libro libro = await libroDatos.ObtenerPorId(libroId);
                if (libro == null || !libro.Activo)
                {
                    throw new ReglaNegocioException($"El libro con ID {libroId} no está activo o no existe.");
                }

                if (libro.Ejemplares <= 0)
                {
                    throw new ReglaNegocioException($"El libro '{libro.Titulo}' no tiene ejemplares disponibles.");
                }
            }

            // Ejecutar transacción
            string cadenaConexion = ConfigurationManager.ConnectionStrings["BibliotecaDB"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(cadenaConexion))
            {
                await conn.OpenAsync();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    // 1. Insertar cabecera del préstamo
                    int prestamoId = await prestamoDatos.Insertar(socioId, fechaLimite, conn, transaction);

                    // 2. Insertar detalles y descontar stock
                    foreach (int libroId in libroIds)
                    {
                        await detalleDatos.Insertar(prestamoId, libroId, conn, transaction);
                        await libroDatos.DescontarStock(libroId, conn, transaction);
                    }

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw new ReglaNegocioException("Error al registrar el préstamo. La operación fue cancelada.");
                }
            }
        }

        // Regla: Guardar FechaDevolucion, devolver ejemplar al stock, calcular multa
        // Regla: Cuando no quedan libros pendientes, el préstamo pasa a Devuelto
        public async Task<decimal> RegistrarDevolucion(int prestamoId, int libroId)
        {
            DetallePrestamo detalle = await detalleDatos.ObtenerPorPrestamoYLibro(prestamoId, libroId);

            if (detalle == null)
            {
                throw new ReglaNegocioException("No se encontró el detalle del préstamo.");
            }

            if (detalle.FechaDevolucion.HasValue)
            {
                throw new ReglaNegocioException("Este libro ya fue devuelto.");
            }

            // Obtener fecha límite del préstamo
            Prestamo prestamo = await prestamoDatos.ObtenerPorId(prestamoId);
            DateTime fechaLimite = prestamo.FechaLimite;
            DateTime fechaDevolucion = DateTime.Now.Date;

            // Calcular multa si hay retraso
            decimal multa = 0;
            if (fechaDevolucion > fechaLimite)
            {
                int diasRetraso = (fechaDevolucion - fechaLimite).Days;
                multa = diasRetraso * MULTA_POR_DIA;
            }

            // Ejecutar transacción
            string cadenaConexion = ConfigurationManager.ConnectionStrings["BibliotecaDB"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(cadenaConexion))
            {
                await conn.OpenAsync();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    // 1. Guardar fecha de devolución
                    await detalleDatos.ActualizarFechaDevolucion(prestamoId, libroId, fechaDevolucion, conn, transaction);

                    // 2. Devolver ejemplar al stock
                    await libroDatos.AumentarStock(libroId, conn, transaction);

                    // 3. Verificar si quedan libros pendientes en este préstamo
                    int pendientes = await detalleDatos.ContarPendientes(prestamoId, conn, transaction);
                    if (pendientes == 0)
                    {
                        await prestamoDatos.ActualizarEstado(prestamoId, "Devuelto", conn, transaction);
                    }

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw new ReglaNegocioException("Error al registrar la devolución. La operación fue cancelada.");
                }
            }

            return multa;
        }
    }
}