using Biblioteca.Entidades;
using Biblioteca.Negocio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace BibliotecaSolucion
{
    public partial class RegistrarPrestamo : Window
    {
        private PrestamoNegocio prestamoNegocio;
        private SocioNegocio socioNegocio;
        private LibroNegocio libroNegocio;
        
        private List<Libro> librosSeleccionados = new List<Libro>();

        public RegistrarPrestamo()
        {
            InitializeComponent();
            prestamoNegocio = new PrestamoNegocio();
            socioNegocio = new SocioNegocio();
            libroNegocio = new LibroNegocio();
            
            this.Loaded += async (s, e) => await CargarCombos();
        }

        private async Task CargarCombos()
        {
            try
            {
                var socios = await socioNegocio.ListarActivos();
                CmbSocio.ItemsSource = socios;
                CmbSocio.DisplayMemberPath = "Nombre";
                CmbSocio.SelectedValuePath = "SocioId";

                var libros = await libroNegocio.ListarActivos();
                // Filtrar libros que tengan ejemplares > 0
                CmbLibro.ItemsSource = libros.Where(l => l.Ejemplares > 0).ToList();
                CmbLibro.DisplayMemberPath = "Titulo";
                CmbLibro.SelectedValuePath = "LibroId";
                
                DpFechaLimite.SelectedDate = DateTime.Now.AddDays(14); // 2 semanas por defecto
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al conectar con la Base de Datos: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnAgregar_Click(object sender, RoutedEventArgs e)
        {
            if (CmbLibro.SelectedItem is Libro libro)
            {
                if (librosSeleccionados.Any(l => l.LibroId == libro.LibroId))
                {
                    MessageBox.Show("El libro ya está en la lista.");
                    return;
                }
                
                librosSeleccionados.Add(libro);
                ActualizarGrilla();
            }
            else
            {
                MessageBox.Show("Seleccione un libro.");
            }
        }

        private void BtnQuitar_Click(object sender, RoutedEventArgs e)
        {
            if (DgLibrosSeleccionados.SelectedItem is Libro libro)
            {
                librosSeleccionados.Remove(libro);
                ActualizarGrilla();
            }
        }

        private void ActualizarGrilla()
        {
            DgLibrosSeleccionados.ItemsSource = null;
            DgLibrosSeleccionados.ItemsSource = librosSeleccionados;
        }

        private async void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (CmbSocio.SelectedValue == null)
            {
                MessageBox.Show("Seleccione un socio.");
                return;
            }

            if (librosSeleccionados.Count == 0)
            {
                MessageBox.Show("Agregue al menos un libro.");
                return;
            }

            if (DpFechaLimite.SelectedDate == null)
            {
                MessageBox.Show("Seleccione una fecha límite.");
                return;
            }

            try
            {
                int socioId = (int)CmbSocio.SelectedValue;
                DateTime fechaLimite = DpFechaLimite.SelectedDate.Value;
                List<int> libroIds = librosSeleccionados.Select(l => l.LibroId).ToList();

                await prestamoNegocio.RegistrarPrestamo(socioId, libroIds, fechaLimite);
                MessageBox.Show("Préstamo registrado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                
                // Limpiar
                CmbSocio.SelectedIndex = -1;
                CmbLibro.SelectedIndex = -1;
                librosSeleccionados.Clear();
                ActualizarGrilla();
                await CargarCombos();
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
    }
}
