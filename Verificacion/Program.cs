using Sistema_Facturacion.CapaDatos;
using Sistema_Facturacion.Seguridad;
using Npgsql;

static void Comprobar(bool condicion, string descripcion)
{
    if (!condicion) throw new InvalidOperationException(descripcion);
    Console.WriteLine($"OK: {descripcion}");
}

const string saltInicial = "YjVUZDdtOTNjS3hSU0ExOGVtaUp2NlFhNHpOMHdYZUc=";
const string hashInicial = "e08aaba811a72a4102baff9932469d7724cb489a16be7a231ff641410a25d869";
Comprobar(SeguridadPassword.Verificar("AdminInicio2026!", hashInicial, saltInicial), "Vector del administrador inicial");
var primera = SeguridadPassword.Crear("Clave de prueba áé🔐2026");
var segunda = SeguridadPassword.Crear("Clave de prueba áé🔐2026");
Comprobar(primera.PasswordSalt != segunda.PasswordSalt, "Salt distinto para la misma contraseña");
Comprobar(primera.PasswordHash != segunda.PasswordHash, "Hash distinto con salt distinto");
Comprobar(SeguridadPassword.Verificar("Clave de prueba áé🔐2026", primera.PasswordHash, primera.PasswordSalt), "Contraseña Unicode correcta");
Comprobar(!SeguridadPassword.Verificar("incorrecta", primera.PasswordHash, primera.PasswordSalt), "Rechazo de contraseña incorrecta");
Comprobar(!SeguridadPassword.Verificar("", primera.PasswordHash, primera.PasswordSalt), "Rechazo de contraseña vacía");
Comprobar(!SeguridadPassword.Verificar("clave", "zz", saltInicial), "Rechazo de hash inválido");
Comprobar(!SeguridadPassword.Verificar("clave", hashInicial, "salt inválido"), "Rechazo de salt inválido");
Comprobar(!SeguridadPassword.Verificar("clave", hashInicial, "AA=="), "Rechazo de salt de longitud incorrecta");
bool rechazoCorta = false;
try { SeguridadPassword.Crear("corta"); }
catch (ArgumentException) { rechazoCorta = true; }
Comprobar(rechazoCorta, "Longitud mínima al crear contraseña");
bool rechazoTls = false;
try { using var insegura = new Conexion("Host=localhost;Database=prueba;Username=prueba;SSL Mode=Disable"); }
catch (ArgumentException) { rechazoTls = true; }
Comprobar(rechazoTls, "Rechazo de configuración sin TLS");
using (var valida = new Conexion("Host=example.com;Database=prueba;Username=prueba;SSL Mode=VerifyFull"))
    Comprobar(true, "Creación y disposición del pool sin abrir conexiones");
using (var cifrada = new Conexion("Host=example.com;Database=prueba;Username=prueba;SSL Mode=Require"))
    Comprobar(true, "Compatibilidad con TLS Require de la URI de Aiven");

var uriPrueba = new NpgsqlConnectionStringBuilder(ConfiguracionConexion.ConvertirUri(
    "postgresql://usuario:p%40ss%3Bword%23%25@localhost:15432/base?sslmode=require", AppContext.BaseDirectory));
Comprobar(uriPrueba.Password == "p@ss;word#%" && uriPrueba.Port == 15432 && uriPrueba.SslMode == SslMode.Require,
    "URI con contraseña codificada y puerto personalizado");
bool rechazoUri = false;
try { ConfiguracionConexion.ConvertirUri("postgresql://u:p@localhost/base?sslmode=disable", AppContext.BaseDirectory); }
catch (InvalidOperationException) { rechazoUri = true; }
Comprobar(rechazoUri, "Rechazo de URI sin cifrado");
string temporal = Path.GetTempFileName();
try
{
    File.WriteAllText(temporal, "# comentario\nDATABASE_URL=\"postgresql://u:p@localhost/base?sslmode=require\"\n");
    Comprobar(new NpgsqlConnectionStringBuilder(ConfiguracionConexion.LeerEnv(temporal)).Database == "base", "Lectura .env con comillas y comentarios");
    File.AppendAllText(temporal, "DATABASE_URL=duplicada\n");
    bool rechazoDuplicada = false;
    try { ConfiguracionConexion.LeerEnv(temporal); }
    catch (InvalidOperationException) { rechazoDuplicada = true; }
    Comprobar(rechazoDuplicada, "Rechazo de DATABASE_URL duplicada");
}
finally { File.Delete(temporal); }

if (args.Contains("--conexion"))
{
    await using var conexion = Conexion.DesdeEnv(Path.GetFullPath(".env"));
    Comprobar(await conexion.ProbarAsync(), "Conexión real a PostgreSQL");
    var consultas = new UsuarioConsultas(conexion);
    Comprobar(await consultas.AutenticarAsync("admin", "AdminInicio2026!") is not null, "Autenticación real del administrador inicial");
    Comprobar(await consultas.AutenticarAsync("admin", "ContraseñaIncorrecta") is null, "Rechazo de contraseña incorrecta en PostgreSQL");
    Comprobar(await consultas.AutenticarAsync("admin' OR '1'='1", "ContraseñaIncorrecta") is null, "Rechazo de intento de inyección SQL");
}
Console.WriteLine("Verificación terminada.");
