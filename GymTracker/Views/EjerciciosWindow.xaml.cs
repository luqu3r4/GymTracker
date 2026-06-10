using GymTracker.ViewModels;
using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace GymTracker.Views
{
    public partial class EjerciciosWindow : Window
    {
        public EjerciciosWindow()
        {
            InitializeComponent();
            var vm = new EjerciciosViewModel();

            vm.ElegirImagenAction = () =>
            {
                var dlg = new OpenFileDialog
                {
                    Filter = "Imágenes|*.jpg;*.jpeg;*.png;*.bmp;*.gif",
                    Title = "Seleccionar imagen del ejercicio"
                };
                if (dlg.ShowDialog() != true) return null;
                return ResizarYComprimir(dlg.FileName);
            };

            DataContext = vm;
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e) => Close();

        private static byte[] ResizarYComprimir(string filePath)
        {
            const int maxDim = 400;
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.UriSource = new Uri(filePath, UriKind.Absolute);
            bmp.DecodePixelWidth = maxDim;
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.EndInit();
            bmp.Freeze();

            var encoder = new JpegBitmapEncoder { QualityLevel = 80 };
            encoder.Frames.Add(BitmapFrame.Create(bmp));
            using var ms = new MemoryStream();
            encoder.Save(ms);
            return ms.ToArray();
        }
    }
}
