using Sistema_Facturacion.CapaDatos;
using Sistema_Facturacion.Formularios;

namespace Sistema_Facturacion
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.SetDefaultFont(new Font("Segoe UI", 10F));
            try
            {
                using var conexion = Conexion.DesdeEnv();
                using var contexto = new ContextoAplicacion(conexion);
                Application.Run(contexto);
            }
            catch (Exception ex) when (ex is InvalidOperationException or ArgumentException or IOException or UnauthorizedAccessException)
            {
                MessageBox.Show("No se pudo cargar la configuración de conexión. Revisa DATABASE_URL en .env, el modo TLS y la ruta del certificado si lo usas.",
                    "Configuración de facturación", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
