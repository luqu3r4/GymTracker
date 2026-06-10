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
        private readonly EjercicioRepository _ejercicioRepo = new();

        private Cliente? _clienteSeleccionado;
        private Ejercicio? _ejercicioSeleccionado;
        private string _fechaActual = string.Empty;

        public ObservableCollection<Cliente> Clientes { get; } = new();
        public ObservableCollection<Ejercicio> Ejercicios { get; } = new();

        public Cliente? ClienteSeleccionado
        {
            get => _clienteSeleccionado;
            set => SetProperty(ref _clienteSeleccionado, value);
        }

        public Ejercicio? EjercicioSeleccionado
        {
            get => _ejercicioSeleccionado;
            set => SetProperty(ref _ejercicioSeleccionado, value);
        }

        public string NombreEntrenador => Session.EntrenadorActivo?.Nombre ?? string.Empty;

        public string FechaActual
        {
            get => _fechaActual;
            set => SetProperty(ref _fechaActual, value);
        }

        public ICommand AgregarClienteCommand { get; }
        public ICommand EliminarClienteCommand { get; }
        public ICommand CerrarSesionCommand { get; }
        public ICommand SalirCommand { get; }
        public ICommand AcercaDeCommand { get; }

        public Action? CerrarSesionAction { get; set; }
        public Func<string?>? PedirNombreAction { get; set; }
        public Action<Cliente, Ejercicio>? AbrirRegistrosAction { get; set; }

        public ClientesViewModel()
        {
            AgregarClienteCommand = new RelayCommand(_ => AgregarCliente());
            EliminarClienteCommand = new RelayCommand(_ => EliminarCliente(), _ => ClienteSeleccionado != null);
            CerrarSesionCommand = new RelayCommand(_ => CerrarSesionAction?.Invoke());
            SalirCommand = new RelayCommand(_ => Application.Current.Shutdown());
            AcercaDeCommand = new RelayCommand(_ => MessageBox.Show(
                "GymTracker v1.0\nAplicación de seguimiento de entrenamiento.",
                "Acerca de", MessageBoxButton.OK, MessageBoxImage.Information));

            CargarDatos();
            IniciarReloj();
        }

        public void CargarDatos()
        {
            if (Session.EntrenadorActivo is null) return;

            Clientes.Clear();
            foreach (var c in _clienteRepo.GetByEntrenador(Session.EntrenadorActivo.IdEntrenador))
                Clientes.Add(c);

            Ejercicios.Clear();
            foreach (var e in _ejercicioRepo.GetAll())
                Ejercicios.Add(e);
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

        public void AbrirRegistros(Cliente cliente, Ejercicio ejercicio)
            => AbrirRegistrosAction?.Invoke(cliente, ejercicio);

        private void IniciarReloj()
        {
            FechaActual = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(30) };
            timer.Tick += (_, _) => FechaActual = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
            timer.Start();
        }
    }
}
