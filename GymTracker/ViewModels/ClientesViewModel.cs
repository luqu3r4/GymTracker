using GymTracker.Data;
using GymTracker.Models;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace GymTracker.ViewModels
{
    public class ClientesViewModel : ViewModelBase
    {
        private readonly ClienteRepository _clienteRepo = new();
        private readonly RutinaRepository _rutinaRepo = new();

        private Cliente? _clienteSeleccionado;
        private Rutina? _rutinaSeleccionadaCliente;
        private RutinaEjercicio? _ejercicioSeleccionado;
        private string _fechaActual = string.Empty;

        public ObservableCollection<Cliente> Clientes { get; } = new();
        public ObservableCollection<Rutina> Rutinas { get; } = new();
        public ObservableCollection<RutinaEjercicio> EjerciciosRutina { get; } = new();

        public Cliente? ClienteSeleccionado
        {
            get => _clienteSeleccionado;
            set
            {
                SetProperty(ref _clienteSeleccionado, value);
                ActualizarRutinaCliente();
            }
        }

        public Rutina? RutinaSeleccionadaCliente
        {
            get => _rutinaSeleccionadaCliente;
            set
            {
                SetProperty(ref _rutinaSeleccionadaCliente, value);
                OnPropertyChanged(nameof(SinRutina));
                CargarEjerciciosRutina();
            }
        }

        public RutinaEjercicio? EjercicioSeleccionado
        {
            get => _ejercicioSeleccionado;
            set => SetProperty(ref _ejercicioSeleccionado, value);
        }

        public bool SinRutina => RutinaSeleccionadaCliente is null;

        public string NombreEntrenador => Session.EntrenadorActivo?.Nombre ?? string.Empty;

        public string FechaActual
        {
            get => _fechaActual;
            set => SetProperty(ref _fechaActual, value);
        }

        public ICommand AgregarClienteCommand { get; }
        public ICommand EliminarClienteCommand { get; }
        public ICommand AsignarRutinaCommand { get; }
        public ICommand QuitarRutinaCommand { get; }
        public ICommand CerrarSesionCommand { get; }
        public ICommand SalirCommand { get; }
        public ICommand AcercaDeCommand { get; }
        public ICommand GestionarRutinasCommand { get; }
        public ICommand GestionarEjerciciosCommand { get; }

        public Action? CerrarSesionAction { get; set; }
        public Func<string?>? PedirNombreAction { get; set; }
        public Action<Cliente, Ejercicio>? AbrirRegistrosAction { get; set; }
        public Action? AbrirRutinasAction { get; set; }
        public Action? AbrirEjerciciosAction { get; set; }

        public ClientesViewModel()
        {
            AgregarClienteCommand = new RelayCommand(_ => AgregarCliente());
            EliminarClienteCommand = new RelayCommand(_ => EliminarCliente(), _ => ClienteSeleccionado != null);
            AsignarRutinaCommand = new RelayCommand(_ => AsignarRutina(),
                _ => ClienteSeleccionado != null && RutinaSeleccionadaCliente != null);
            QuitarRutinaCommand = new RelayCommand(_ => QuitarRutina(),
                _ => ClienteSeleccionado?.IdRutinaActual != null);
            CerrarSesionCommand = new RelayCommand(_ => CerrarSesionAction?.Invoke());
            SalirCommand = new RelayCommand(_ => Application.Current.Shutdown());
            GestionarRutinasCommand = new RelayCommand(_ => AbrirRutinasAction?.Invoke());
            GestionarEjerciciosCommand = new RelayCommand(_ => AbrirEjerciciosAction?.Invoke());
            AcercaDeCommand = new RelayCommand(_ => MessageBox.Show(
                "GymTracker v1.0\nAplicación de seguimiento de entrenamiento.",
                "Acerca de", MessageBoxButton.OK, MessageBoxImage.Information));

            CargarDatos();
            IniciarReloj();
        }

        public void CargarDatos()
        {
            if (Session.EntrenadorActivo is null) return;

            var clienteAnteriorId = ClienteSeleccionado?.IdCliente;

            Clientes.Clear();
            foreach (var c in _clienteRepo.GetByEntrenador(Session.EntrenadorActivo.IdEntrenador))
                Clientes.Add(c);

            Rutinas.Clear();
            foreach (var r in _rutinaRepo.GetByEntrenador(Session.EntrenadorActivo.IdEntrenador))
                Rutinas.Add(r);

            if (clienteAnteriorId.HasValue)
                ClienteSeleccionado = Clientes.FirstOrDefault(c => c.IdCliente == clienteAnteriorId.Value);
        }

        private void ActualizarRutinaCliente()
        {
            if (ClienteSeleccionado?.IdRutinaActual is int idRutina)
                RutinaSeleccionadaCliente = Rutinas.FirstOrDefault(r => r.IdRutina == idRutina);
            else
                RutinaSeleccionadaCliente = null;
        }

        private void CargarEjerciciosRutina()
        {
            EjerciciosRutina.Clear();
            if (RutinaSeleccionadaCliente is null) return;
            foreach (var e in _rutinaRepo.GetEjercicios(RutinaSeleccionadaCliente.IdRutina))
                EjerciciosRutina.Add(e);
        }

        private void AgregarCliente()
        {
            var nombre = PedirNombreAction?.Invoke();
            if (string.IsNullOrWhiteSpace(nombre)) return;
            _clienteRepo.Crear(nombre.Trim(), Session.EntrenadorActivo!.IdEntrenador);
            CargarDatos();
        }

        private void EliminarCliente()
        {
            if (ClienteSeleccionado is null) return;
            var result = MessageBox.Show(
                $"¿Eliminar a '{ClienteSeleccionado.Nombre}'? Se borrarán todos sus registros.",
                "Confirmar eliminación", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                _clienteRepo.Eliminar(ClienteSeleccionado.IdCliente);
                CargarDatos();
            }
        }

        private void AsignarRutina()
        {
            if (ClienteSeleccionado is null || RutinaSeleccionadaCliente is null) return;
            _rutinaRepo.AsignarRutinaCliente(ClienteSeleccionado.IdCliente, RutinaSeleccionadaCliente.IdRutina);
            ClienteSeleccionado.IdRutinaActual = RutinaSeleccionadaCliente.IdRutina;
            CargarDatos();
        }

        private void QuitarRutina()
        {
            if (ClienteSeleccionado is null) return;
            _rutinaRepo.QuitarRutinaCliente(ClienteSeleccionado.IdCliente);
            ClienteSeleccionado.IdRutinaActual = null;
            CargarDatos();
        }

        public void AbrirRegistros(Cliente cliente, RutinaEjercicio re)
        {
            var ejercicio = new Ejercicio { IdEjercicio = re.IdEjercicio, Nombre = re.NombreEjercicio, Foto = re.Foto };
            AbrirRegistrosAction?.Invoke(cliente, ejercicio);
        }

        private void IniciarReloj()
        {
            FechaActual = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(30) };
            timer.Tick += (_, _) => FechaActual = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
            timer.Start();
        }
    }
}
