using Biblioteca.Entidades;
using Biblioteca.Negocio;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace BibliotecaSolucion
{
    public partial class MantenimientoLibros : Window
    {
        private LibroNegocio libroNegocio;
        private List<Libro> listaLibros = new List<Libro>(); // ✅ CORREGIDO
        private int libroIdSeleccionado = 0;

        public MantenimientoLibros()
        {
            InitializeComponent();
            libroNegocio = new LibroNegocio();
            this.Loaded += async (s, e) => await CargarDatos(); // ✅ CORREGIDO
        }

        private async Task CargarDatos()
        {
            listaLibros = await libroNegocio.ListarActivos();
            DgLibros.ItemsSource = listaLibros;
            await CargarAutores();
        }

        private async Task CargarAutores()
        {
            List<Autor> autores = await libroNegocio.ListarAutoresActivos();
            CmbAutor.ItemsSource = autores;
            CmbAutor.DisplayMemberPath = "Nombre";
            CmbAutor.SelectedValuePath = "AutorId";
        }

        private async void BtnBuscar_Click(object sender, RoutedEventArgs e)
        {
            string texto = TxtBuscar.Text.ToLower();
            var filtrados = listaLibros.FindAll(l =>
                l.Titulo.ToLower().Contains(texto) ||
                l.NombreAutor.ToLower().Contains(texto));
            DgLibros.ItemsSource = filtrados;
        }

        private void DgLibros_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DgLibros.SelectedItem is Libro libro)
            {
                libroIdSeleccionado = libro.LibroId;
                TxtId.Text = libro.LibroId.ToString();
                TxtTitulo.Text = libro.Titulo;
                TxtISBN.Text = libro.ISBN;
                CmbAutor.SelectedValue = libro.AutorId;
                TxtEjemplares.Text = libro.Ejemplares.ToString();
            }
        }

        private void BtnNuevo_Click(object sender, RoutedEventArgs e)
        {
            libroIdSeleccionado = 0;
            TxtId.Text = "";
            TxtTitulo.Text = "";
            TxtISBN.Text = "";
            TxtEjemplares.Text = "";
            CmbAutor.SelectedIndex = -1;
        }

        private async void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Libro libro = new Libro
                {
                    LibroId = libroIdSeleccionado,
                    Titulo = TxtTitulo.Text,
                    ISBN = TxtISBN.Text,
                    AutorId = (int)CmbAutor.SelectedValue!,
                    Ejemplares = int.Parse(TxtEjemplares.Text),
                    Activo = true
                };

                if (libroIdSeleccionado == 0)
                {
                    await libroNegocio.Insertar(libro);
                    MessageBox.Show("Libro insertado correctamente.");
                }
                else
                {
                    await libroNegocio.Actualizar(libro);
                    MessageBox.Show("Libro actualizado correctamente.");
                }

                await CargarDatos();
                BtnNuevo_Click(sender, e); // ✅ CORREGIDO
            }
            catch (ReglaNegocioException ex)
            {
                MessageBox.Show(ex.Message, "Error de Regla de Negocio", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void BtnEliminar_Click(object sender, RoutedEventArgs e)
        {
            if (libroIdSeleccionado == 0)
            {
                MessageBox.Show("Seleccione un libro primero.");
                return;
            }

            if (MessageBox.Show("¿Está seguro de eliminar este libro?", "Confirmar",
                MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    await libroNegocio.EliminarLogico(libroIdSeleccionado);
                    MessageBox.Show("Libro eliminado correctamente.");
                    await CargarDatos();
                    BtnNuevo_Click(sender, e); // ✅ CORREGIDO
                }
                catch (ReglaNegocioException ex)
                {
                    MessageBox.Show(ex.Message, "Error de Regla de Negocio", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }
    }
}