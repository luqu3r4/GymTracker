using GymTracker.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace GymTracker.Views
{
    public partial class ClientesWindow : Window
    {
        public ClientesWindow()
        {
            InitializeComponent();
            var vm = new ClientesViewModel();

            vm.CerrarSesionAction = () =>
            {
                Session.EntrenadorActivo = null;
                new LoginWindow().Show();
                Close();
            };

            vm.PedirNombreAction = () =>
            {
                var dialog = new NombreDialog { Owner = this };
                return dialog.ShowDialog() == true ? dialog.Resultado : null;
            };

            vm.AbrirRegistrosAction = (cliente, ejercicio) =>
            {
                new RegistrosWindow(cliente, ejercicio) { Owner = this }.Show();
            };

            vm.AbrirRutinasAction = () =>
            {
                new RutinasWindow { Owner = this }.ShowDialog();
                vm.CargarDatos();
            };

            vm.AbrirEjerciciosAction = () =>
            {
                new EjerciciosWindow { Owner = this }.ShowDialog();
                vm.CargarDatos();
            };

            DataContext = vm;
        }

        private void DataGridEjercicios_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is not ClientesViewModel vm) return;

            if (vm.ClienteSeleccionado is null)
            {
                MessageBox.Show("Selecciona un cliente primero.", "GymTracker",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (vm.EjercicioSeleccionado is null) return;

            vm.AbrirRegistros(vm.ClienteSeleccionado, vm.EjercicioSeleccionado);
        }

    }
}
