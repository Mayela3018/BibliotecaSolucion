using Biblioteca.Entidades;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Biblioteca.Datos
{
    public class AutorDatos
    {
        private Conexion conexion;

        public AutorDatos()
        {
            conexion = new Conexion();
        }

        public async Task<List<Autor>> ListarActivos()
        {
            List<Autor> lista = new List<Autor>();
            using (SqlConnection conn = conexion.ObtenerConexion())
            {
                await conn.OpenAsync();
                string query = "SELECT AutorId, Nombre, Nacionalidad, Activo FROM Autores WHERE Activo = 1";
                SqlCommand cmd = new SqlCommand(query, conn);

                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        lista.Add(new Autor
                        {
                            AutorId = (int)reader["AutorId"],
                            Nombre = reader["Nombre"].ToString(),
                            Nacionalidad = reader["Nacionalidad"].ToString(),
                            Activo = (bool)reader["Activo"]
                        });
                    }
                }
            }
            return lista;
        }

        public async Task<Autor> ObtenerPorId(int autorId)
        {
            using (SqlConnection conn = conexion.ObtenerConexion())
            {
                await conn.OpenAsync();
                string query = "SELECT AutorId, Nombre, Nacionalidad, Activo FROM Autores WHERE AutorId = @AutorId";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@AutorId", autorId);

                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new Autor
                        {
                            AutorId = (int)reader["AutorId"],
                            Nombre = reader["Nombre"].ToString(),
                            Nacionalidad = reader["Nacionalidad"].ToString(),
                            Activo = (bool)reader["Activo"]
                        };
                    }
                }
            }
            return null;
        }

        public async Task<bool> Insertar(Autor autor)
        {
            using (SqlConnection conn = conexion.ObtenerConexion())
            {
                await conn.OpenAsync();
                string query = "INSERT INTO Autores (Nombre, Nacionalidad, Activo) VALUES (@Nombre, @Nacionalidad, 1)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Nombre", autor.Nombre);
                cmd.Parameters.AddWithValue("@Nacionalidad", autor.Nacionalidad);

                int filas = await cmd.ExecuteNonQueryAsync();
                return filas > 0;
            }
        }

        public async Task<bool> Actualizar(Autor autor)
        {
            using (SqlConnection conn = conexion.ObtenerConexion())
            {
                await conn.OpenAsync();
                string query = "UPDATE Autores SET Nombre = @Nombre, Nacionalidad = @Nacionalidad WHERE AutorId = @AutorId";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@AutorId", autor.AutorId);
                cmd.Parameters.AddWithValue("@Nombre", autor.Nombre);
                cmd.Parameters.AddWithValue("@Nacionalidad", autor.Nacionalidad);

                int filas = await cmd.ExecuteNonQueryAsync();
                return filas > 0;
            }
        }

        public async Task<bool> EliminarLogico(int autorId)
        {
            using (SqlConnection conn = conexion.ObtenerConexion())
            {
                await conn.OpenAsync();
                string query = "UPDATE Autores SET Activo = 0 WHERE AutorId = @AutorId";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@AutorId", autorId);

                int filas = await cmd.ExecuteNonQueryAsync();
                return filas > 0;
            }
        }
    }
}