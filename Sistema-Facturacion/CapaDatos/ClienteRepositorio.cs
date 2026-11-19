using Npgsql;
using NpgsqlTypes;
using Sistema_Facturacion.Modelos;
using Sistema_Facturacion.Seguridad;

namespace Sistema_Facturacion.CapaDatos;

public sealed class ClienteRepositorio(Conexion conexion) : RepositorioBase(conexion)
{
    public Task<List<Cliente>> ListarAsync(CancellationToken ct = default)
        => ConsultarAsync("SELECT idcliente, nombre, telefono, email, direccion FROM facturacion.cliente ORDER BY idcliente DESC",
            r => new Cliente(r.GetInt32(0), r.GetString(1), r.GetString(2), r.GetString(3), r.GetString(4)), ct);

    public Task GuardarAsync(int? id, string nombre, string telefono, string email, string direccion, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nombre);
        if (nombre.Trim().Length > 150 || telefono.Trim().Length > 30 || email.Trim().Length > 254
            || direccion.Trim().Length > 300 || !Validaciones.EmailOpcionalValido(email.Trim()))
            throw new ArgumentException("Datos de cliente inválidos.");
        var parametros = new List<NpgsqlParameter>
        {
            Parametro("nombre", NpgsqlDbType.Varchar, nombre.Trim()),
            Parametro("telefono", NpgsqlDbType.Varchar, telefono.Trim()),
            Parametro("email", NpgsqlDbType.Varchar, email.Trim()),
            Parametro("direccion", NpgsqlDbType.Varchar, direccion.Trim())
        };
        string sql;
        if (id.HasValue)
        {
            sql = "UPDATE facturacion.cliente SET nombre=@nombre, telefono=@telefono, email=@email, direccion=@direccion WHERE idcliente=@id";
            parametros.Add(Parametro("id", NpgsqlDbType.Integer, id.Value));
        }
        else sql = "INSERT INTO facturacion.cliente (nombre,telefono,email,direccion) VALUES (@nombre,@telefono,@email,@direccion)";
        return EjecutarAsync(sql, ct, parametros.ToArray());
    }

    public Task EliminarAsync(int id, CancellationToken ct = default)
        => EjecutarAsync("DELETE FROM facturacion.cliente WHERE idcliente=@id", ct, Parametro("id", NpgsqlDbType.Integer, id));
}
