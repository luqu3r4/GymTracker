using GymTracker.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace GymTracker.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            var vm = new LoginViewModel();
            vm.LoginExitoso = () =>
            {
                new ClientesWindow().Show();
                Close();
            };
            DataContext = vm;
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is LoginViewModel vm)
                vm.ExecuteLogin(PinBox.Password);
        }

        private void PinBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && DataContext is LoginViewModel vm)
                vm.ExecuteLogin(PinBox.Password);
        }
    }
}
