using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Protocols;
using System.Configuration;

namespace Biblioteca.Datos
{
    public class Conexion
    {
        private string cadenaConexion;

        public Conexion()
        {
            // Lee la cadena de conexión del App.config
            cadenaConexion = ConfigurationManager.ConnectionStrings["BibliotecaDB"].ConnectionString;
        }

        public SqlConnection ObtenerConexion()
        {
            return new SqlConnection(cadenaConexion);
        }
    }
}