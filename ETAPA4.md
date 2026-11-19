# Etapa 4 — Productos, Clientes y Usuarios

Los menús Productos, Clientes y Usuarios abren sus formularios CRUD. Nuevo / Registrar limpia el editor; Consultar / Editar / Eliminar abre el mismo catálogo. Si ya está abierto, se activa la ventana existente. Facturas permanece deshabilitado hasta la etapa 5.

## Uso

- **Guardar:** crea un registro nuevo. Los campos obligatorios están marcados con asterisco.
- **Editar:** selecciona primero una fila, modifica sus datos y pulsa Editar.
- **Eliminar:** selecciona una fila y confirma el registro que se eliminará.
- **Limpiar:** abandona la selección y prepara el editor para otro registro.
- **Buscar:** filtra el listado cargado al escribir. Cambiar la búsqueda limpia el editor para evitar editar una fila oculta.
- **Actualizar lista:** consulta de nuevo PostgreSQL para ver cambios realizados desde otros equipos.

Los campos se limpian y el listado se recarga después de guardar, editar o eliminar. Si la escritura terminó pero falló la recarga, el mensaje distingue ambas situaciones.

## Datos y validaciones

Productos usa descripción obligatoria, stock entero no negativo y precio decimal de hasta dos decimales, entre 0 y 99999999.99. Para escribir precios usa punto decimal: `12.50`. Se restringen las teclas numéricas y se valida también el contenido pegado.

Clientes exige nombre. Teléfono, correo y dirección son opcionales según el esquema existente. El correo se valida cuando se proporciona.

Usuarios exige usuario y nombre completo. Los nombres de usuario se guardan recortados y en minúsculas y no pueden repetirse. Al crear un usuario, su contraseña requiere un mínimo de 12 caracteres y confirmación. Al editar, deja ambos campos de contraseña vacíos para conservarla. Una nueva contraseña genera un salt aleatorio y hash mediante la utilidad existente. El listado no recupera ni muestra hashes o salts.

No se permite eliminar al usuario conectado. Si editas tu usuario o nombre completo, se actualiza la barra inferior del MDI. Los clientes o productos asociados a facturas no se pueden eliminar debido a las claves foráneas existentes.

## Archivos

Los controles se construyen completamente en código C# con Windows Forms nativo. No es necesario añadir controles manualmente en el diseñador.

| Archivos dentro de Sistema-Facturacion | Contenido |
| --- | --- |
| `CapaDatos/RepositorioBase.cs` | Conexiones por operación, lectura tipada y ejecución parametrizada |
| `CapaDatos/ProductoRepositorio.cs` | CRUD PostgreSQL de productos |
| `CapaDatos/ClienteRepositorio.cs` | CRUD PostgreSQL de clientes |
| `CapaDatos/UsuarioRepositorio.cs` | CRUD PostgreSQL de usuarios y generación de credenciales |
| `Seguridad/Validaciones.cs` | Validaciones de datos ingresados en los formularios |
| `Formularios/FrmCrud.cs` | Búsqueda, DataGridView, editor, botones, ErrorProvider y flujo asíncrono compartido |
| `Formularios/FrmProductos.cs` | Campos y lógica del catálogo de productos |
| `Formularios/FrmClientes.cs` | Campos y lógica del catálogo de clientes |
| `Formularios/FrmUsuarios.cs` | Campos y lógica del catálogo de usuarios |
| `Formularios/FrmPrincipalMDI.cs` y `.Designer.cs` | Apertura única y opciones de menú habilitadas |

Las consultas usan NpgsqlParameter con tipos explícitos. Durante una operación se deshabilitan las acciones del catálogo para evitar solicitudes simultáneas. No se muestran credenciales de la conexión ni errores técnicos de PostgreSQL al usuario.

## Entrega

No se ejecutaron compilaciones, pruebas ni la aplicación, y no se añadieron pruebas para esta etapa, conforme a tu solicitud. Tampoco se ejecutó SQL contra Aiven. Se conserva tu `.env` y no hace falta cambiar el esquema de la base de datos.

Para usar los cambios, detén la ejecución anterior y compila e inicia el proyecto desde Visual Studio. La verificación queda a tu cargo.
