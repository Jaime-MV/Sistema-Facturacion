using System.Security.Cryptography;
using System.Text;

namespace Sistema_Facturacion.Seguridad;

public sealed record CredencialesPassword(string PasswordHash, string PasswordSalt);

public static class SeguridadPassword
{
    public static CredencialesPassword Crear(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);
        if (password.Length < 12)
            throw new ArgumentException("La contraseña debe tener al menos 12 caracteres.", nameof(password));

        string salt = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        return new CredencialesPassword(CalcularHash(password, salt), salt);
    }

    public static string CalcularHash(string password, string salt)
    {
        ArgumentNullException.ThrowIfNull(password);
        ArgumentException.ThrowIfNullOrWhiteSpace(salt);
        if (Convert.FromBase64String(salt).Length != 32)
            throw new ArgumentException("El salt debe contener 32 bytes en Base64.", nameof(salt));

        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(password + salt));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    public static bool Verificar(string password, string passwordHash, string passwordSalt)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(passwordHash)
            || string.IsNullOrEmpty(passwordSalt))
            return false;

        try
        {
            byte[] esperado = Convert.FromHexString(passwordHash);
            if (esperado.Length != 32)
                return false;
            byte[] recibido = Convert.FromHexString(CalcularHash(password, passwordSalt));
            return CryptographicOperations.FixedTimeEquals(esperado, recibido);
        }
        catch (FormatException) { return false; }
        catch (ArgumentException) { return false; }
    }
}