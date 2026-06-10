using GymTracker.Data;
using GymTracker.Models;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace GymTracker.ViewModels
{
    public class EjerciciosViewModel : ViewModelBase
    {
        private readonly EjercicioRepository _repo = new();

        private Ejercicio? _ejercicioSeleccionado;
        private string _nombreInput = string.Empty;
        private byte[]? _fotoPreview;
        private bool _esNuevo;

        public ObservableCollection<Ejercicio> Ejercicios { get; } = new();

        public Ejercicio? EjercicioSeleccionado
        {
            get => _ejercicioSeleccionado;
            set
            {
                SetProperty(ref _ejercicioSeleccionado, value);
                if (value != null)
                {
                    NombreInput = value.Nombre;
                    FotoPreview = value.Foto;
                    EsNuevo = false;
                }
            }
        }

        public string NombreInput
        {
            get => _nombreInput;
            set => SetProperty(ref _nombreInput, value);
        }

        public byte[]? FotoPreview
        {
            get => _fotoPreview;
            set => SetProperty(ref _fotoPreview, value);
        }

        public bool EsNuevo
        {
            get => _esNuevo;
            set
            {
                SetProperty(ref _esNuevo, value);
                OnPropertyChanged(nameof(TituloFormulario));
                OnPropertyChanged(nameof(EsEdicion));
                OnPropertyChanged(nameof(TieneFoto));
            }
        }

        public string TituloFormulario => EsNuevo ? "Nuevo Ejercicio" : "Editar Ejercicio";
        public bool EsEdicion => !EsNuevo;
        public bool TieneFoto => FotoPreview is { Length: > 0 };
        public bool SinFoto => FotoPreview is null || FotoPreview.Length == 0;

        public Func<byte[]?>? ElegirImagenAction { get; set; }

        public ICommand NuevoCommand { get; }
        public ICommand GuardarCommand { get; }
        public ICommand EliminarCommand { get; }
        public ICommand ElegirImagenCommand { get; }
        public ICommand QuitarImagenCommand { get; }

        public EjerciciosViewModel()
        {
            NuevoCommand = new RelayCommand(_ => NuevoEjercicio());
            GuardarCommand = new RelayCommand(_ => Guardar(), _ => !string.IsNullOrWhiteSpace(NombreInput));
            EliminarCommand = new RelayCommand(_ => Eliminar(), _ => !EsNuevo && EjercicioSeleccionado != null);
            ElegirImagenCommand = new RelayCommand(_ => ElegirImagen());
            QuitarImagenCommand = new RelayCommand(_ =>
            {
                FotoPreview = null;
                OnPropertyChanged(nameof(TieneFoto));
                OnPropertyChanged(nameof(SinFoto));
            });

            CargarEjercicios();
        }

        public void CargarEjercicios()
        {
            Ejercicios.Clear();
            foreach (var e in _repo.GetAll())
                Ejercicios.Add(e);
        }

        private void NuevoEjercicio()
        {
            _ejercicioSeleccionado = null;
            NombreInput = string.Empty;
            FotoPreview = null;
            EsNuevo = true;
            OnPropertyChanged(nameof(SinFoto));
            OnPropertyChanged(nameof(TieneFoto));
        }

        private void ElegirImagen()
        {
            var bytes = ElegirImagenAction?.Invoke();
            if (bytes != null)
            {
                FotoPreview = bytes;
                OnPropertyChanged(nameof(TieneFoto));
                OnPropertyChanged(nameof(SinFoto));
            }
        }

        private void Guardar()
        {
            if (string.IsNullOrWhiteSpace(NombreInput)) return;

            try
            {
                if (EsNuevo)
                    _repo.Crear(NombreInput.Trim(), FotoPreview);
                else
                {
                    if (EjercicioSeleccionado is null) return;
                    _repo.Actualizar(EjercicioSeleccionado.IdEjercicio, NombreInput.Trim(), FotoPreview);
                }
                CargarEjercicios();
                NuevoEjercicio();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Eliminar()
        {
            if (EjercicioSeleccionado is null) return;
            var result = MessageBox.Show(
                $"¿Eliminar '{EjercicioSeleccionado.Nombre}'?",
                "Confirmar eliminación", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes) return;

            try
            {
                _repo.Eliminar(EjercicioSeleccionado.IdEjercicio);
                CargarEjercicios();
                NuevoEjercicio();
            }
            catch
            {
                MessageBox.Show(
                    "No se puede eliminar: el ejercicio tiene registros de seguimiento asociados.",
                    "No se puede eliminar", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
