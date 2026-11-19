param([string]$EnvPath = (Join-Path $PSScriptRoot '../.env'))
$ErrorActionPreference = 'Stop'
$conexion = $null
$comando = $null
try {
    $dllDir = Join-Path $PSScriptRoot '../Sistema-Facturacion/bin/Debug/net10.0-windows'
    foreach ($dll in @('Microsoft.Extensions.DependencyInjection.Abstractions.dll', 'Microsoft.Extensions.Logging.Abstractions.dll', 'Npgsql.dll')) {
        Add-Type -Path (Join-Path $dllDir $dll)
    }
    $linea = Get-Content -LiteralPath $EnvPath | Where-Object { $_ -match '^\s*DATABASE_URL\s*=' } | Select-Object -First 1
    if (-not $linea) { throw [FormatException]::new('Falta DATABASE_URL.') }
    $valor = ($linea -split '=', 2)[1].Trim()
    if ($valor.Length -ge 2 -and (($valor.StartsWith('"') -and $valor.EndsWith('"')) -or ($valor.StartsWith("'") -and $valor.EndsWith("'")))) {
        $valor = $valor.Substring(1, $valor.Length - 2)
    }
    $uri = [Uri]::new($valor, [UriKind]::Absolute)
    if ($uri.Scheme -notin @('postgres', 'postgresql') -or $uri.Fragment) {
        throw [FormatException]::new('URI inválida; codifica caracteres especiales.')
    }
    $credenciales = $uri.UserInfo.Split(':', 2)
    if ($credenciales.Count -ne 2) { throw [FormatException]::new('Faltan credenciales.') }
    $config = [Npgsql.NpgsqlConnectionStringBuilder]::new()
    $config.Host = $uri.DnsSafeHost
    $config.Port = $(if ($uri.Port -gt 0) { $uri.Port } else { 5432 })
    $config.Database = [Uri]::UnescapeDataString($uri.AbsolutePath.TrimStart('/'))
    $config.Username = [Uri]::UnescapeDataString($credenciales[0])
    $config.Password = [Uri]::UnescapeDataString($credenciales[1])
    $config.Timeout = 15
    $config.CommandTimeout = 15
    $config.Pooling = $false
    $config.SslMode = [Npgsql.SslMode]::VerifyFull
    foreach ($item in $uri.Query.TrimStart('?').Split('&', [StringSplitOptions]::RemoveEmptyEntries)) {
        $par = $item.Split('=', 2)
        if ($par.Count -ne 2) { throw [FormatException]::new('Parámetro URI inválido.') }
        $clave = [Uri]::UnescapeDataString($par[0]).ToLowerInvariant()
        $dato = [Uri]::UnescapeDataString($par[1])
        switch ($clave) {
            'sslmode' {
                $config.SslMode = switch ($dato.ToLowerInvariant()) {
                    'require' { [Npgsql.SslMode]::Require }
                    'verify-ca' { [Npgsql.SslMode]::VerifyCA }
                    'verify-full' { [Npgsql.SslMode]::VerifyFull }
                    default { throw [FormatException]::new('Se requiere TLS.') }
                }
            }
            'sslrootcert' {
                $config.RootCertificate = [IO.Path]::GetFullPath($dato, [IO.Path]::GetDirectoryName([IO.Path]::GetFullPath($EnvPath)))
            }
            'connect_timeout' { $config.Timeout = [int]$dato }
            'application_name' { $config.ApplicationName = $dato }
            'options' { $config.Options = $dato }
            default { throw [FormatException]::new('Parámetro URI no admitido por el verificador.') }
        }
    }
    Write-Output ('Configuración leída de .env. TLS: ' + $config.SslMode)
    $conexion = [Npgsql.NpgsqlConnection]::new($config.ConnectionString)
    $conexion.Open()
    $comando = $conexion.CreateCommand()
    $comando.CommandText = 'SELECT 1'
    if ([int]$comando.ExecuteScalar() -ne 1) { throw [InvalidOperationException]::new('Respuesta inesperada.') }
    Write-Output 'OK: conexión y autenticación PostgreSQL; SELECT 1 correcto.'
    $comando.CommandText = "SELECT count(*) FROM information_schema.tables WHERE table_schema = 'facturacion' AND table_name IN ('usuario','producto','cliente','factura','detallefactura')"
    Write-Output ('Tablas esperadas visibles: ' + $comando.ExecuteScalar() + '/5')
    $comando.CommandText = "SELECT EXISTS (SELECT 1 FROM facturacion.usuario WHERE nombre = 'admin')"
    Write-Output ('Usuario admin presente: ' + $comando.ExecuteScalar())
    Write-Output 'Verificación terminada; no se modificaron datos.'
} catch {
    # No imprimir Message ni ToString: pueden incluir datos de la conexión.
    $causa = $_.Exception
    while ($causa.InnerException) { $causa = $causa.InnerException }
    Write-Output ('ERROR: ' + $causa.GetType().FullName)
    if ($causa -is [Net.Sockets.SocketException]) { Write-Output ('SocketError: ' + $causa.SocketErrorCode) }
    if ($causa.PSObject.Properties['SqlState']) { Write-Output ('SQLSTATE: ' + $causa.SqlState) }
    exit 1
} finally {
    if ($comando) { $comando.Dispose() }
    if ($conexion) { $conexion.Dispose() }
}
