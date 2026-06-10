namespace GymTracker.Models
{
    public class Ejercicio
    {
        public int IdEjercicio { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Foto { get; set; }

        public override string ToString() => Nombre;
    }
}
