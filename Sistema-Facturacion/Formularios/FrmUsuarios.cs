using Sistema_Facturacion.CapaDatos;
using Sistema_Facturacion.Modelos;

namespace Sistema_Facturacion.Formularios;

public sealed class FrmUsuarios : FrmCrud<Usuario>
{
    private readonly UsuarioRepositorio _repositorio;
    private readonly int _idActual;
    private readonly TextBox _nombre;
    private readonly TextBox _completo;
    private readonly TextBox _password;
    private readonly TextBox _confirmacion;
    public event Action<Usuario>? UsuarioActualizado;

    public FrmUsuarios(Conexion conexion, int idUsuarioActual) : base("Gestión de usuarios")
    {
        _repositorio = new UsuarioRepositorio(conexion);
        _idActual = idUsuarioActual;
        _nombre = CrearTexto(50);
        _completo = CrearTexto(150);
        _password = CrearTexto(256, password: true);
        _confirmacion = CrearTexto(256, password: true);
        AgregarCampo("Usuario *", _nombre);
        AgregarCampo("Nombre completo *", _completo);
        AgregarCampo("Contraseña", _password);
        AgregarCampo("Confirmar contraseña", _confirmacion);
        AgregarCampo("Contraseña", new Label
        {
            Text = "Mínimo 12 caracteres. Al crear es obligatoria; al editar deja ambos campos vacíos para conservarla.",
            AutoSize = true, MaximumSize = new Size(650, 0)
        });
        Columna(nameof(Usuario.IdUsuario), "ID", 30);
        Columna(nameof(Usuario.Nombre), "Usuario", 80);
        Columna(nameof(Usuario.NombreCompleto), "Nombre completo", 150);
        Columna(nameof(Usuario.FechaRegistro), "Fecha de registro", 90, "dd/MM/yyyy HH:mm");
    }

    protected override int ObtenerId(Usuario registro) => registro.IdUsuario;
    protected override string Describir(Usuario registro) => registro.Nombre;
    protected override bool Coincide(Usuario registro, string filtro)
        => Contiene($"{registro.IdUsuario} {registro.Nombre} {registro.NombreCompleto}", filtro);
    protected override Task<List<Usuario>> ListarAsync(CancellationToken ct) => _repositorio.ListarAsync(ct);
    protected override Task EliminarRegistroAsync(int id, CancellationToken ct) => _repositorio.EliminarAsync(id, _idActual, ct);

    protected override async Task GuardarRegistroAsync(int? id, CancellationToken ct)
    {
        var anterior = Seleccionado;
        string nombre = _nombre.Text.Trim().ToLowerInvariant();
        string completo = _completo.Text.Trim();
        await _repositorio.GuardarAsync(id, nombre, completo, _password.Text, ct);
        _password.Clear(); _confirmacion.Clear();
        if (anterior is not null && id == _idActual)
            UsuarioActualizado?.Invoke(anterior with { Nombre = nombre, NombreCompleto = completo });
    }

    protected override bool ValidarCampos()
    {
        bool nombre = Validar(_nombre, !string.IsNullOrWhiteSpace(_nombre.Text), "Ingresa el nombre de usuario.");
        bool completo = Validar(_completo, !string.IsNullOrWhiteSpace(_completo.Text), "Ingresa el nombre completo.");
        bool necesitaPassword = !IdSeleccionado.HasValue || _password.Text.Length > 0 || _confirmacion.Text.Length > 0;
        bool password = Validar(_password, !necesitaPassword || (!string.IsNullOrWhiteSpace(_password.Text) && _password.Text.Length >= 12), "La contraseña debe tener al menos 12 caracteres.");
        bool confirma = Validar(_confirmacion, _password.Text == _confirmacion.Text, "Las contraseñas no coinciden.");
        return nombre && completo && password && confirma;
    }

    protected override bool PuedeEliminar(Usuario registro, out string motivo)
    {
        motivo = "No puedes eliminar el usuario con el que has iniciado sesión.";
        return registro.IdUsuario != _idActual;
    }

    protected override void MostrarRegistro(Usuario registro)
    {
        _nombre.Text = registro.Nombre;
        _completo.Text = registro.NombreCompleto;
        _password.Clear(); _confirmacion.Clear();
    }

    protected override void LimpiarCampos()
    {
        _nombre.Clear(); _completo.Clear(); _password.Clear(); _confirmacion.Clear();
    }
}
