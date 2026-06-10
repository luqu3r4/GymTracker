using MySqlConnector;
using System.Data;

namespace GymTracker.Data
{
    public sealed class DatabaseConnection
    {
        private const string ConnectionString =
            "Server=localhost;Port=3306;Database=gymtracker;" +
            "User ID=root;Password=alumno;CharSet=utf8mb4;";

        private static DatabaseConnection? _instance;
        private static readonly object _lock = new();

        private DatabaseConnection() { }

        public static DatabaseConnection Instance
        {
            get
            {
                if (_instance is null)
                {
                    lock (_lock)
                    {
                        _instance ??= new DatabaseConnection();
                    }
                }
                return _instance;
            }
        }

        public MySqlConnection GetConnection()
        {
            var conn = new MySqlConnection(ConnectionString);
            conn.Open();
            return conn;
        }

        public (int Id, string Nombre)? LoginEntrenador(string pin)
        {
            using var conn = GetConnection();
            using var cmd = new MySqlCommand("sp_login_entrenador", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("p_pin", pin);
            cmd.Parameters.Add("p_id", MySqlDbType.Int32).Direction = ParameterDirection.Output;
            cmd.Parameters.Add("p_nombre", MySqlDbType.VarChar, 100).Direction = ParameterDirection.Output;

            cmd.ExecuteNonQuery();

            if (cmd.Parameters["p_id"].Value is DBNull) return null;

            return (
                Convert.ToInt32(cmd.Parameters["p_id"].Value),
                cmd.Parameters["p_nombre"].Value!.ToString()!
            );
        }
    }
}
