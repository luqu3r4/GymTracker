using GymTracker.Models;
using MySqlConnector;
using System.Data;

namespace GymTracker.Data
{
    public class EjercicioRepository
    {
        private readonly DatabaseConnection _db = DatabaseConnection.Instance;

        public List<Ejercicio> GetAll()
        {
            var list = new List<Ejercicio>();
            using var conn = _db.GetConnection();
            using var cmd = new MySqlCommand("sp_listar_ejercicios", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                list.Add(new Ejercicio
                {
                    IdEjercicio = reader.GetInt32("id_ejercicio"),
                    Nombre = reader.GetString("nombre"),
                    Foto = reader.IsDBNull(reader.GetOrdinal("foto")) ? null : (byte[])reader["foto"]
                });
            return list;
        }

        public void Crear(string nombre, byte[]? foto)
        {
            using var conn = _db.GetConnection();
            using var cmd = new MySqlCommand("sp_crear_ejercicio", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("p_nombre", nombre);
            var p = cmd.Parameters.Add("p_foto", MySqlDbType.MediumBlob);
            p.Value = foto is null ? DBNull.Value : (object)foto;
            cmd.ExecuteNonQuery();
        }

        public void Actualizar(int id, string nombre, byte[]? foto)
        {
            using var conn = _db.GetConnection();
            using var cmd = new MySqlCommand("sp_actualizar_ejercicio", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("p_id", id);
            cmd.Parameters.AddWithValue("p_nombre", nombre);
            var p = cmd.Parameters.Add("p_foto", MySqlDbType.MediumBlob);
            p.Value = foto is null ? DBNull.Value : (object)foto;
            cmd.ExecuteNonQuery();
        }

        public void Eliminar(int id)
        {
            using var conn = _db.GetConnection();
            using var cmd = new MySqlCommand("sp_eliminar_ejercicio", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("p_id", id);
            cmd.ExecuteNonQuery();
        }
    }
}
