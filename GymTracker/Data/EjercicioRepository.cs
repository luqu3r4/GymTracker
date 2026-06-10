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
                    Foto = reader.IsDBNull(reader.GetOrdinal("foto")) ? null : reader.GetString("foto")
                });
            return list;
        }
    }
}
