using Npgsql;

namespace Sistema_Facturacion.Formularios;

public abstract class FrmCrud<T> : Form where T : class
{
    private readonly CancellationTokenSource _cierre = new();
    private readonly ErrorProvider _errores;
    private readonly TableLayoutPanel _editor;
    private readonly FlowLayoutPanel _botones;
    private readonly Button _guardar = new() { Text = "Guardar", AutoSize = true };
    private readonly Button _editar = new() { Text = "Editar", AutoSize = true };
    private readonly Button _eliminar = new() { Text = "Eliminar", AutoSize = true };
    private readonly Button _limpiar = new() { Text = "Limpiar", AutoSize = true };
    private readonly Button _actualizar = new() { Text = "Actualizar lista", AutoSize = true };
    private readonly TextBox _busqueda = new() { Dock = DockStyle.Fill, PlaceholderText = "Escribe para filtrar…", TabIndex = 0 };
    private readonly Label _estado = new() { Dock = DockStyle.Fill, AutoSize = true, Padding = new Padding(0, 8, 0, 4) };
    private readonly Label _modo = new() { AutoSize = true, Text = "Nuevo registro", Padding = new Padding(0, 6, 0, 6) };
    private List<T> _registros = [];
    private bool _ocupado;
    private bool _mutando;
    private bool _enlazando;
    protected T? Seleccionado { get; private set; }
    protected int? IdSeleccionado => Seleccionado is null ? null : ObtenerId(Seleccionado);
    protected DataGridView Tabla { get; } = new()
    {
        Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = false,
        AllowUserToAddRows = false, AllowUserToDeleteRows = false, MultiSelect = false,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect, RowHeadersVisible = false,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, TabIndex = 3,
        BackgroundColor = SystemColors.Window, BorderStyle = BorderStyle.Fixed3D
    };

