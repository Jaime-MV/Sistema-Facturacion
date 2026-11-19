using Npgsql;

namespace Sistema_Facturacion.CapaDatos;

public static class ConfiguracionConexion
{
    public static string LeerEnv(string? archivo = null)
    {
        archivo = Path.GetFullPath(archivo ?? Path.Combine(AppContext.BaseDirectory, ".env"));
        if (!File.Exists(archivo))
            throw new InvalidOperationException("No se encontró .env junto al ejecutable. Revisa el archivo de la raíz del proyecto y vuelve a compilar.");
        string? valor = null;
        foreach (string linea in File.ReadLines(archivo))
        {
            string texto = linea.Trim();
            if (texto.Length == 0 || texto.StartsWith('#')) continue;
            int igual = texto.IndexOf('=');
            if (igual < 0 || texto[..igual].Trim() != "DATABASE_URL") continue;
            if (valor is not null) throw new InvalidOperationException("DATABASE_URL está duplicada en .env.");
            valor = texto[(igual + 1)..].Trim();
            if (valor.Length >= 2 && ((valor[0] == '"' && valor[^1] == '"') || (valor[0] == '\'' && valor[^1] == '\'')))
                valor = valor[1..^1];
        }
        if (string.IsNullOrWhiteSpace(valor)) throw new InvalidOperationException("Configura DATABASE_URL en .env.");
        return ConvertirUri(valor, Path.GetDirectoryName(archivo)!);
    }

    public static string ConvertirUri(string valor, string directorioBase)
    {
        // Los errores nunca incluyen la URI, pues contiene credenciales.
        try
        {
            if (!Uri.TryCreate(valor, UriKind.Absolute, out var uri)
                || uri.Scheme is not ("postgres" or "postgresql")
                || string.IsNullOrEmpty(uri.Host) || uri.Fragment.Length != 0) throw new FormatException();
            string[] credenciales = uri.UserInfo.Split(':', 2);
            if (credenciales.Length != 2) throw new FormatException();
            var opciones = new NpgsqlConnectionStringBuilder
            {
                Host = uri.DnsSafeHost,
                Port = uri.Port > 0 ? uri.Port : 5432,
                Database = Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/')),
                Username = Uri.UnescapeDataString(credenciales[0]),
                Password = Uri.UnescapeDataString(credenciales[1]),
                SslMode = SslMode.VerifyFull,
                Timeout = 15,
                CommandTimeout = 30,
                Options = "-c timezone=America/El_Salvador"
            };
            foreach (string parametro in uri.Query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
            {
                string[] par = parametro.Split('=', 2);
                if (par.Length != 2) throw new FormatException();
                string dato = Uri.UnescapeDataString(par[1]);
                switch (Uri.UnescapeDataString(par[0]).ToLowerInvariant())
                {
                    case "sslmode":
                        opciones.SslMode = dato.ToLowerInvariant() switch
                        {
                            "require" => SslMode.Require,
                            "verify-ca" => SslMode.VerifyCA,
                            "verify-full" => SslMode.VerifyFull,
                            _ => throw new FormatException()
                        };
                        break;
                    case "sslrootcert": opciones.RootCertificate = Path.GetFullPath(dato, directorioBase); break;
                    case "connect_timeout": opciones.Timeout = int.Parse(dato); break;
                    case "application_name": opciones.ApplicationName = dato; break;
                    case "options": opciones.Options = dato; break;
                    default: throw new FormatException();
                }
            }
            if (string.IsNullOrWhiteSpace(opciones.Database) || string.IsNullOrWhiteSpace(opciones.Username)
                || string.IsNullOrEmpty(opciones.Password)) throw new FormatException();
            return opciones.ConnectionString;
        }
        catch (Exception ex) when (ex is ArgumentException or FormatException or OverflowException)
        {
            throw new InvalidOperationException("DATABASE_URL no es válida. Revisa usuario, contraseña codificada para URI, host, puerto, base y parámetros TLS.");
        }
    }
}
