using GymTracker.Models;
using MySqlConnector;
using System.Data;

namespace GymTracker.Data
{
    public class SeguimientoRepository
    {
        private readonly DatabaseConnection _db = DatabaseConnection.Instance;

        public List<Seguimiento> GetByClienteEjercicio(int idCliente, int idEjercicio)
        {
            var list = new List<Seguimiento>();
            using var conn = _db.GetConnection();
            using var cmd = new MySqlCommand("sp_leer_seguimiento_por_cliente", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("p_id_cliente", idCliente);
            cmd.Parameters.AddWithValue("p_id_ejercicio", idEjercicio);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                list.Add(new Seguimiento
                {
                    IdSeguimiento = reader.GetInt32("id_seguimiento"),
                    IdCliente = idCliente,
                    IdEjercicio = idEjercicio,
                    Ejercicio = reader.GetString("ejercicio"),
                    Peso = reader.GetDecimal("peso"),
                    Repeticiones = reader.GetInt32("repeticiones"),
                    Fecha = reader.GetDateTime("fecha")
                });
            return list;
        }

        public List<Seguimiento> GetAllByCliente(int idCliente)
        {
            var list = new List<Seguimiento>();
            using var conn = _db.GetConnection();
            using var cmd = new MySqlCommand(
                "SELECT id_seguimiento, id_cliente, id_ejercicio, ejercicio, peso, repeticiones, fecha " +
                "FROM v_seguimiento_detalle WHERE id_cliente = @id ORDER BY ejercicio, fecha", conn);
            cmd.Parameters.AddWithValue("@id", idCliente);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                list.Add(new Seguimiento
                {
                    IdSeguimiento = reader.GetInt32("id_seguimiento"),
                    IdCliente = reader.GetInt32("id_cliente"),
                    IdEjercicio = reader.GetInt32("id_ejercicio"),
                    Ejercicio = reader.GetString("ejercicio"),
                    Peso = reader.GetDecimal("peso"),
                    Repeticiones = reader.GetInt32("repeticiones"),
                    Fecha = reader.GetDateTime("fecha")
                });
            return list;
        }

        public void Crear(int idCliente, int idEjercicio, decimal peso, int repeticiones, DateTime fecha)
        {
            using var conn = _db.GetConnection();
            using var cmd = new MySqlCommand("sp_crear_seguimiento", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("p_id_cliente", idCliente);
            cmd.Parameters.AddWithValue("p_id_ejercicio", idEjercicio);
            cmd.Parameters.AddWithValue("p_peso", peso);
            cmd.Parameters.AddWithValue("p_repeticiones", repeticiones);
            cmd.Parameters.AddWithValue("p_fecha", fecha.Date);
            cmd.ExecuteNonQuery();
        }

        public void Eliminar(int idSeguimiento)
        {
            using var conn = _db.GetConnection();
            using var cmd = new MySqlCommand("sp_eliminar_seguimiento", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("p_id_seguimiento", idSeguimiento);
            cmd.ExecuteNonQuery();
        }
    }
}
