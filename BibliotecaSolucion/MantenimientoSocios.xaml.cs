using Biblioteca.Entidades;
using Biblioteca.Negocio;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace BibliotecaSolucion
{
    public partial class MantenimientoSocios : Window
    {
        private SocioNegocio socioNegocio;
        private List<Socio> listaSocios = new List<Socio>();
        private int socioIdSeleccionado = 0;

        public MantenimientoSocios()
        {
            InitializeComponent();
            socioNegocio = new SocioNegocio();
            this.Loaded += async (s, e) => await CargarDatos();
        }

        private async Task CargarDatos()
        {
            try
            {
                listaSocios = await socioNegocio.ListarActivos();
                DgSocios.ItemsSource = listaSocios;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al conectar con la Base de Datos: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnBuscar_Click(object sender, RoutedEventArgs e)
        {
            string texto = TxtBuscar.Text.ToLower();
            var filtrados = listaSocios.FindAll(s =>
                s.Nombre.ToLower().Contains(texto) ||
                s.DNI.ToLower().Contains(texto));
            DgSocios.ItemsSource = filtrados;
        }

        private void DgSocios_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DgSocios.SelectedItem is Socio socio)
            {
                socioIdSeleccionado = socio.SocioId;
                TxtId.Text = socio.SocioId.ToString();
                TxtDNI.Text = socio.DNI;
                TxtNombre.Text = socio.Nombre;
                TxtEmail.Text = socio.Email;
            }
        }

        private void BtnNuevo_Click(object sender, RoutedEventArgs e)
        {
            socioIdSeleccionado = 0;
            TxtId.Text = "";
            TxtDNI.Text = "";
            TxtNombre.Text = "";
            TxtEmail.Text = "";
        }

        private async void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Socio socio = new Socio
                {
                    SocioId = socioIdSeleccionado,
                    DNI = TxtDNI.Text,
                    Nombre = TxtNombre.Text,
                    Email = TxtEmail.Text,
                    Activo = true
                };

                if (socioIdSeleccionado == 0)
                {
                    await socioNegocio.Insertar(socio);
                    MessageBox.Show("Socio insertado correctamente.");
                }
                else
                {
                    await socioNegocio.Actualizar(socio);
                    MessageBox.Show("Socio actualizado correctamente.");
                }

                await CargarDatos();
                BtnNuevo_Click(sender, e);
            }
            catch (ReglaNegocioException ex)
            {
                MessageBox.Show(ex.Message, "Error de Regla de Negocio", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void BtnEliminar_Click(object sender, RoutedEventArgs e)
        {
            if (socioIdSeleccionado == 0)
            {
                MessageBox.Show("Seleccione un socio primero.");
                return;
            }

            if (MessageBox.Show("¿Está seguro de eliminar este socio?", "Confirmar",
                MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    await socioNegocio.EliminarLogico(socioIdSeleccionado);
                    MessageBox.Show("Socio eliminado correctamente.");
                    await CargarDatos();
                    BtnNuevo_Click(sender, e);
                }
                catch (ReglaNegocioException ex)
                {
                    MessageBox.Show(ex.Message, "Error de Regla de Negocio", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }
    }
}
