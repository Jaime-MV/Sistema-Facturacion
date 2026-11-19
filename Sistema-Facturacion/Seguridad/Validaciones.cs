using System.Globalization;
using System.Net.Mail;

namespace Sistema_Facturacion.Seguridad;

public static class Validaciones
{
    public static bool EmailOpcionalValido(string email)
        => email.Length == 0 || (MailAddress.TryCreate(email, out var direccion)
            && string.Equals(direccion.Address, email, StringComparison.OrdinalIgnoreCase)
            && !email.Any(char.IsWhiteSpace));

    public static bool CantidadValida(string texto, out int cantidad)
        => int.TryParse(texto, NumberStyles.None, CultureInfo.InvariantCulture, out cantidad) && cantidad >= 0;

    public static bool PrecioValido(string texto, out decimal precio)
        => decimal.TryParse(texto, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out precio)
            && precio >= 0 && precio <= 99999999.99m && decimal.Round(precio, 2) == precio;
}
