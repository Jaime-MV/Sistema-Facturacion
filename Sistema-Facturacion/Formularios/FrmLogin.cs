using Npgsql;
using Sistema_Facturacion.CapaDatos;
using Sistema_Facturacion.Modelos;

namespace Sistema_Facturacion.Formularios;

public partial class FrmLogin : Form
{
    private readonly UsuarioConsultas _usuarios;
    private readonly CancellationTokenSource _cierre = new();
    private bool _autenticando;
    public event Action<Usuario>? UsuarioAutenticado;

    public FrmLogin(UsuarioConsultas usuarios)
    {
        _usuarios = usuarios;
        InitializeComponent();
    }

    public void PrepararNuevaSesion()
    {
        txtUsuario.Clear();
        txtPassword.Clear();
        chkMostrar.Checked = false;
        errores.Clear();
        lblEstado.Text = "Ingresa tus credenciales para continuar.";
        ActiveControl = txtUsuario;
    }

    private async void Ingresar(object? sender, EventArgs e)
    {
        if (_autenticando) return;
        errores.Clear();
        if (string.IsNullOrWhiteSpace(txtUsuario.Text)) errores.SetError(txtUsuario, "Ingresa tu usuario.");
        if (string.IsNullOrEmpty(txtPassword.Text)) errores.SetError(txtPassword, "Ingresa tu contraseña.");
        if (!string.IsNullOrEmpty(errores.GetError(txtUsuario))) { txtUsuario.Focus(); return; }
        if (!string.IsNullOrEmpty(errores.GetError(txtPassword))) { txtPassword.Focus(); return; }
        _autenticando = true;
        btnIngresar.Enabled = false;
        txtUsuario.Enabled = txtPassword.Enabled = chkMostrar.Enabled = false;
        UseWaitCursor = true;
        lblEstado.Text = "Verificando credenciales…";
        Usuario? usuario = null;
        try
        {
            usuario = await _usuarios.AutenticarAsync(txtUsuario.Text, txtPassword.Text, _cierre.Token);
            if (IsDisposed || Disposing) return;
            if (usuario is null)
            {
                lblEstado.Text = "Usuario o contraseña incorrectos.";
                txtPassword.Clear();
            }
            else lblEstado.Text = "Sesión iniciada.";
        }
        catch (OperationCanceledException) when (_cierre.IsCancellationRequested) { }
        catch (Exception ex) when (ex is NpgsqlException or TimeoutException or IOException or ArgumentException)
        {
            if (!IsDisposed && !Disposing)
            {
                lblEstado.Text = "No se pudo verificar el acceso.";
                MessageBox.Show(this, "No fue posible consultar la base de datos. Revisa tu conexión a Internet y la configuración de Aiven en .env.",
                    "Error de conexión", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        finally
        {
            _autenticando = false;
            if (!IsDisposed && !Disposing)
            {
                UseWaitCursor = false;
                btnIngresar.Enabled = true;
                txtUsuario.Enabled = txtPassword.Enabled = chkMostrar.Enabled = true;
                txtPassword.Focus();
            }
        }
        if (usuario is not null && !IsDisposed && !Disposing)
        {
            txtPassword.Clear();
            UsuarioAutenticado?.Invoke(usuario);
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);
        if (!e.Cancel) _cierre.Cancel();
    }
}
