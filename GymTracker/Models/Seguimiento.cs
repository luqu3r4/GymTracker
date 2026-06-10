namespace GymTracker.Models
{
    public class Seguimiento
    {
        public int IdSeguimiento { get; set; }
        public int IdCliente { get; set; }
        public int IdEjercicio { get; set; }
        public string Ejercicio { get; set; } = string.Empty;
        public decimal Peso { get; set; }
        public int Repeticiones { get; set; }
        public DateTime Fecha { get; set; }
    }
}
