namespace GymTracker.Models
{
    public class Cliente
    {
        public int IdCliente { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int IdEntrenador { get; set; }
    }
}
