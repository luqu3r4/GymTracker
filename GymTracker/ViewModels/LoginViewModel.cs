using GymTracker.Data;
using GymTracker.Models;
using System.Collections.ObjectModel;

namespace GymTracker.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private readonly EntrenadorRepository _repo = new();

        private Entrenador? _entrenadorSeleccionado;
        private string _mensajeError = string.Empty;

        public ObservableCollection<Entrenador> Entrenadores { get; } = new();

        public Entrenador? EntrenadorSeleccionado
        {
            get => _entrenadorSeleccionado;
            set => SetProperty(ref _entrenadorSeleccionado, value);
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

        public Action? LoginExitoso { get; set; }

        public LoginViewModel()
        {
            CargarEntrenadores();
        }

        private void CargarEntrenadores()
        {
            try
            {
                foreach (var e in _repo.GetAll())
                    Entrenadores.Add(e);
            }
            catch
            {
                MensajeError = "No se pudo conectar con la base de datos.";
            }
        }

        public void ExecuteLogin(string pin)
        {
            if (EntrenadorSeleccionado is null)
            {
                MensajeError = "Selecciona un entrenador.";
                return;
            }

            if (string.IsNullOrWhiteSpace(pin))
            {
                MensajeError = "Introduce el PIN.";
                return;
            }

            try
            {
                var entrenador = _repo.VerificarLogin(EntrenadorSeleccionado.IdEntrenador, pin);
                if (entrenador is null)
                {
                    MensajeError = "PIN incorrecto.";
                    return;
                }

                Session.EntrenadorActivo = entrenador;
                MensajeError = string.Empty;
                LoginExitoso?.Invoke();
            }
            catch
            {
                MensajeError = "Error al conectar con la base de datos.";
            }
        }
    }
}
