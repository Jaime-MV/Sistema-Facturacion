using Sistema_Facturacion.CapaDatos;
using Sistema_Facturacion.Modelos;

namespace Sistema_Facturacion.Formularios;

public sealed class ContextoAplicacion : ApplicationContext
{
    private readonly Conexion _conexion;
    private readonly FrmLogin _login;
    private FrmPrincipalMDI? _principal;

    public ContextoAplicacion(Conexion conexion)
    {
        _conexion = conexion;
        _login = new FrmLogin(new UsuarioConsultas(conexion));
        _login.UsuarioAutenticado += IniciarSesion;
        _login.FormClosed += (_, _) => ExitThread();
        _login.Show();
    }

    private void IniciarSesion(Usuario usuario)
    {
        _principal = new FrmPrincipalMDI(usuario, _conexion);
        _principal.FormClosed += PrincipalCerrado;
        _principal.Show();
        _login.Hide();
    }

    private void PrincipalCerrado(object? sender, FormClosedEventArgs e)
    {
        bool volver = _principal?.SolicitaCerrarSesion == true;
        _principal = null;
        if (volver && !_login.IsDisposed)
        {
            _login.PrepararNuevaSesion();
            _login.Show();
            _login.Activate();
        }
        else _login.Close();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) { _principal?.Dispose(); _login.Dispose(); }
        base.Dispose(disposing);
    }
}
