using System.Windows;
using System.Windows.Input;

namespace GymTracker.Views
{
    public partial class NombreDialog : Window
    {
        public string Resultado { get; private set; } = string.Empty;

        public NombreDialog()
        {
            InitializeComponent();
            Loaded += (_, _) => TxtNombre.Focus();
        }

        private void BtnAceptar_Click(object sender, RoutedEventArgs e) => Aceptar();

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void TxtNombre_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) Aceptar();
            if (e.Key == Key.Escape) { DialogResult = false; Close(); }
        }

        private void Aceptar()
        {
            if (string.IsNullOrWhiteSpace(TxtNombre.Text))
            {
                TxtNombre.BorderBrush = System.Windows.Media.Brushes.Red;
                return;
            }
            Resultado = TxtNombre.Text.Trim();
            DialogResult = true;
            Close();
        }
    }
}
