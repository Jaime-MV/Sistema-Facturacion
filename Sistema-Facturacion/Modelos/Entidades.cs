namespace Sistema_Facturacion.Modelos;

// Usuario representa la sesión. El hash y el salt no se muestran en los listados.
public sealed record Usuario(int IdUsuario, string Nombre, string NombreCompleto, DateTime FechaRegistro);
public sealed record Producto(int IdProducto, string Descripcion, int Cantidad, decimal Precio);
public sealed record Cliente(int IdCliente, string Nombre, string Telefono, string Email, string Direccion);
public sealed record Factura(int IdFactura, int IdCliente, string Cliente, DateTime Fecha,
    decimal Subtotal, decimal IVA, decimal Total);
public sealed record DetalleFactura(int IdDetalle, int IdFactura, int IdProducto,
    int Cantidad, decimal Precio, decimal Total);