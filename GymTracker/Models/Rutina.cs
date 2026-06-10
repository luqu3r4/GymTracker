namespace GymTracker.Models
{
    public class Rutina
    {
        public int IdRutina { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int IdEntrenador { get; set; }

        public override string ToString() => Nombre;
    }
}
