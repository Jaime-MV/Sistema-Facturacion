using Npgsql;
using NpgsqlTypes;
using Sistema_Facturacion.Modelos;

namespace Sistema_Facturacion.CapaDatos;

public sealed class ProductoRepositorio(Conexion conexion) : RepositorioBase(conexion)
{
    public Task<List<Producto>> ListarAsync(CancellationToken ct = default)
        => ConsultarAsync("SELECT idproducto, descripcion, cantidad, precio FROM facturacion.producto ORDER BY idproducto DESC",
            r => new Producto(r.GetInt32(0), r.GetString(1), r.GetInt32(2), r.GetDecimal(3)), ct);

    public Task GuardarAsync(int? id, string descripcion, int cantidad, decimal precio, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);
        if (descripcion.Trim().Length > 200 || cantidad < 0 || precio < 0 || precio > 99999999.99m
            || decimal.Round(precio, 2) != precio) throw new ArgumentException("Datos de producto inválidos.");
        var parametros = new List<NpgsqlParameter>
        {
            Parametro("descripcion", NpgsqlDbType.Varchar, descripcion.Trim()),
            Parametro("cantidad", NpgsqlDbType.Integer, cantidad),
            Parametro("precio", NpgsqlDbType.Numeric, precio)
        };
        string sql;
        if (id.HasValue)
        {
            sql = "UPDATE facturacion.producto SET descripcion=@descripcion, cantidad=@cantidad, precio=@precio WHERE idproducto=@id";
            parametros.Add(Parametro("id", NpgsqlDbType.Integer, id.Value));
        }
        else sql = "INSERT INTO facturacion.producto (descripcion,cantidad,precio) VALUES (@descripcion,@cantidad,@precio)";
        return EjecutarAsync(sql, ct, parametros.ToArray());
    }

    public Task EliminarAsync(int id, CancellationToken ct = default)
        => EjecutarAsync("DELETE FROM facturacion.producto WHERE idproducto=@id", ct, Parametro("id", NpgsqlDbType.Integer, id));
}
