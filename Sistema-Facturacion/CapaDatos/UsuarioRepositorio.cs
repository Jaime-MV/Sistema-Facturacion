using Npgsql;
using NpgsqlTypes;
using Sistema_Facturacion.Modelos;
using Sistema_Facturacion.Seguridad;

namespace Sistema_Facturacion.CapaDatos;

public sealed class UsuarioRepositorio(Conexion conexion) : RepositorioBase(conexion)
{
    public Task<List<Usuario>> ListarAsync(CancellationToken ct = default)
        => ConsultarAsync("SELECT idusuario, nombre, nombrecompleto, fecharegistro FROM facturacion.usuario ORDER BY idusuario DESC",
            r => new Usuario(r.GetInt32(0), r.GetString(1), r.GetString(2), r.GetDateTime(3)), ct);

    public Task GuardarAsync(int? id, string nombre, string nombreCompleto, string password, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nombre);
        ArgumentException.ThrowIfNullOrWhiteSpace(nombreCompleto);
        if (nombre.Trim().Length > 50 || nombreCompleto.Trim().Length > 150)
            throw new ArgumentException("Datos de usuario inválidos.");
        var parametros = new List<NpgsqlParameter>
        {
            Parametro("nombre", NpgsqlDbType.Varchar, nombre.Trim().ToLowerInvariant()),
            Parametro("completo", NpgsqlDbType.Varchar, nombreCompleto.Trim())
        };
        bool cambiarPassword = !id.HasValue || password.Length > 0;
        if (cambiarPassword)
        {
            var credenciales = SeguridadPassword.Crear(password);
            parametros.Add(Parametro("hash", NpgsqlDbType.Varchar, credenciales.PasswordHash));
            parametros.Add(Parametro("salt", NpgsqlDbType.Varchar, credenciales.PasswordSalt));
        }
        string sql;
        if (id.HasValue)
        {
            sql = cambiarPassword
                ? "UPDATE facturacion.usuario SET nombre=@nombre, nombrecompleto=@completo, passwordhash=@hash, passwordsalt=@salt WHERE idusuario=@id"
                : "UPDATE facturacion.usuario SET nombre=@nombre, nombrecompleto=@completo WHERE idusuario=@id";
            parametros.Add(Parametro("id", NpgsqlDbType.Integer, id.Value));
        }
        else sql = "INSERT INTO facturacion.usuario (nombre,nombrecompleto,passwordhash,passwordsalt) VALUES (@nombre,@completo,@hash,@salt)";
        return EjecutarAsync(sql, ct, parametros.ToArray());
    }

    public Task EliminarAsync(int id, int idUsuarioActual, CancellationToken ct = default)
    {
        if (id == idUsuarioActual) throw new InvalidOperationException("No puedes eliminar el usuario de la sesión actual.");
        return EjecutarAsync("DELETE FROM facturacion.usuario WHERE idusuario=@id AND idusuario<>@actual", ct,
            Parametro("id", NpgsqlDbType.Integer, id), Parametro("actual", NpgsqlDbType.Integer, idUsuarioActual));
    }
}
