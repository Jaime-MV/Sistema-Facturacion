#nullable enable
namespace Sistema_Facturacion.Formularios;

partial class FrmPrincipalMDI
{
    private System.ComponentModel.IContainer? components;
    private MenuStrip menu = null!;
    private StatusStrip estado = null!;
    private ToolStripStatusLabel lblUsuario = null!;
    private ToolStripStatusLabel lblFecha = null!;
    private System.Windows.Forms.Timer reloj = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing) components?.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        menu = new MenuStrip { Dock = DockStyle.Top, ShowItemToolTips = true };
        estado = new StatusStrip { Dock = DockStyle.Bottom };
        lblUsuario = new ToolStripStatusLabel();
        lblFecha = new ToolStripStatusLabel();
        reloj = new System.Windows.Forms.Timer(components) { Interval = 1000 };
        var productos = new ToolStripMenuItem("&Productos");
        productos.DropDownItems.Add("Nuevo / Registrar", null, (_, _) => AbrirProductos(true));
        productos.DropDownItems.Add("Consultar / Editar / Eliminar", null, (_, _) => AbrirProductos(false));
        var clientes = new ToolStripMenuItem("&Clientes");
        clientes.DropDownItems.Add("Nuevo / Registrar", null, (_, _) => AbrirClientes(true));
        clientes.DropDownItems.Add("Consultar / Editar / Eliminar", null, (_, _) => AbrirClientes(false));
        var facturas = new ToolStripMenuItem("&Facturas");
        facturas.DropDownItems.Add(new ToolStripMenuItem("Nueva factura") { Enabled = false });
        facturas.DropDownItems.Add(new ToolStripMenuItem("Reporte de ventas por fechas") { Enabled = false });
        var usuarios = new ToolStripMenuItem("&Usuarios");
        usuarios.DropDownItems.Add("Nuevo / Registrar", null, (_, _) => AbrirUsuarios(true));
        usuarios.DropDownItems.Add("Consultar / Editar / Eliminar", null, (_, _) => AbrirUsuarios(false));
        var sistema = new ToolStripMenuItem("&Sistema / Sesión");
        var cerrarSesion = new ToolStripMenuItem("&Cerrar sesión");
        var salir = new ToolStripMenuItem("&Salir");
        cerrarSesion.Click += CerrarSesion;
        salir.Click += (_, _) => Close();
        sistema.DropDownItems.AddRange([cerrarSesion, new ToolStripSeparator(), salir]);
        menu.Items.AddRange([productos, clientes, facturas, usuarios, sistema]);
        estado.Items.AddRange([lblUsuario, new ToolStripStatusLabel { Spring = true }, lblFecha]);
        Controls.Add(estado);
        Controls.Add(menu);
        MainMenuStrip = menu;
        IsMdiContainer = true;
        WindowState = FormWindowState.Maximized;
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(1100, 700);
        MinimumSize = new Size(800, 500);
        Font = new Font("Segoe UI", 10F);
        AutoScaleMode = AutoScaleMode.Font;
        Text = "Sistema de facturación";
        reloj.Tick += (_, _) => ActualizarReloj();
    }
}
