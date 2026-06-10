namespace GymTracker.Models
{
    public class RutinaEjercicio
    {
        public int IdEjercicio { get; set; }
        public string NombreEjercicio { get; set; } = string.Empty;
        public int Series { get; set; }
        public int RepsObjetivo { get; set; }
        public byte[]? Foto { get; set; }
    }
}
