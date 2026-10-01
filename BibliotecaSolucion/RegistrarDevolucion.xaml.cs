using Biblioteca.Entidades;
using Biblioteca.Negocio;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace BibliotecaSolucion
{
    public partial class RegistrarDevolucion : Window
    {
        private PrestamoNegocio prestamoNegocio;
        private int prestamoIdActual = 0;

        public RegistrarDevolucion()
        {
            InitializeComponent();
            prestamoNegocio = new PrestamoNegocio();
        }

        private async void BtnBuscar_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(TxtPrestamoId.Text, out int prestamoId))
            {
                prestamoIdActual = prestamoId;
                await CargarDetalles();
                LblMulta.Content = "Multa: S/ 0.00";
            }
            else
            {
                MessageBox.Show("Ingrese un ID de préstamo válido.");
            }
        }

        private async Task CargarDetalles()
        {
            try
            {
                var detalles = await prestamoNegocio.ListarDetallesPorPrestamo(prestamoIdActual);
                DgLibros.ItemsSource = detalles;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void BtnDevolver_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int libroId)
            {
                try
                {
                    decimal multa = await prestamoNegocio.RegistrarDevolucion(prestamoIdActual, libroId);
                    
                    if (multa > 0)
                    {
                        LblMulta.Content = $"Multa: S/ {multa:0.00}";
                        MessageBox.Show($"Devolución registrada con retraso. Multa a pagar: S/ {multa:0.00}", "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    else
                    {
                        LblMulta.Content = "Multa: S/ 0.00";
                        MessageBox.Show("Devolución registrada a tiempo.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    }

                    await CargarDetalles();
                }
                catch (ReglaNegocioException ex)
                {
                    MessageBox.Show(ex.Message, "Regla de Negocio", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
