using Npgsql;

namespace Sistema_Facturacion.CapaDatos;

/// <summary>Una instancia por aplicación. Cada operación abre y dispone su propia conexión.</summary>
public sealed class Conexion : IDisposable, IAsyncDisposable
{
    public const string VariableEntorno = "FACTURACION_CONNECTION_STRING";
    private readonly NpgsqlDataSource _dataSource;

    public Conexion(string cadenaConexion)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cadenaConexion);
        var opciones = new NpgsqlConnectionStringBuilder(cadenaConexion);
        if (string.IsNullOrWhiteSpace(opciones.Host) || string.IsNullOrWhiteSpace(opciones.Database)
            || string.IsNullOrWhiteSpace(opciones.Username))
            throw new ArgumentException("Configura Host, Database y Username.", nameof(cadenaConexion));
        if (opciones.SslMode is not (SslMode.Require or SslMode.VerifyCA or SslMode.VerifyFull))
            throw new ArgumentException("La conexión remota requiere TLS.", nameof(cadenaConexion));

        opciones.IncludeErrorDetail = false;
        opciones.PersistSecurityInfo = false;
        _dataSource = NpgsqlDataSource.Create(opciones.ConnectionString);
    }

    public static Conexion DesdeVariableEntorno()
    {
        string? cadena = Environment.GetEnvironmentVariable(VariableEntorno);
        if (string.IsNullOrWhiteSpace(cadena))
            throw new InvalidOperationException($"Configura la variable de entorno {VariableEntorno} y reinicia Visual Studio.");
        return new Conexion(cadena);
    }

    public ValueTask<NpgsqlConnection> AbrirAsync(CancellationToken cancellationToken = default)
        => _dataSource.OpenConnectionAsync(cancellationToken);

    public static Conexion DesdeEnv(string? archivo = null)
        => new(ConfiguracionConexion.LeerEnv(archivo));

    public async Task<bool> ProbarAsync(CancellationToken cancellationToken = default)
    {
        await using var conexion = await AbrirAsync(cancellationToken);
        await using var comando = new NpgsqlCommand("SELECT 1", conexion);
        return Convert.ToInt32(await comando.ExecuteScalarAsync(cancellationToken)) == 1;
    }

    public void Dispose() => _dataSource.Dispose();
    public ValueTask DisposeAsync() => _dataSource.DisposeAsync();
}
