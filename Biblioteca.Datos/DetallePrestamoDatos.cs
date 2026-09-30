using Biblioteca.Entidades;
using Microsoft.Data.SqlClient;
using System;
using System.Threading.Tasks;

namespace Biblioteca.Datos
{
    public class DetallePrestamoDatos
    {
        private Conexion conexion;

        public DetallePrestamoDatos()
        {
            conexion = new Conexion();
        }

        public async Task<DetallePrestamo> ObtenerPorPrestamoYLibro(int prestamoId, int libroId)
        {
            using (SqlConnection conn = conexion.ObtenerConexion())
            {
                await conn.OpenAsync();
                string query = @"SELECT dp.PrestamoId, dp.LibroId, dp.FechaDevolucion, l.Titulo AS TituloLibro
                                 FROM DetallePrestamo dp LEFT JOIN Libros l ON dp.LibroId = l.LibroId
                                 WHERE dp.PrestamoId = @PrestamoId AND dp.LibroId = @LibroId";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@PrestamoId", prestamoId);
                cmd.Parameters.AddWithValue("@LibroId", libroId);

                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new DetallePrestamo
                        {
                            PrestamoId = (int)reader["PrestamoId"],
                            LibroId = (int)reader["LibroId"],
                            FechaDevolucion = reader["FechaDevolucion"] == DBNull.Value ? (DateTime?)null : (DateTime)reader["FechaDevolucion"],
                            TituloLibro = reader["TituloLibro"].ToString()
                        };
                    }
                }
            }
            return null;
        }

        public async Task Insertar(int prestamoId, int libroId, SqlConnection conn, SqlTransaction transaction)
        {
            string query = "INSERT INTO DetallePrestamo (PrestamoId, LibroId, FechaDevolucion) VALUES (@PrestamoId, @LibroId, NULL)";
            SqlCommand cmd = new SqlCommand(query, conn, transaction);
            cmd.Parameters.AddWithValue("@PrestamoId", prestamoId);
            cmd.Parameters.AddWithValue("@LibroId", libroId);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task ActualizarFechaDevolucion(int prestamoId, int libroId, DateTime fechaDevolucion, SqlConnection conn, SqlTransaction transaction)
        {
            string query = "UPDATE DetallePrestamo SET FechaDevolucion = @FechaDevolucion WHERE PrestamoId = @PrestamoId AND LibroId = @LibroId";
            SqlCommand cmd = new SqlCommand(query, conn, transaction);
            cmd.Parameters.AddWithValue("@FechaDevolucion", fechaDevolucion);
            cmd.Parameters.AddWithValue("@PrestamoId", prestamoId);
            cmd.Parameters.AddWithValue("@LibroId", libroId);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<int> ContarPendientes(int prestamoId, SqlConnection conn, SqlTransaction transaction)
        {
            string query = "SELECT COUNT(*) FROM DetallePrestamo WHERE PrestamoId = @PrestamoId AND FechaDevolucion IS NULL";
            SqlCommand cmd = new SqlCommand(query, conn, transaction);
            cmd.Parameters.AddWithValue("@PrestamoId", prestamoId);

            int count = (int)await cmd.ExecuteScalarAsync();
            return count;
        }
    }
}
