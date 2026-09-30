using Biblioteca.Negocio;
using System;
using System.Windows;
using System.Windows.Input;

namespace BibliotecaSolucion
{
    public partial class ReportePrestamos : Window
    {
        private PrestamoNegocio prestamoNegocio;

        public ReportePrestamos()
        {
            InitializeComponent();
            prestamoNegocio = new PrestamoNegocio();

            // Establecer fechas por defecto (últimos 30 días)
            DpFechaFin.SelectedDate = DateTime.Now;
            DpFechaInicio.SelectedDate = DateTime.Now.AddDays(-30);
        }

        private async void BtnGenerar_Click(object sender, RoutedEventArgs e)
        {
            if (!DpFechaInicio.SelectedDate.HasValue || !DpFechaFin.SelectedDate.HasValue)
            {
                MessageBox.Show("Por favor, seleccione ambas fechas.", "Advertencia",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Mostrar cursor de espera
                this.Cursor = Cursors.Wait;

                DateTime inicio = DpFechaInicio.SelectedDate.Value.Date;
                DateTime fin = DpFechaFin.SelectedDate.Value.Date;

                if (inicio > fin)
                {
                    MessageBox.Show("La fecha de inicio no puede ser mayor a la fecha fin.", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var reporte = await prestamoNegocio.ListarReportePorFechas(inicio, fin);
                DgReporte.ItemsSource = reporte;

                if (reporte.Count == 0)
                {
                    MessageBox.Show("No se encontraron préstamos en el intervalo seleccionado.", "Información",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al generar el reporte: " + ex.Message, "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                // Restaurar cursor
                this.Cursor = Cursors.Arrow;
            }
        }
    }
}