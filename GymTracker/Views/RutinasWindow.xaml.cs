using GymTracker.ViewModels;
using System.Windows;

namespace GymTracker.Views
{
    public partial class RutinasWindow : Window
    {
        public RutinasWindow()
        {
            InitializeComponent();
            var vm = new RutinasViewModel();
            vm.PedirNombreAction = () =>
            {
                var dialog = new NombreDialog { Owner = this };
                return dialog.ShowDialog() == true ? dialog.Resultado : null;
            };
            DataContext = vm;
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e) => Close();
    }
}
