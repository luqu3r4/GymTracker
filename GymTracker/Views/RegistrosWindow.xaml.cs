using GymTracker.Models;
using GymTracker.ViewModels;
using System.Windows;

namespace GymTracker.Views
{
    public partial class RegistrosWindow : Window
    {
        public RegistrosWindow(Cliente cliente, Ejercicio ejercicio)
        {
            InitializeComponent();
            DataContext = new RegistrosViewModel(cliente, ejercicio);
        }

        private void BtnVolver_Click(object sender, RoutedEventArgs e) => Close();
    }
}
