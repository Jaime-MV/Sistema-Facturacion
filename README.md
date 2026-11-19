# Sistema de facturación — etapas 1, 2, 3 y 4

Implementación de las etapas 1 a 4. La configuración y el inicio de sesión están documentados en [ETAPA3.md](ETAPA3.md); el CRUD y los menús habilitados, en [ETAPA4.md](ETAPA4.md). Se conserva el proyecto Windows Forms existente con `net10.0-windows` y Npgsql 10.0.3. Abre `Sistema-Facturacion.slnx` con el Visual Studio que creó la solución.

## Etapa 1: crear la estructura PostgreSQL

1. Crea una base PostgreSQL o utiliza la base asignada por tu proveedor remoto.
2. En el editor SQL del proveedor o en pgAdmin, conéctate como propietario y ejecuta **todo** `BaseDatos/01_crear_esquema.sql`.
3. El script crea el esquema `facturacion`, las cinco tablas, claves, índices y administrador dentro de una transacción. No necesita pgcrypto. Se ejecuta una sola vez: volver a ejecutarlo falla sin borrar ni reiniciar los datos existentes.
4. Comprueba el resultado:

```sql
SELECT idusuario, nombre, nombrecompleto, fecharegistro
FROM facturacion.usuario;
SELECT table_name FROM information_schema.tables
WHERE table_schema = 'facturacion' ORDER BY table_name;
```

Acceso inicial de la aplicación: **admin / AdminInicio2026!**. Son credenciales de arranque conocidas; cambia la contraseña cuando esté disponible el CRUD de usuarios. La contraseña de PostgreSQL es distinta.

Los nombres SQL son minúsculos sin comillas; se usan siempre con el prefijo `facturacion.`. Los modelos C# utilizan PascalCase. `factura.idcliente` conserva la relación y `factura.cliente` conserva el nombre del cliente en el momento de la venta. Las claves foráneas impiden eliminar clientes y productos referenciados por facturas.

Los importes se almacenan con `NUMERIC(10,2)` y se manejan con `decimal` en C#. Se debe redondear IVA con `decimal.Round(subtotal * 0.13m, 2, MidpointRounding.AwayFromZero)`. Hay una línea por producto en cada factura: agregar otra vez el mismo producto deberá acumular su cantidad. La transacción que inserta factura, detalle y descuenta existencias corresponde a la etapa 5; el SQL de instalación no descuenta stock mediante triggers. La coherencia de la suma de detalles con el encabezado también se implementará en esa transacción.

Las fechas usan `TIMESTAMP` sin zona horaria, como se solicitó. En próximas etapas se usarán valores `DateTimeKind.Unspecified` con una única convención horaria: hora local de El Salvador. El valor por defecto depende de la zona horaria de la sesión PostgreSQL; la cadena de ejemplo fija `America/El_Salvador`.

## Etapa 2: estructura y código

```text
BaseDatos/01_crear_esquema.sql
Sistema-Facturacion/
  CapaDatos/
    Conexion.cs
    UsuarioConsultas.cs
  Modelos/
    Entidades.cs
  Seguridad/
    SeguridadPassword.cs
  Form1.cs y Form1.Designer.cs (formulario original)
  Program.cs (punto de entrada original)
Verificacion/
  Verificacion.csproj
  Program.cs
```

`Conexion` administra un `NpgsqlDataSource` con pool. Se creará una sola instancia durante la vida de la aplicación; cada operación obtiene y dispone su conexión con `await using`. `ProbarAsync` ejecuta `SELECT 1`. `UsuarioConsultas.AutenticarAsync` muestra una consulta completa con `NpgsqlParameter`, recupera salt y hash y devuelve un `Usuario` sin secretos cuando la contraseña coincide; devuelve `null` si no coincide. No construyas consultas concatenando texto ingresado por el usuario. Los nombres de usuario se guardarán recortados y en minúsculas, como ya requiere la consulta y la restricción SQL.

`SeguridadPassword.Crear` genera 32 bytes aleatorios de salt, los convierte a Base64 y calcula SHA-256 sobre los bytes UTF-8 de **contraseña + texto del salt en Base64**. Devuelve hash hexadecimal en minúsculas y salt. `Verificar` usa comparación en tiempo constante y rechaza datos malformados. La contraseña no se recorta ni se convierte a minúsculas. Al actualizarla se debe invocar `Crear` nuevamente; no se reutiliza el salt anterior.

Se implementa SHA-256 por ser un requisito expreso. Para un despliegue de producción conviene migrar a un algoritmo específico de contraseñas como PBKDF2 o Argon2, con versionado del formato: SHA-256 por sí solo es rápido frente a intentos masivos.

### Configuración vigente y ejecución

La aplicación lee `DATABASE_URL` desde `.env`, en formato URI de Aiven. Ya no requiere configurar una variable de entorno de Windows. La compilación copia `.env` junto al ejecutable y F5 abre `FrmLogin`, seguido de `FrmPrincipalMDI` al autenticar.

Consulta [ETAPA3.md](ETAPA3.md) para instrucciones, archivos, alcance de los menús y comprobaciones. La configuración acepta TLS `require`, `verify-ca` y `verify-full`, según la URI; los modos sin cifrado se rechazan. El `.env` real no se incluye en Git.

Documentación de referencia: [uso de Npgsql](https://www.npgsql.org/doc/basic-usage.html), [seguridad y TLS](https://www.npgsql.org/doc/security.html), [parámetros de conexión](https://www.npgsql.org/doc/connection-string-parameters.html).
