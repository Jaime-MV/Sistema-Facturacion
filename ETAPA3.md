# Etapa 3: inicio de sesión y contenedor MDI

## Ejecutar desde Visual Studio

1. Abre `Sistema-Facturacion.slnx` y selecciona `Sistema-Facturacion` como proyecto de inicio.
2. Conserva `.env` en la raíz de la solución, con `DATABASE_URL` y tu URI de Aiven. No requiere variables de entorno de Windows. El proyecto copia ese archivo junto al ejecutable al compilar; después de cambiarlo vuelve a compilar.
3. Detén cualquier ejecución anterior de `Form1` y pulsa F5. Ahora debe aparecer **Iniciar sesión — Facturación**.
4. Ingresa `admin` y `AdminInicio2026!`, salvo que hayas cambiado ese usuario en PostgreSQL.
5. Al autenticarte aparecerá **Sistema de facturación**, maximizado, con los menús y la barra inferior de usuario y reloj.

El formulario original `Form1` permanece en el proyecto, pero ya no es el punto de inicio.

## Archivos completos

- `Sistema-Facturacion/Program.cs`: carga configuración y mantiene una sola conexión lógica con pool durante la vida de la aplicación.
- `Sistema-Facturacion/CapaDatos/ConfiguracionConexion.cs`: lee `.env` y convierte la URI a una cadena de Npgsql mediante su constructor de parámetros, sin concatenar credenciales. Reconoce `postgres://` y `postgresql://`, contraseña codificada, puerto, `sslmode`, `sslrootcert`, `connect_timeout`, `application_name` y `options`. Las rutas relativas de certificados se resuelven respecto del archivo `.env`.
- `Sistema-Facturacion/Formularios/FrmLogin.cs` y `.Designer.cs`: controles nativos, contraseña oculta, opción para mostrarla, Enter para ingresar, Escape para salir, tabulación, validación con ErrorProvider y autenticación asíncrona que mantiene la ventana responsive. Impide intentos simultáneos y cancela la consulta al cerrar.
- `Sistema-Facturacion/Formularios/ContextoAplicacion.cs`: coordina login, apertura del MDI y regreso al login sin salir del proceso al cerrar sesión. Limpia los datos de acceso al volver y dispone los formularios al finalizar.
- `Sistema-Facturacion/Formularios/FrmPrincipalMDI.cs` y `.Designer.cs`: contenedor MDI, menús, sesión, reloj de El Salvador y apertura única de hijos por tipo.

La autenticación usa `UsuarioConsultas`: consulta parametrizada a `facturacion.usuario`, SHA-256 de contraseña + salt y comparación en tiempo constante. Un usuario inexistente y una contraseña errónea muestran el mismo mensaje. Los mensajes de error no incluyen la URI ni la contraseña de PostgreSQL.

## Menús y alcance

Se muestran Productos, Clientes, Facturas, Usuarios y Sistema / Sesión con los submenús solicitados. En la etapa 3 las operaciones CRUD y de facturación estaban deshabilitadas. La [etapa 4](ETAPA4.md) habilita Productos, Clientes y Usuarios; Facturas permanece pendiente de la etapa 5.

**Cerrar sesión** pide confirmación, cierra el MDI con todos sus hijos y devuelve al login limpio. **Salir** y la X del MDI terminan la aplicación.

Para las etapas siguientes, todos los módulos deben abrirse con el método existente:

```csharp
public T AbrirFormulario<T>(Func<T> crearFormulario) where T : Form
```

Si ya existe una ventana de ese tipo, se activa y restaura si está minimizada. Si fue cerrada, se crea otra instancia al volver a solicitarla. No se usa una instancia estática descartada.

## Configuración y distribución

Se respeta el `sslmode` de tu URI de Aiven. `require` cifra el tráfico; `verify-ca` valida además la CA y `verify-full` valida también el nombre del servidor. Los modos sin TLS se rechazan. No se modificó tu `.env`. La verificación anterior confirmó tu conexión con `require`.

El archivo `.env` está excluido en `.gitignore`. Se copia al directorio de compilación para F5, pero no al publicar: para distribuir debes colocar un `.env` configurado junto al ejecutable y el certificado si corresponde. La configuración en un escritorio es accesible al usuario del equipo; esta etapa no implementa un servidor intermedio.

## Verificación

```powershell
dotnet build .\Sistema-Facturacion.slnx --no-restore
dotnet run --project .\Verificacion\Verificacion.csproj
dotnet run --project .\Verificacion\Verificacion.csproj -- --conexion
```

El verificador incluye pruebas de hash/salt, lectura de `.env`, contraseña URI con caracteres especiales, rechazo de configuración sin TLS y entradas duplicadas. Con `--conexion` prueba además PostgreSQL, la contraseña inicial del administrador, una contraseña incorrecta y un intento de inyección SQL. Si cambiaste la contraseña del administrador, actualiza ese dato de prueba antes de ejecutarlo.

En este equipo Windows bloqueó la ejecución del verificador con su política de control de aplicaciones (0x800711C7). Por ello no se declaran superadas las pruebas en ejecución ni la revisión visual. No se modificaron políticas de Windows.

Prueba manual al iniciar con F5: campos vacíos; usuario o contraseña incorrectos; acceso correcto; usuario y reloj en el MDI; cancelar y confirmar cierre de sesión; nuevo acceso y salida. Los formularios hijos se comprobarán con los módulos de las etapas 4 y 5.
