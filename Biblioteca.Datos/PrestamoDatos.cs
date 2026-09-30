using Biblioteca.Entidades;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Biblioteca.Datos
{
    public class PrestamoDatos
    {
        private Conexion conexion;

        public PrestamoDatos()
        {
            conexion = new Conexion();
        }

        public async Task<List<Prestamo>> ListarPorFechas(DateTime fechaInicio, DateTime fechaFin)
        {
            List<Prestamo> lista = new List<Prestamo>();
            using (SqlConnection conn = conexion.ObtenerConexion())
            {
                await conn.OpenAsync();
                string query = @"SELECT p.PrestamoId, p.SocioId, p.FechaPrestamo, p.FechaLimite, p.Estado, s.Nombre AS NombreSocio
                                 FROM Prestamos p INNER JOIN Socios s ON p.SocioId = s.SocioId
                                 WHERE p.FechaPrestamo BETWEEN @FechaInicio AND @FechaFin";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@FechaInicio", fechaInicio);
                cmd.Parameters.AddWithValue("@FechaFin", fechaFin);

                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        lista.Add(new Prestamo
                        {
                            PrestamoId = (int)reader["PrestamoId"],
                            SocioId = (int)reader["SocioId"],
                            FechaPrestamo = (DateTime)reader["FechaPrestamo"],
                            FechaLimite = (DateTime)reader["FechaLimite"],
                            Estado = reader["Estado"].ToString(),
                            NombreSocio = reader["NombreSocio"].ToString()
                        });
                    }
                }
            }
            return lista;
        }

        public async Task<Prestamo> ObtenerPorId(int prestamoId)
        {
            using (SqlConnection conn = conexion.ObtenerConexion())
            {
                await conn.OpenAsync();
                string query = @"SELECT p.PrestamoId, p.SocioId, p.FechaPrestamo, p.FechaLimite, p.Estado, s.Nombre AS NombreSocio
                                 FROM Prestamos p INNER JOIN Socios s ON p.SocioId = s.SocioId
                                 WHERE p.PrestamoId = @PrestamoId";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@PrestamoId", prestamoId);

                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new Prestamo
                        {
                            PrestamoId = (int)reader["PrestamoId"],
                            SocioId = (int)reader["SocioId"],
                            FechaPrestamo = (DateTime)reader["FechaPrestamo"],
                            FechaLimite = (DateTime)reader["FechaLimite"],
                            Estado = reader["Estado"].ToString(),
                            NombreSocio = reader["NombreSocio"].ToString()
                        };
                    }
                }
            }
            return null;
        }

        public async Task<int> Insertar(int socioId, DateTime fechaLimite, SqlConnection conn, SqlTransaction transaction)
        {
            string query = "INSERT INTO Prestamos (SocioId, FechaPrestamo, FechaLimite, Estado) VALUES (@SocioId, GETDATE(), @FechaLimite, 'Pendiente'); SELECT CAST(SCOPE_IDENTITY() AS int);";
            SqlCommand cmd = new SqlCommand(query, conn, transaction);
            cmd.Parameters.AddWithValue("@SocioId", socioId);
            cmd.Parameters.AddWithValue("@FechaLimite", fechaLimite);

            object result = await cmd.ExecuteScalarAsync();
            return result != null ? Convert.ToInt32(result) : 0;
        }

        public async Task ActualizarEstado(int prestamoId, string estado, SqlConnection conn, SqlTransaction transaction)
        {
            string query = "UPDATE Prestamos SET Estado = @Estado WHERE PrestamoId = @PrestamoId";
            SqlCommand cmd = new SqlCommand(query, conn, transaction);
            cmd.Parameters.AddWithValue("@Estado", estado);
            cmd.Parameters.AddWithValue("@PrestamoId", prestamoId);

            await cmd.ExecuteNonQueryAsync();
        }
        public async Task<List<ReportePrestamo>> ListarReportePorFechas(DateTime fechaInicio, DateTime fechaFin)
        {
            List<ReportePrestamo> lista = new List<ReportePrestamo>();
            using (SqlConnection conn = conexion.ObtenerConexion())
            {
                await conn.OpenAsync();
                // INNER JOIN entre las 4 tablas como exige el Punto 14
                string query = @"SELECT s.Nombre AS NombreSocio, l.Titulo AS TituloLibro, 
                                p.FechaPrestamo, p.FechaLimite, p.Estado
                         FROM Prestamos p 
                         INNER JOIN Socios s ON p.SocioId = s.SocioId 
                         INNER JOIN DetallePrestamo dp ON p.PrestamoId = dp.PrestamoId 
                         INNER JOIN Libros l ON dp.LibroId = l.LibroId
                         WHERE p.FechaPrestamo BETWEEN @FechaInicio AND @FechaFin
                         ORDER BY p.FechaPrestamo DESC";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@FechaInicio", fechaInicio);
                cmd.Parameters.AddWithValue("@FechaFin", fechaFin);

                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        lista.Add(new ReportePrestamo
                        {
                            NombreSocio = reader["NombreSocio"].ToString(),
                            TituloLibro = reader["TituloLibro"].ToString(),
                            FechaPrestamo = (DateTime)reader["FechaPrestamo"],
                            FechaLimite = (DateTime)reader["FechaLimite"],
                            Estado = reader["Estado"].ToString()
                        });
                    }
                }
            }
            return lista;
        }
    }
}
