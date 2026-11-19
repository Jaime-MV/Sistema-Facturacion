using Sistema_Facturacion.CapaDatos;
using Sistema_Facturacion.Modelos;
using Sistema_Facturacion.Seguridad;

namespace Sistema_Facturacion.Formularios;

public sealed class FrmClientes : FrmCrud<Cliente>
{
    private readonly ClienteRepositorio _repositorio;
    private readonly TextBox _nombre;
    private readonly TextBox _telefono;
    private readonly TextBox _email;
    private readonly TextBox _direccion;

    public FrmClientes(Conexion conexion) : base("Gestión de clientes")
    {
        _repositorio = new ClienteRepositorio(conexion);
        _nombre = CrearTexto(150);
        _telefono = CrearTexto(30);
        _email = CrearTexto(254);
        _direccion = CrearTexto(300);
        AgregarCampo("Nombre *", _nombre);
        AgregarCampo("Teléfono", _telefono);
        AgregarCampo("Correo electrónico", _email);
        AgregarCampo("Dirección", _direccion);
        AgregarCampo("Campos", new Label { Text = "* Obligatorio. El correo es opcional; si lo ingresas debe tener un formato válido.", AutoSize = true });
        Columna(nameof(Cliente.IdCliente), "ID", 25);
        Columna(nameof(Cliente.Nombre), "Nombre", 100);
        Columna(nameof(Cliente.Telefono), "Teléfono", 65);
        Columna(nameof(Cliente.Email), "Correo", 100);
        Columna(nameof(Cliente.Direccion), "Dirección", 130);
    }

    protected override int ObtenerId(Cliente registro) => registro.IdCliente;
    protected override string Describir(Cliente registro) => registro.Nombre;
    protected override bool Coincide(Cliente registro, string filtro)
        => Contiene($"{registro.IdCliente} {registro.Nombre} {registro.Telefono} {registro.Email} {registro.Direccion}", filtro);
    protected override Task<List<Cliente>> ListarAsync(CancellationToken ct) => _repositorio.ListarAsync(ct);
    protected override Task GuardarRegistroAsync(int? id, CancellationToken ct)
        => _repositorio.GuardarAsync(id, _nombre.Text, _telefono.Text, _email.Text, _direccion.Text, ct);
    protected override Task EliminarRegistroAsync(int id, CancellationToken ct) => _repositorio.EliminarAsync(id, ct);

    protected override bool ValidarCampos()
    {
        bool nombre = Validar(_nombre, !string.IsNullOrWhiteSpace(_nombre.Text), "Ingresa el nombre del cliente.");
        bool email = Validar(_email, Validaciones.EmailOpcionalValido(_email.Text.Trim()), "Ingresa un correo válido, por ejemplo cliente@ejemplo.com.");
        return nombre && email;
    }

    protected override void MostrarRegistro(Cliente registro)
    {
        _nombre.Text = registro.Nombre;
        _telefono.Text = registro.Telefono;
        _email.Text = registro.Email;
        _direccion.Text = registro.Direccion;
    }

    protected override void LimpiarCampos()
    {
        _nombre.Clear(); _telefono.Clear(); _email.Clear(); _direccion.Clear();
    }
}
