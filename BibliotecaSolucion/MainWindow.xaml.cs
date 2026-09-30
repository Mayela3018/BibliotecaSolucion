using System.Windows;

namespace BibliotecaSolucion
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnLibros_Click(object sender, RoutedEventArgs e)
        {
            MantenimientoLibros ventana = new MantenimientoLibros();
            ventana.ShowDialog();
        }

        private void BtnSocios_Click(object sender, RoutedEventArgs e)
        {
            MantenimientoSocios ventana = new MantenimientoSocios();
            ventana.ShowDialog();
        }

        private void BtnPrestamo_Click(object sender, RoutedEventArgs e)
        {
            RegistrarPrestamo ventana = new RegistrarPrestamo();
            ventana.ShowDialog();
        }

        private void BtnDevolucion_Click(object sender, RoutedEventArgs e)
        {
            RegistrarDevolucion ventana = new RegistrarDevolucion();
            ventana.ShowDialog();
        }

        private void BtnReporte_Click(object sender, RoutedEventArgs e)
        {
            ReportePrestamos ventana = new ReportePrestamos();
            ventana.ShowDialog();
        }
    }
}