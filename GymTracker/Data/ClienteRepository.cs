using GymTracker.Models;
using MySqlConnector;
using System.Data;

namespace GymTracker.Data
{
    public class ClienteRepository
    {
        private readonly DatabaseConnection _db = DatabaseConnection.Instance;

        public List<Cliente> GetByEntrenador(int idEntrenador)
        {
            var list = new List<Cliente>();
            using var conn = _db.GetConnection();
            using var cmd = new MySqlCommand("sp_leer_clientes_por_entrenador", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("p_id_entrenador", idEntrenador);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                list.Add(new Cliente
                {
                    IdCliente = reader.GetInt32("id_cliente"),
                    Nombre = reader.GetString("nombre"),
                    IdEntrenador = idEntrenador
                });
            return list;
        }

        public void Crear(string nombre, int idEntrenador)
        {
            using var conn = _db.GetConnection();
            using var cmd = new MySqlCommand("sp_crear_cliente", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("p_nombre", nombre);
            cmd.Parameters.AddWithValue("p_id_entrenador", idEntrenador);
            cmd.ExecuteNonQuery();
        }

        public void Eliminar(int idCliente)
        {
            using var conn = _db.GetConnection();
            using var cmd = new MySqlCommand("sp_eliminar_cliente", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("p_id_cliente", idCliente);
            cmd.ExecuteNonQuery();
        }
    }
}
