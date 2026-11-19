using Npgsql;
using NpgsqlTypes;

namespace Sistema_Facturacion.CapaDatos;

public abstract class RepositorioBase(Conexion conexion)
{
    protected static NpgsqlParameter Parametro(string nombre, NpgsqlDbType tipo, object valor)
        => new(nombre, tipo) { Value = valor };

    protected async Task<List<T>> ConsultarAsync<T>(string sql, Func<NpgsqlDataReader, T> convertir, CancellationToken ct)
    {
        await using var cn = await conexion.AbrirAsync(ct);
        await using var cmd = new NpgsqlCommand(sql, cn);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        var resultado = new List<T>();
        while (await reader.ReadAsync(ct)) resultado.Add(convertir(reader));
        return resultado;
    }

    protected async Task EjecutarAsync(string sql, CancellationToken ct, params NpgsqlParameter[] parametros)
    {
        await using var cn = await conexion.AbrirAsync(ct);
        await using var cmd = new NpgsqlCommand(sql, cn);
        cmd.Parameters.AddRange(parametros);
        if (await cmd.ExecuteNonQueryAsync(ct) != 1)
            throw new InvalidOperationException("El registro ya no existe o no se puede modificar.");
    }
}
