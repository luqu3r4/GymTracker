using GymTracker.Data;
using GymTracker.Models;
using GymTracker.Reports;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace GymTracker.ViewModels
{
    public class RegistrosViewModel : ViewModelBase
    {
        private readonly SeguimientoRepository _repo = new();
        private readonly Cliente _cliente;
        private readonly Ejercicio _ejercicio;

        private Seguimiento? _registroSeleccionado;
        private string _pesoInput = string.Empty;
        private string _repeticionesInput = string.Empty;
        private DateTime _fechaInput = DateTime.Today;
        private string _mensajeError = string.Empty;

        public ObservableCollection<Seguimiento> Registros { get; } = new();

        public string Titulo => $"{_cliente.Nombre}  —  {_ejercicio.Nombre}";

        public Seguimiento? RegistroSeleccionado
        {
            get => _registroSeleccionado;
            set => SetProperty(ref _registroSeleccionado, value);
        }

        public string PesoInput
        {
            get => _pesoInput;
            set => SetProperty(ref _pesoInput, value);
        }

        public string RepeticionesInput
        {
            get => _repeticionesInput;
            set => SetProperty(ref _repeticionesInput, value);
        }

        public DateTime FechaInput
        {
            get => _fechaInput;
            set => SetProperty(ref _fechaInput, value);
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

        public ICommand AgregarCommand { get; }
        public ICommand EliminarCommand { get; }
        public ICommand GenerarInformeCommand { get; }
        public ICommand SalirCommand { get; }

        public RegistrosViewModel(Cliente cliente, Ejercicio ejercicio)
        {
            _cliente = cliente;
            _ejercicio = ejercicio;

            AgregarCommand = new RelayCommand(_ => AgregarRegistro());
            EliminarCommand = new RelayCommand(_ => EliminarRegistro(), _ => RegistroSeleccionado != null);
            GenerarInformeCommand = new RelayCommand(_ => GenerarInforme());
            SalirCommand = new RelayCommand(_ => System.Windows.Application.Current.Shutdown());

            CargarRegistros();
        }

        private void CargarRegistros()
        {
            Registros.Clear();
            foreach (var r in _repo.GetByClienteEjercicio(_cliente.IdCliente, _ejercicio.IdEjercicio))
                Registros.Add(r);
        }

        private void AgregarRegistro()
        {
            if (!decimal.TryParse(PesoInput.Replace(',', '.'),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out decimal peso) || peso <= 0)
            {
                MensajeError = "Introduce un peso válido (ej: 75 o 82.5).";
                return;
            }

            if (!int.TryParse(RepeticionesInput, out int reps) || reps <= 0)
            {
                MensajeError = "Introduce un número de repeticiones válido.";
                return;
            }

            try
            {
                _repo.Crear(_cliente.IdCliente, _ejercicio.IdEjercicio, peso, reps, FechaInput);
                PesoInput = string.Empty;
                RepeticionesInput = string.Empty;
                FechaInput = DateTime.Today;
                MensajeError = string.Empty;
                CargarRegistros();
            }
            catch (Exception ex)
            {
                MensajeError = $"Error al guardar: {ex.Message}";
            }
        }

        private void EliminarRegistro()
        {
            if (RegistroSeleccionado is null) return;

            var result = MessageBox.Show(
                "¿Eliminar este registro?", "Confirmar",
                MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                _repo.Eliminar(RegistroSeleccionado.IdSeguimiento);
                CargarRegistros();
            }
        }

        private void GenerarInforme()
        {
            try
            {
                var datos = _repo.GetByClienteEjercicio(_cliente.IdCliente, _ejercicio.IdEjercicio)
                    .OrderBy(r => r.Fecha)
                    .ToList();
                ReporteService.GenerarInforme(_cliente, _ejercicio, datos);
            }
            catch (Exception ex)
            {
                MensajeError = $"Error al generar el informe: {ex.Message}";
            }
        }
    }
}
