#nullable enable
namespace Sistema_Facturacion.Formularios;

partial class FrmLogin
{
    private System.ComponentModel.IContainer? components;
    private TextBox txtUsuario = null!;
    private TextBox txtPassword = null!;
    private CheckBox chkMostrar = null!;
    private Button btnIngresar = null!;
    private Label lblEstado = null!;
    private ErrorProvider errores = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing) { components?.Dispose(); _cierre.Dispose(); }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        txtUsuario = new TextBox { Name = "txtUsuario", Dock = DockStyle.Fill, MaxLength = 50, TabIndex = 0 };
        txtPassword = new TextBox { Name = "txtPassword", Dock = DockStyle.Fill, UseSystemPasswordChar = true, TabIndex = 1 };
        chkMostrar = new CheckBox { Text = "Mostrar contraseña", AutoSize = true, TabIndex = 2 };
        btnIngresar = new Button { Text = "Ingresar", AutoSize = true, MinimumSize = new Size(110, 36), TabIndex = 3 };
        var btnSalir = new Button { Text = "Salir", AutoSize = true, MinimumSize = new Size(90, 36), TabIndex = 4, DialogResult = DialogResult.Cancel };
        lblEstado = new Label { Text = "Ingresa tus credenciales para continuar.", AutoSize = true, Dock = DockStyle.Fill, Margin = new Padding(0, 14, 0, 0) };
        errores = new ErrorProvider(components) { ContainerControl = this, BlinkStyle = ErrorBlinkStyle.NeverBlink };
        var titulo = new Label { Text = "Sistema de facturación", Font = new Font("Segoe UI", 17F, FontStyle.Bold), AutoSize = true, Margin = new Padding(0, 0, 0, 18) };
        var botones = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, FlowDirection = FlowDirection.RightToLeft, Margin = new Padding(0, 10, 0, 0), TabIndex = 3 };
        botones.Controls.Add(btnSalir);
        botones.Controls.Add(btnIngresar);
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(28), ColumnCount = 1, RowCount = 8 };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        for (int i = 0; i < 8; i++) layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.Controls.Add(titulo, 0, 0);
        layout.Controls.Add(new Label { Text = "Usuario", AutoSize = true }, 0, 1);
        layout.Controls.Add(txtUsuario, 0, 2);
        layout.Controls.Add(new Label { Text = "Contraseña", AutoSize = true, Margin = new Padding(3, 12, 3, 3) }, 0, 3);
        layout.Controls.Add(txtPassword, 0, 4);
        layout.Controls.Add(chkMostrar, 0, 5);
        layout.Controls.Add(botones, 0, 6);
        layout.Controls.Add(lblEstado, 0, 7);
        Controls.Add(layout);
        Text = "Iniciar sesión — Facturación";
        ClientSize = new Size(500, 465);
        AutoScaleMode = AutoScaleMode.Font;
        Font = new Font("Segoe UI", 10F);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        AcceptButton = btnIngresar;
        CancelButton = btnSalir;
        ActiveControl = txtUsuario;
        btnIngresar.Click += Ingresar;
        btnSalir.Click += (_, _) => Close();
        chkMostrar.CheckedChanged += (_, _) => txtPassword.UseSystemPasswordChar = !chkMostrar.Checked;
    }
}
