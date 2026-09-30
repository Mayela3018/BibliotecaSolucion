using Biblioteca.Entidades;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Biblioteca.Datos
{
    public class SocioDatos
    {
        private Conexion conexion;

        public SocioDatos()
        {
            conexion = new Conexion();
        }

        public async Task<List<Socio>> ListarActivos()
        {
            List<Socio> lista = new List<Socio>();
            using (SqlConnection conn = conexion.ObtenerConexion())
            {
                await conn.OpenAsync();
                string query = "SELECT SocioId, DNI, Nombre, Email, Activo FROM Socios WHERE Activo = 1";
                SqlCommand cmd = new SqlCommand(query, conn);

                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        lista.Add(new Socio
                        {
                            SocioId = (int)reader["SocioId"],
                            DNI = reader["DNI"].ToString(),
                            Nombre = reader["Nombre"].ToString(),
                            Email = reader["Email"].ToString(),
                            Activo = (bool)reader["Activo"]
                        });
                    }
                }
            }
            return lista;
        }

        public async Task<Socio> ObtenerPorId(int socioId)
        {
            using (SqlConnection conn = conexion.ObtenerConexion())
            {
                await conn.OpenAsync();
                string query = "SELECT SocioId, DNI, Nombre, Email, Activo FROM Socios WHERE SocioId = @SocioId";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@SocioId", socioId);

                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new Socio
                        {
                            SocioId = (int)reader["SocioId"],
                            DNI = reader["DNI"].ToString(),
                            Nombre = reader["Nombre"].ToString(),
                            Email = reader["Email"].ToString(),
                            Activo = (bool)reader["Activo"]
                        };
                    }
                }
            }
            return null;
        }

        public async Task<bool> DNIExiste(string dni, int? socioId = null)
        {
            using (SqlConnection conn = conexion.ObtenerConexion())
            {
                await conn.OpenAsync();
                string query = "SELECT COUNT(*) FROM Socios WHERE DNI = @DNI";
                if (socioId.HasValue)
                    query += " AND SocioId != @SocioId";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@DNI", dni);
                if (socioId.HasValue)
                    cmd.Parameters.AddWithValue("@SocioId", socioId.Value);

                int count = (int)await cmd.ExecuteScalarAsync();
                return count > 0;
            }
        }

        public async Task<bool> TienePrestamosPendientes(int socioId)
        {
            using (SqlConnection conn = conexion.ObtenerConexion())
            {
                await conn.OpenAsync();
                string query = "SELECT COUNT(*) FROM Prestamos WHERE SocioId = @SocioId AND Estado = 'Pendiente'";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@SocioId", socioId);

                int count = (int)await cmd.ExecuteScalarAsync();
                return count > 0;
            }
        }

        public async Task<int> ContarPrestamosPendientes(int socioId)
        {
            using (SqlConnection conn = conexion.ObtenerConexion())
            {
                await conn.OpenAsync();
                string query = "SELECT COUNT(*) FROM Prestamos WHERE SocioId = @SocioId AND Estado = 'Pendiente'";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@SocioId", socioId);

                int count = (int)await cmd.ExecuteScalarAsync();
                return count;
            }
        }

        public async Task<bool> Insertar(Socio socio)
        {
            using (SqlConnection conn = conexion.ObtenerConexion())
            {
                await conn.OpenAsync();
                string query = "INSERT INTO Socios (DNI, Nombre, Email, Activo) VALUES (@DNI, @Nombre, @Email, 1)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@DNI", socio.DNI);
                cmd.Parameters.AddWithValue("@Nombre", socio.Nombre);
                cmd.Parameters.AddWithValue("@Email", socio.Email);

                int filas = await cmd.ExecuteNonQueryAsync();
                return filas > 0;
            }
        }

        public async Task<bool> Actualizar(Socio socio)
        {
            using (SqlConnection conn = conexion.ObtenerConexion())
            {
                await conn.OpenAsync();
                string query = "UPDATE Socios SET DNI = @DNI, Nombre = @Nombre, Email = @Email WHERE SocioId = @SocioId";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@SocioId", socio.SocioId);
                cmd.Parameters.AddWithValue("@DNI", socio.DNI);
                cmd.Parameters.AddWithValue("@Nombre", socio.Nombre);
                cmd.Parameters.AddWithValue("@Email", socio.Email);

                int filas = await cmd.ExecuteNonQueryAsync();
                return filas > 0;
            }
        }

        public async Task<bool> EliminarLogico(int socioId)
        {
            using (SqlConnection conn = conexion.ObtenerConexion())
            {
                await conn.OpenAsync();
                string query = "UPDATE Socios SET Activo = 0 WHERE SocioId = @SocioId";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@SocioId", socioId);

                int filas = await cmd.ExecuteNonQueryAsync();
                return filas > 0;
            }
        }
    }
}