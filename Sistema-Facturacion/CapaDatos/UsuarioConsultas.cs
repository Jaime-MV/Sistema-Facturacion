using Npgsql;
using NpgsqlTypes;
using Sistema_Facturacion.Modelos;
using Sistema_Facturacion.Seguridad;

namespace Sistema_Facturacion.CapaDatos;

/// <summary>Consulta parametrizada lista para usar desde FrmLogin en la etapa 3.</summary>
public sealed class UsuarioConsultas(Conexion conexion)
{
    public async Task<Usuario?> AutenticarAsync(string nombre, string password,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrEmpty(password))
            return null;

        await using var cn = await conexion.AbrirAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand("""
            SELECT idusuario, nombre, nombrecompleto, fecharegistro, passwordhash, passwordsalt
            FROM facturacion.usuario WHERE nombre = @nombre
            """, cn);
        cmd.Parameters.Add(new NpgsqlParameter("nombre", NpgsqlDbType.Varchar)
        {
            Value = nombre.Trim().ToLowerInvariant()
        });
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;
        if (!SeguridadPassword.Verificar(password, reader.GetString(4), reader.GetString(5)))
            return null;
        return new Usuario(reader.GetInt32(0), reader.GetString(1), reader.GetString(2), reader.GetDateTime(3));
    }
}