    protected FrmCrud(string titulo)
    {
        Text = titulo;
        Font = new Font("Segoe UI", 10F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1000, 680);
        MinimumSize = new Size(800, 600);
        StartPosition = FormStartPosition.CenterParent;
        _errores = new ErrorProvider { ContainerControl = this, BlinkStyle = ErrorBlinkStyle.NeverBlink };
        var raiz = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(16), ColumnCount = 1, RowCount = 7 };
        raiz.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        for (int i = 0; i < 7; i++)
            raiz.RowStyles.Add(new RowStyle(i == 5 ? SizeType.Percent : SizeType.AutoSize, i == 5 ? 100 : 0));
        _editor = new TableLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, ColumnCount = 2, TabIndex = 1 };
        _editor.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180));
        _editor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        _botones = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, Padding = new Padding(0, 6, 0, 8), TabIndex = 2 };
        int orden = 0;
        foreach (var boton in new[] { _guardar, _editar, _eliminar, _limpiar })
        {
            boton.MinimumSize = new Size(100, 34);
            boton.TabIndex = orden++;
            _botones.Controls.Add(boton);
        }
        var filtro = new TableLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, ColumnCount = 3, Padding = new Padding(0, 6, 0, 6), TabIndex = 0 };
        filtro.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70));
        filtro.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        filtro.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        filtro.Controls.Add(new Label { Text = "Buscar", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 0);
        filtro.Controls.Add(_busqueda, 1, 0);
        filtro.Controls.Add(_actualizar, 2, 0);
        raiz.Controls.Add(new Label { Text = titulo, Font = new Font("Segoe UI", 16F, FontStyle.Bold), AutoSize = true }, 0, 0);
        raiz.Controls.Add(_modo, 0, 1);
        raiz.Controls.Add(_editor, 0, 2);
        raiz.Controls.Add(_botones, 0, 3);
        raiz.Controls.Add(filtro, 0, 4);
        raiz.Controls.Add(Tabla, 0, 5);
        raiz.Controls.Add(_estado, 0, 6);
        Controls.Add(raiz);
        _guardar.Click += async (_, _) => await GuardarAsync(false);
        _editar.Click += async (_, _) => await GuardarAsync(true);
        _eliminar.Click += async (_, _) => await EliminarAsync();
        _limpiar.Click += (_, _) => PrepararNuevo();
        _actualizar.Click += async (_, _) => await EjecutarAsync(RecargarAsync);
        _busqueda.TextChanged += (_, _) => { if (!_ocupado) { Filtrar(); LimpiarEditor(); } };
        Tabla.SelectionChanged += (_, _) =>
        {
            if (_enlazando || _ocupado) return;
            if (Tabla.SelectedRows.Count == 1 && Tabla.SelectedRows[0].DataBoundItem is T registro)
            {
                Seleccionado = registro;
                _errores.Clear();
                MostrarRegistro(registro);
                _modo.Text = $"Editando registro #{ObtenerId(registro)}";
                ActualizarBotones();
            }
        };
        ActualizarBotones();
    }

    protected TextBox CrearTexto(int longitud, bool password = false)
        => new() { Dock = DockStyle.Fill, MaxLength = longitud, UseSystemPasswordChar = password };

    protected void AgregarCampo(string etiqueta, Control control)
    {
        int fila = _editor.RowCount++;
        _editor.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        control.TabIndex = fila;
        control.Margin = new Padding(3, 4, 24, 4);
        _editor.Controls.Add(new Label { Text = etiqueta, AutoSize = true, Anchor = AnchorStyles.Left }, 0, fila);
        _editor.Controls.Add(control, 1, fila);
    }

    protected void Columna(string propiedad, string titulo, float ancho, string? formato = null)
    {
        var columna = new DataGridViewTextBoxColumn { DataPropertyName = propiedad, HeaderText = titulo,
            FillWeight = ancho, SortMode = DataGridViewColumnSortMode.NotSortable };
        if (formato is not null) columna.DefaultCellStyle.Format = formato;
        Tabla.Columns.Add(columna);
    }

    protected bool Validar(Control control, bool condicion, string mensaje)
    {
        if (!condicion) _errores.SetError(control, mensaje);
        return condicion;
    }

    protected static bool Contiene(string texto, string filtro)
        => texto.Contains(filtro, StringComparison.CurrentCultureIgnoreCase);

    protected static void SoloNumeros(object? sender, KeyPressEventArgs e)
        => e.Handled = !char.IsControl(e.KeyChar) && !char.IsAsciiDigit(e.KeyChar);

    protected static void SoloPrecio(object? sender, KeyPressEventArgs e)
    {
        if (char.IsControl(e.KeyChar) || char.IsAsciiDigit(e.KeyChar)) return;
        e.Handled = e.KeyChar != '.' || sender is not TextBox texto
            || texto.Text.Remove(texto.SelectionStart, texto.SelectionLength).Contains('.');
    }

    public void PrepararNuevo()
    {
        if (_ocupado) return;
        _enlazando = true;
        Tabla.ClearSelection();
        Tabla.CurrentCell = null;
        _enlazando = false;
        LimpiarEditor();
        _editor.GetControlFromPosition(1, 0)?.Focus();
    }

    private void LimpiarEditor()
    {
        Seleccionado = null;
        _errores.Clear();
        LimpiarCampos();
        _modo.Text = "Nuevo registro";
        ActualizarBotones();
    }

    protected override async void OnShown(EventArgs e)
    {
        base.OnShown(e);
        LimpiarEditor();
        await EjecutarAsync(RecargarAsync);
    }

    private void Filtrar()
    {
        string filtro = _busqueda.Text.Trim();
        var visibles = _registros.Where(r => Coincide(r, filtro)).ToList();
        _enlazando = true;
        try
        {
            Tabla.DataSource = visibles;
            Tabla.ClearSelection();
            Tabla.CurrentCell = null;
        }
        finally { _enlazando = false; }
        _estado.Text = $"{visibles.Count} de {_registros.Count} registros. Selecciona una fila para editar o eliminar.";
    }

    private async Task RecargarAsync()
    {
        var registros = await ListarAsync(_cierre.Token);
        if (IsDisposed || Disposing) return;
        _registros = registros;
        Filtrar();
        LimpiarEditor();
    }

    private async Task GuardarAsync(bool editar)
    {
        if (_ocupado || editar != IdSeleccionado.HasValue) return;
        _errores.Clear();
        if (!ValidarCampos())
        {
            _estado.Text = "Revisa los campos marcados antes de continuar.";
            foreach (Control control in _editor.Controls)
                if (_errores.GetError(control).Length > 0) { control.Focus(); break; }
            return;
        }
        await EjecutarAsync(async () =>
        {
            await GuardarRegistroAsync(IdSeleccionado, _cierre.Token);
            await TrasCambioAsync(editar ? "Registro actualizado." : "Registro guardado.");
        }, mutando: true);
    }

    private async Task EliminarAsync()
    {
        if (_ocupado || Seleccionado is null) return;
        if (!PuedeEliminar(Seleccionado, out string motivo))
        {
            MessageBox.Show(this, motivo, Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        int id = ObtenerId(Seleccionado);
        if (MessageBox.Show(this, $"¿Eliminar el registro #{id}: {Describir(Seleccionado)}?", "Confirmar eliminación",
            MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) != DialogResult.Yes) return;
        await EjecutarAsync(async () =>
        {
            await EliminarRegistroAsync(id, _cierre.Token);
            await TrasCambioAsync("Registro eliminado.");
        }, mutando: true);
    }

    private async Task TrasCambioAsync(string mensaje)
    {
        if (IsDisposed || Disposing) return;
        _busqueda.Clear();
        LimpiarEditor();
        // La escritura ya terminó: si falla la recarga, el mensaje debe distinguirlo.
        try
        {
            await RecargarAsync();
            if (!IsDisposed && !Disposing) _estado.Text = mensaje + " " + _estado.Text;
        }
        catch (Exception ex) when (ex is NpgsqlException or IOException or TimeoutException or OperationCanceledException)
        {
            if (!IsDisposed && !Disposing)
            {
                _registros = [];
                Filtrar();
                _estado.Text = mensaje + " No se pudo actualizar el listado; pulsa Actualizar lista.";
                MessageBox.Show(this, _estado.Text, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }

    private async Task EjecutarAsync(Func<Task> accion, bool mutando = false)
    {
        if (_ocupado || IsDisposed) return;
        _ocupado = true;
        _mutando = mutando;
        _editor.Enabled = _botones.Enabled = Tabla.Enabled = _busqueda.Enabled = _actualizar.Enabled = false;
        UseWaitCursor = true;
        _estado.Text = "Procesando…";
        try { await accion(); }
        catch (OperationCanceledException) when (_cierre.IsCancellationRequested) { }
        catch (Exception ex) when (ex is NpgsqlException or IOException or TimeoutException or InvalidOperationException or ArgumentException)
        {
            if (!IsDisposed && !Disposing)
            {
                string mensaje = ex switch
                {
                    PostgresException { SqlState: "23505" } => "Ya existe un usuario con ese nombre. Usa otro nombre de usuario.",
                    PostgresException { SqlState: "23503" } => "No se puede eliminar: el registro está asociado a una factura.",
                    PostgresException { SqlState: "23514" or "22003" or "22001" } => "Los datos no cumplen los límites de la base de datos. Revisa los campos.",
                    InvalidOperationException => "El registro ya no existe o no puede modificarse. Actualiza la lista.",
                    ArgumentException => "Revisa los datos ingresados antes de continuar.",
                    _ => "No se pudo completar la operación. Comprueba la conexión a Aiven y actualiza la lista antes de reintentar."
                };
                _estado.Text = mensaje;
                MessageBox.Show(this, mensaje, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        finally
        {
            _ocupado = _mutando = false;
            if (!IsDisposed && !Disposing)
            {
                UseWaitCursor = false;
                _editor.Enabled = _botones.Enabled = Tabla.Enabled = _busqueda.Enabled = _actualizar.Enabled = true;
                ActualizarBotones();
            }
        }
    }

    private void ActualizarBotones()
    {
        _guardar.Enabled = Seleccionado is null;
        _editar.Enabled = _eliminar.Enabled = Seleccionado is not null;
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (_mutando)
        {
            e.Cancel = true;
            _estado.Text = "Espera a que termine la operación antes de cerrar.";
        }
        base.OnFormClosing(e);
        if (!e.Cancel) _cierre.Cancel();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) { _errores.Dispose(); _cierre.Dispose(); }
        base.Dispose(disposing);
    }

    protected virtual bool PuedeEliminar(T registro, out string motivo) { motivo = ""; return true; }
    protected abstract int ObtenerId(T registro);
    protected abstract string Describir(T registro);
    protected abstract bool Coincide(T registro, string filtro);
    protected abstract Task<List<T>> ListarAsync(CancellationToken ct);
    protected abstract Task GuardarRegistroAsync(int? id, CancellationToken ct);
    protected abstract Task EliminarRegistroAsync(int id, CancellationToken ct);
    protected abstract bool ValidarCampos();
    protected abstract void MostrarRegistro(T registro);
    protected abstract void LimpiarCampos();
}
