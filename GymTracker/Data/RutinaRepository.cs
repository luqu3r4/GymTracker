using GymTracker.Models;
using MySqlConnector;
using System.Data;

namespace GymTracker.Data
{
    public class RutinaRepository
    {
        private readonly DatabaseConnection _db = DatabaseConnection.Instance;

        public List<Rutina> GetByEntrenador(int idEntrenador)
        {
            var list = new List<Rutina>();
            using var conn = _db.GetConnection();
            using var cmd = new MySqlCommand("sp_listar_rutinas_por_entrenador", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("p_id_entrenador", idEntrenador);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                list.Add(new Rutina
                {
                    IdRutina = reader.GetInt32("id_rutina"),
                    Nombre = reader.GetString("nombre"),
                    IdEntrenador = idEntrenador
                });
            return list;
        }

        public void Crear(string nombre, int idEntrenador)
        {
            using var conn = _db.GetConnection();
            using var cmd = new MySqlCommand("sp_crear_rutina", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("p_nombre", nombre);
            cmd.Parameters.AddWithValue("p_id_entrenador", idEntrenador);
            cmd.ExecuteNonQuery();
        }

        public void Eliminar(int idRutina)
        {
            using var conn = _db.GetConnection();
            using var cmd = new MySqlCommand("sp_eliminar_rutina", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("p_id_rutina", idRutina);
            cmd.ExecuteNonQuery();
        }

        public List<RutinaEjercicio> GetEjercicios(int idRutina)
        {
            var list = new List<RutinaEjercicio>();
            using var conn = _db.GetConnection();
            using var cmd = new MySqlCommand("sp_listar_ejercicios_rutina", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("p_id_rutina", idRutina);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                list.Add(new RutinaEjercicio
                {
                    IdEjercicio = reader.GetInt32("id_ejercicio"),
                    NombreEjercicio = reader.GetString("nombre"),
                    Series = reader.GetInt32("series"),
                    RepsObjetivo = reader.GetInt32("reps_objetivo")
                });
            return list;
        }

        public void AgregarEjercicio(int idRutina, int idEjercicio, int series, int reps)
        {
            using var conn = _db.GetConnection();
            using var cmd = new MySqlCommand("sp_agregar_ejercicio_rutina", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("p_id_rutina", idRutina);
            cmd.Parameters.AddWithValue("p_id_ejercicio", idEjercicio);
            cmd.Parameters.AddWithValue("p_series", series);
            cmd.Parameters.AddWithValue("p_reps", reps);
            cmd.ExecuteNonQuery();
        }

        public void QuitarEjercicio(int idRutina, int idEjercicio)
        {
            using var conn = _db.GetConnection();
            using var cmd = new MySqlCommand("sp_quitar_ejercicio_rutina", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("p_id_rutina", idRutina);
            cmd.Parameters.AddWithValue("p_id_ejercicio", idEjercicio);
            cmd.ExecuteNonQuery();
        }

        public void AsignarRutinaCliente(int idCliente, int idRutina)
        {
            using var conn = _db.GetConnection();
            using var cmd = new MySqlCommand("sp_asignar_rutina_cliente", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("p_id_cliente", idCliente);
            cmd.Parameters.AddWithValue("p_id_rutina", idRutina);
            cmd.ExecuteNonQuery();
        }

        public void QuitarRutinaCliente(int idCliente)
        {
            using var conn = _db.GetConnection();
            using var cmd = new MySqlCommand("sp_quitar_rutina_cliente", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("p_id_cliente", idCliente);
            cmd.ExecuteNonQuery();
        }
    }
}
