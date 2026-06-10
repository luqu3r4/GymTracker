using GymTracker.Models;
using MySqlConnector;

namespace GymTracker.Data
{
    public class EntrenadorRepository
    {
        private readonly DatabaseConnection _db = DatabaseConnection.Instance;

        public List<Entrenador> GetAll()
        {
            var list = new List<Entrenador>();
            using var conn = _db.GetConnection();
            using var cmd = new MySqlCommand(
                "SELECT id_entrenador, nombre FROM entrenadores ORDER BY nombre", conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Entrenador
                {
                    IdEntrenador = reader.GetInt32("id_entrenador"),
                    Nombre = reader.GetString("nombre")
                });
            }
            return list;
        }

        public Entrenador? VerificarLogin(int idEntrenador, string pin)
        {
            var result = _db.LoginEntrenador(pin);
            if (result is null || result.Value.Id != idEntrenador) return null;

            return new Entrenador
            {
                IdEntrenador = result.Value.Id,
                Nombre = result.Value.Nombre
            };
        }
    }
}
