using Sistema_Facturacion.CapaDatos;
using Sistema_Facturacion.Modelos;

namespace Sistema_Facturacion.Formularios;

public partial class FrmPrincipalMDI : Form
{
    public Usuario UsuarioActual { get; private set; }
    public Conexion Conexion { get; }
    public bool SolicitaCerrarSesion { get; private set; }

    public FrmPrincipalMDI(Usuario usuario, Conexion conexion)
    {
        UsuarioActual = usuario;
        Conexion = conexion;
        InitializeComponent();
        lblUsuario.Text = $"Usuario: {usuario.NombreCompleto} ({usuario.Nombre})";
        ActualizarReloj();
        reloj.Start();
    }

    // Único punto de apertura para los formularios de las siguientes etapas.
    public T AbrirFormulario<T>(Func<T> crearFormulario) where T : Form
    {
        var existente = MdiChildren.OfType<T>().FirstOrDefault(f => !f.IsDisposed);
        if (existente is not null)
        {
            if (existente.WindowState == FormWindowState.Minimized) existente.WindowState = FormWindowState.Normal;
            existente.Activate();
            return existente;
        }
        T formulario = crearFormulario();
        formulario.MdiParent = this;
        formulario.Show();
        return formulario;
    }

    private void ActualizarReloj()
        => lblFecha.Text = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTimeOffset.UtcNow, "Central America Standard Time")
            .ToString("dd/MM/yyyy HH:mm:ss");

    private void CerrarSesion(object? sender, EventArgs e)
    {
        if (MessageBox.Show(this, "¿Deseas cerrar la sesión actual?", "Cerrar sesión",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        SolicitaCerrarSesion = true;
        Close();
        if (!IsDisposed) SolicitaCerrarSesion = false;
    }

    private void AbrirProductos(bool nuevo)
    {
        var formulario = AbrirFormulario(() => new FrmProductos(Conexion));
        if (nuevo) formulario.PrepararNuevo();
    }

    private void AbrirClientes(bool nuevo)
    {
        var formulario = AbrirFormulario(() => new FrmClientes(Conexion));
        if (nuevo) formulario.PrepararNuevo();
    }

    private void AbrirUsuarios(bool nuevo)
    {
        var formulario = AbrirFormulario(() =>
        {
            var usuarios = new FrmUsuarios(Conexion, UsuarioActual.IdUsuario);
            usuarios.UsuarioActualizado += actualizado =>
            {
                UsuarioActual = actualizado;
                lblUsuario.Text = $"Usuario: {actualizado.NombreCompleto} ({actualizado.Nombre})";
            };
            return usuarios;
        });
        if (nuevo) formulario.PrepararNuevo();
    }
}
