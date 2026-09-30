using Biblioteca.Entidades;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Biblioteca.Datos
{
    public class LibroDatos
    {
        private Conexion conexion;

        public LibroDatos()
        {
            conexion = new Conexion();
        }

        public async Task<List<Libro>> ListarActivos()
        {
            List<Libro> lista = new List<Libro>();
            using (SqlConnection conn = conexion.ObtenerConexion())
            {
                await conn.OpenAsync();
                string query = @"SELECT l.LibroId, l.Titulo, l.ISBN, l.AutorId, l.Ejemplares, l.Activo, a.Nombre AS NombreAutor
                                 FROM Libros l INNER JOIN Autores a ON l.AutorId = a.AutorId
                                 WHERE l.Activo = 1";
                SqlCommand cmd = new SqlCommand(query, conn);

                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        lista.Add(new Libro
                        {
                            LibroId = (int)reader["LibroId"],
                            Titulo = reader["Titulo"].ToString(),
                            ISBN = reader["ISBN"].ToString(),
                            AutorId = (int)reader["AutorId"],
                            Ejemplares = (int)reader["Ejemplares"],
                            Activo = (bool)reader["Activo"],
                            NombreAutor = reader["NombreAutor"].ToString()
                        });
                    }
                }
            }
            return lista;
        }

        public async Task<Libro> ObtenerPorId(int libroId)
        {
            using (SqlConnection conn = conexion.ObtenerConexion())
            {
                await conn.OpenAsync();
                string query = @"SELECT l.LibroId, l.Titulo, l.ISBN, l.AutorId, l.Ejemplares, l.Activo, a.Nombre AS NombreAutor
                                 FROM Libros l INNER JOIN Autores a ON l.AutorId = a.AutorId
                                 WHERE l.LibroId = @LibroId";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@LibroId", libroId);

                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new Libro
                        {
                            LibroId = (int)reader["LibroId"],
                            Titulo = reader["Titulo"].ToString(),
                            ISBN = reader["ISBN"].ToString(),
                            AutorId = (int)reader["AutorId"],
                            Ejemplares = (int)reader["Ejemplares"],
                            Activo = (bool)reader["Activo"],
                            NombreAutor = reader["NombreAutor"].ToString()
                        };
                    }
                }
            }
            return null;
        }

        public async Task<bool> ISBNExiste(string isbn, int? libroId = null)
        {
            using (SqlConnection conn = conexion.ObtenerConexion())
            {
                await conn.OpenAsync();
                string query = "SELECT COUNT(*) FROM Libros WHERE ISBN = @ISBN";
                if (libroId.HasValue)
                    query += " AND LibroId != @LibroId";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ISBN", isbn);
                if (libroId.HasValue)
                    cmd.Parameters.AddWithValue("@LibroId", libroId.Value);

                int count = (int)await cmd.ExecuteScalarAsync();
                return count > 0;
            }
        }

        public async Task<bool> TienePrestamosPendientes(int libroId)
        {
            using (SqlConnection conn = conexion.ObtenerConexion())
            {
                await conn.OpenAsync();
                string query = @"SELECT COUNT(*) FROM DetallePrestamo dp
                                 INNER JOIN Prestamos p ON dp.PrestamoId = p.PrestamoId
                                 WHERE dp.LibroId = @LibroId AND p.Estado = 'Pendiente'";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@LibroId", libroId);

                int count = (int)await cmd.ExecuteScalarAsync();
                return count > 0;
            }
        }

        public async Task<bool> Insertar(Libro libro)
        {
            using (SqlConnection conn = conexion.ObtenerConexion())
            {
                await conn.OpenAsync();
                string query = "INSERT INTO Libros (Titulo, ISBN, AutorId, Ejemplares, Activo) VALUES (@Titulo, @ISBN, @AutorId, @Ejemplares, 1)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Titulo", libro.Titulo);
                cmd.Parameters.AddWithValue("@ISBN", libro.ISBN);
                cmd.Parameters.AddWithValue("@AutorId", libro.AutorId);
                cmd.Parameters.AddWithValue("@Ejemplares", libro.Ejemplares);

                int filas = await cmd.ExecuteNonQueryAsync();
                return filas > 0;
            }
        }

        public async Task<bool> Actualizar(Libro libro)
        {
            using (SqlConnection conn = conexion.ObtenerConexion())
            {
                await conn.OpenAsync();
                string query = "UPDATE Libros SET Titulo = @Titulo, ISBN = @ISBN, AutorId = @AutorId, Ejemplares = @Ejemplares WHERE LibroId = @LibroId";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@LibroId", libro.LibroId);
                cmd.Parameters.AddWithValue("@Titulo", libro.Titulo);
                cmd.Parameters.AddWithValue("@ISBN", libro.ISBN);
                cmd.Parameters.AddWithValue("@AutorId", libro.AutorId);
                cmd.Parameters.AddWithValue("@Ejemplares", libro.Ejemplares);

                int filas = await cmd.ExecuteNonQueryAsync();
                return filas > 0;
            }
        }

        public async Task<bool> EliminarLogico(int libroId)
        {
            using (SqlConnection conn = conexion.ObtenerConexion())
            {
                await conn.OpenAsync();
                string query = "UPDATE Libros SET Activo = 0 WHERE LibroId = @LibroId";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@LibroId", libroId);

                int filas = await cmd.ExecuteNonQueryAsync();
                return filas > 0;
            }
        }

        public async Task<bool> DescontarStock(int libroId)
        {
            using (SqlConnection conn = conexion.ObtenerConexion())
            {
                await conn.OpenAsync();
                string query = "UPDATE Libros SET Ejemplares = Ejemplares - 1 WHERE LibroId = @LibroId AND Ejemplares > 0";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@LibroId", libroId);

                int filas = await cmd.ExecuteNonQueryAsync();
                return filas > 0;
            }
        }

        public async Task<bool> AumentarStock(int libroId)
        {
            using (SqlConnection conn = conexion.ObtenerConexion())
            {
                await conn.OpenAsync();
                string query = "UPDATE Libros SET Ejemplares = Ejemplares + 1 WHERE LibroId = @LibroId";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@LibroId", libroId);

                int filas = await cmd.ExecuteNonQueryAsync();
                return filas > 0;
            }
        }

        // Overloads que usan la conexión y transacción proporcionadas (para operaciones dentro de una transacción)
        public async Task<bool> DescontarStock(int libroId, SqlConnection conn, SqlTransaction transaction)
        {
            string query = "UPDATE Libros SET Ejemplares = Ejemplares - 1 WHERE LibroId = @LibroId AND Ejemplares > 0";
            SqlCommand cmd = new SqlCommand(query, conn, transaction);
            cmd.Parameters.AddWithValue("@LibroId", libroId);

            int filas = await cmd.ExecuteNonQueryAsync();
            return filas > 0;
        }

        public async Task<bool> AumentarStock(int libroId, SqlConnection conn, SqlTransaction transaction)
        {
            string query = "UPDATE Libros SET Ejemplares = Ejemplares + 1 WHERE LibroId = @LibroId";
            SqlCommand cmd = new SqlCommand(query, conn, transaction);
            cmd.Parameters.AddWithValue("@LibroId", libroId);

            int filas = await cmd.ExecuteNonQueryAsync();
            return filas > 0;
        }
    }
}