using System.Globalization;
using Sistema_Facturacion.CapaDatos;
using Sistema_Facturacion.Modelos;
using Sistema_Facturacion.Seguridad;

namespace Sistema_Facturacion.Formularios;

public sealed class FrmProductos : FrmCrud<Producto>
{
    private readonly ProductoRepositorio _repositorio;
    private readonly TextBox _descripcion;
    private readonly TextBox _cantidad;
    private readonly TextBox _precio;

    public FrmProductos(Conexion conexion) : base("Gestión de productos")
    {
        _repositorio = new ProductoRepositorio(conexion);
        _descripcion = CrearTexto(200);
        _cantidad = CrearTexto(10);
        _precio = CrearTexto(11);
        _cantidad.KeyPress += SoloNumeros;
        _precio.KeyPress += SoloPrecio;
        AgregarCampo("Descripción *", _descripcion);
        AgregarCampo("Cantidad disponible *", _cantidad);
        AgregarCampo("Precio unitario *", _precio);
        AgregarCampo("Formato", new Label { Text = "Usa punto decimal, por ejemplo 12.50. Stock y precio pueden ser cero.", AutoSize = true });
        Columna(nameof(Producto.IdProducto), "ID", 30);
        Columna(nameof(Producto.Descripcion), "Descripción", 170);
        Columna(nameof(Producto.Cantidad), "Stock", 50);
        Columna(nameof(Producto.Precio), "Precio", 55, "N2");
    }

    protected override int ObtenerId(Producto registro) => registro.IdProducto;
    protected override string Describir(Producto registro) => registro.Descripcion;
    protected override bool Coincide(Producto registro, string filtro)
        => Contiene($"{registro.IdProducto} {registro.Descripcion}", filtro);
    protected override Task<List<Producto>> ListarAsync(CancellationToken ct) => _repositorio.ListarAsync(ct);
    protected override Task EliminarRegistroAsync(int id, CancellationToken ct) => _repositorio.EliminarAsync(id, ct);

    protected override Task GuardarRegistroAsync(int? id, CancellationToken ct)
    {
        Validaciones.CantidadValida(_cantidad.Text, out int cantidad);
        Validaciones.PrecioValido(_precio.Text, out decimal precio);
        return _repositorio.GuardarAsync(id, _descripcion.Text, cantidad, precio, ct);
    }

    protected override bool ValidarCampos()
    {
        bool descripcion = Validar(_descripcion, !string.IsNullOrWhiteSpace(_descripcion.Text), "Ingresa la descripción.");
        bool cantidad = Validar(_cantidad, Validaciones.CantidadValida(_cantidad.Text, out _), "Ingresa un entero entre 0 y 2147483647.");
        bool precio = Validar(_precio, Validaciones.PrecioValido(_precio.Text, out _), "Ingresa un precio entre 0 y 99999999.99, con hasta dos decimales y punto decimal.");
        return descripcion && cantidad && precio;
    }

    protected override void MostrarRegistro(Producto registro)
    {
        _descripcion.Text = registro.Descripcion;
        _cantidad.Text = registro.Cantidad.ToString(CultureInfo.InvariantCulture);
        _precio.Text = registro.Precio.ToString("0.00", CultureInfo.InvariantCulture);
    }

    protected override void LimpiarCampos()
    {
        _descripcion.Clear();
        _cantidad.Text = "0";
        _precio.Text = "0.00";
    }
}
