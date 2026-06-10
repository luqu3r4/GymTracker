using GymTracker.Data;
using GymTracker.Models;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace GymTracker.ViewModels
{
    public class RutinasViewModel : ViewModelBase
    {
        private readonly RutinaRepository _rutinaRepo = new();
        private readonly EjercicioRepository _ejercicioRepo = new();

        private Rutina? _rutinaSeleccionada;
        private RutinaEjercicio? _ejercicioRutinaSeleccionado;
        private Ejercicio? _ejercicioParaAgregar;
        private string _seriesInput = "3";
        private string _repsInput = "10";
        private string _mensajeError = string.Empty;

        public ObservableCollection<Rutina> Rutinas { get; } = new();
        public ObservableCollection<RutinaEjercicio> EjerciciosRutina { get; } = new();
        public ObservableCollection<Ejercicio> EjerciciosDisponibles { get; } = new();

        public Rutina? RutinaSeleccionada
        {
            get => _rutinaSeleccionada;
            set
            {
                SetProperty(ref _rutinaSeleccionada, value);
                CargarEjerciciosRutina();
            }
        }

        public RutinaEjercicio? EjercicioRutinaSeleccionado
        {
            get => _ejercicioRutinaSeleccionado;
            set => SetProperty(ref _ejercicioRutinaSeleccionado, value);
        }

        public Ejercicio? EjercicioParaAgregar
        {
            get => _ejercicioParaAgregar;
            set => SetProperty(ref _ejercicioParaAgregar, value);
        }

        public string SeriesInput
        {
            get => _seriesInput;
            set => SetProperty(ref _seriesInput, value);
        }

        public string RepsInput
        {
            get => _repsInput;
            set => SetProperty(ref _repsInput, value);
        }

        public string MensajeError
        {
            get => _mensajeError;
            set
            {
                SetProperty(ref _mensajeError, value);
                OnPropertyChanged(nameof(HayError));
            }
        }

        public bool HayError => !string.IsNullOrEmpty(MensajeError);

        public ICommand NuevaRutinaCommand { get; }
        public ICommand EliminarRutinaCommand { get; }
        public ICommand AgregarEjercicioCommand { get; }
        public ICommand QuitarEjercicioCommand { get; }

        public Func<string?>? PedirNombreAction { get; set; }

        public RutinasViewModel()
        {
            NuevaRutinaCommand = new RelayCommand(_ => NuevaRutina());
            EliminarRutinaCommand = new RelayCommand(_ => EliminarRutina(), _ => RutinaSeleccionada != null);
            AgregarEjercicioCommand = new RelayCommand(_ => AgregarEjercicio(),
                _ => RutinaSeleccionada != null && EjercicioParaAgregar != null);
            QuitarEjercicioCommand = new RelayCommand(_ => QuitarEjercicio(),
                _ => RutinaSeleccionada != null && EjercicioRutinaSeleccionado != null);

            CargarRutinas();
        }

        private void CargarRutinas()
        {
            Rutinas.Clear();
            foreach (var r in _rutinaRepo.GetByEntrenador(Session.EntrenadorActivo!.IdEntrenador))
                Rutinas.Add(r);
        }

        private void CargarEjerciciosRutina()
        {
            EjerciciosRutina.Clear();
            EjerciciosDisponibles.Clear();
            MensajeError = string.Empty;

            if (RutinaSeleccionada is null) return;

            var enRutina = _rutinaRepo.GetEjercicios(RutinaSeleccionada.IdRutina);
            foreach (var e in enRutina)
                EjerciciosRutina.Add(e);

            var idsEnRutina = enRutina.Select(e => e.IdEjercicio).ToHashSet();
            foreach (var e in _ejercicioRepo.GetAll())
                if (!idsEnRutina.Contains(e.IdEjercicio))
                    EjerciciosDisponibles.Add(e);
        }

        private void NuevaRutina()
        {
            var nombre = PedirNombreAction?.Invoke();
            if (string.IsNullOrWhiteSpace(nombre)) return;

            _rutinaRepo.Crear(nombre.Trim(), Session.EntrenadorActivo!.IdEntrenador);
            CargarRutinas();
        }

        private void EliminarRutina()
        {
            if (RutinaSeleccionada is null) return;

            var result = MessageBox.Show(
                $"¿Eliminar la rutina '{RutinaSeleccionada.Nombre}'?",
                "Confirmar eliminación", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                _rutinaRepo.Eliminar(RutinaSeleccionada.IdRutina);
                CargarRutinas();
            }
        }

        private void AgregarEjercicio()
        {
            if (RutinaSeleccionada is null || EjercicioParaAgregar is null) return;

            if (!int.TryParse(SeriesInput, out int series) || series <= 0)
            {
                MensajeError = "Series inválidas.";
                return;
            }
            if (!int.TryParse(RepsInput, out int reps) || reps <= 0)
            {
                MensajeError = "Repeticiones inválidas.";
                return;
            }

            _rutinaRepo.AgregarEjercicio(RutinaSeleccionada.IdRutina, EjercicioParaAgregar.IdEjercicio, series, reps);
            EjercicioParaAgregar = null;
            SeriesInput = "3";
            RepsInput = "10";
            MensajeError = string.Empty;
            CargarEjerciciosRutina();
        }

        private void QuitarEjercicio()
        {
            if (RutinaSeleccionada is null || EjercicioRutinaSeleccionado is null) return;

            _rutinaRepo.QuitarEjercicio(RutinaSeleccionada.IdRutina, EjercicioRutinaSeleccionado.IdEjercicio);
            CargarEjerciciosRutina();
        }
    }
}
