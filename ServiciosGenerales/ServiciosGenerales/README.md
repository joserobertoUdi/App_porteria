# ============================================================
#  ServiciosGenerales API — Control de Acceso y Parqueo UDI
# ============================================================

API REST en .NET 8 con Clean Architecture, JWT, EF Core y SQL Server.
Complementa la app Flutter `app_universidad` del mismo repositorio.

## Estructura del proyecto (Clean Architecture)

| Proyecto                 | Responsabilidad                                                        |
|--------------------------|------------------------------------------------------------------------|
| `ServiciosGenerales.Api` | Presentación: controladores, middleware, Swagger, composición raíz    |
| `ServiciosGenerales.Aplicacion` | Casos de uso, DTOs, servicio de tokens                         |
| `ServiciosGenerales.Dominio`    | Entidades y contratos (interfaces de repositorio)              |
| `ServiciosGenerales.Infraestructura` | EF Core (`DbContext`), repositorios, migraciones             |

Regla de dependencias: `Api → Aplicacion`, `Api → Infraestructura`,
`Aplicacion → Dominio`, `Infraestructura → Dominio`. Nunca en sentido inverso.

## Requisitos

- .NET SDK 8.0+
- SQL Server LocalDB (por defecto) o SQL Server completo

## Configuración y variables de entorno

La API lee variables de entorno con respaldo en `appsettings.json`
/ `appsettings.{Environment}.json`.

| Variable | Obligatoria | Propósito |
|----------|:-----------:|-----------|
| `DB_CONNECTION_STRING` | No* | Cadena de conexión a SQL Server |
| `JWT_KEY` | Sí (prod) | Clave secreta para firmar tokens (mín. 32 caracteres) |
| `JWT_ISSUER` | No | Emisor del token (por defecto `UDI`) |
| `JWT_AUDIENCE` | No | Audiencia del token (por defecto `app-universidad`) |
| `JWT_EXPIRE_MINUTES` | No | Minutos de validez del token (por defecto `120`) |
| `SEED_ADMIN_PASSWORD` | No | Crea el usuario administrador al arrancar |
| `SEED_USUARIOS_DEMO` | No | Crea porteros de demostración (`true`/`false`) |
| `SHAREPOINT_BASE_URL` | No | URL base de SharePoint para fotos (fase futura) |

\* Si no existe, usa `ConnectionStrings:CadenaConexionSQL` de `appsettings.json`.
**Nunca** se debe exponer la `JWT_KEY` real en `appsettings.json`: se define
por variable de entorno (`JWT_KEY`) o en `appsettings.Development.json`.

Existe un `.env.example` como referencia. En Windows puede exportar variables
con `$env:JWT_KEY="..."` o `setx JWT_KEY "..."`.

## Puesta en marcha

```powershell
cd ServiciosGenerales
$env:JWT_KEY="clave-segura-de-al-menos-32-caracteres"
dotnet restore
dotnet ef database update --project ServiciosGenerales.Infraestructura --startup-project ServiciosGenerales.Api
dotnet run --project ServiciosGenerales.Api
```

Al arrancar, la API:
1. Aplica las migraciones de EF Core automáticamente.
2. Ejecuta el seed inicial (roles/tipos de usuario y, si se configura, usuarios).

Swagger disponible en `http://localhost:5169/swagger` (solo en Development),
con autenticación JWT (botón `Authorize`).

## Endpoints principales

### Auth (público)
- `POST /api/Auth/login` → `{ documentoIdentidad, password }` → `{ token, usuario }`

### Usuarios (requiere JWT)
- `GET  /api/Usuario/buscar/{dni}` — busca un usuario por documento
- `GET  /api/Usuario/sugerir/{termino}` — autocompletar usuarios

### Portería (JWT + rol `Administrador` o `PorteroPorteria`)
- `POST /api/Porteria/entrada` — registra entrada con `usuarioId`
- `POST /api/Porteria/entrada-completa` — registra entrada buscando o creando el usuario
- `POST /api/Porteria/salida` — registra salida
- `GET  /api/Porteria/activos` — visitas con salida pendiente

### Parqueo (JWT + rol `Administrador` o `PorteroParqueo`)
- `POST /api/Parqueo/entrada-completa` — busca/crea usuario y vehículo, registra entrada
- `POST /api/Parqueo/entrada` — registra entrada con `vehiculoId`
- `POST /api/Parqueo/salida` — registra salida
- `GET  /api/Parqueo/activos` — parqueos activos
- `GET  /api/Parqueo/historial/{usuarioId}` — historial por usuario
- `GET  /api/Parqueo/vehiculos/buscar-placa/{placa}` — busca vehículo por matrícula
- `GET  /api/Parqueo/vehiculos/{usuarioId}` — vehículos de un usuario

### Admin (JWT + rol `Administrador`)
- `GET  /api/Admin/usuarios` — lista los usuarios del sistema
- `POST /api/Admin/usuarios` — crea un usuario con credenciales (`{ nombreCompleto, documentoIdentidad, password, tipoUsuarioId, rolId }`)
- `PUT  /api/Admin/usuarios/{id}` — actualiza nombre, tipo, rol y estado (`{ nombreCompleto, tipoUsuarioId, rolId, estado }`); `404` si el usuario no existe
- `POST /api/Admin/usuarios/{id}/password` — restablece la contraseña y desbloquea la cuenta (`{ nuevaPassword }`); `404` si el usuario no existe
- `GET  /api/Admin/usuarios/{id}/historial` — historial de visitas del usuario (portería y parqueo); `404` si el usuario no existe
- `GET  /api/Admin/porteria/historial` — historial completo de la portería
- `GET  /api/Admin/parqueo/historial` — historial completo del parqueo
- `GET  /api/Admin/estadisticas/tipos?anio=&mes=` — estadísticas del mes por tipo de persona (Estudiante/Trabajador/Visitante): ingresos y salidas de portería y parqueo; `anio`/`mes` opcionales (por defecto, la fecha actual)
- `GET  /api/Admin/estadisticas/tipos/{tipoUsuarioId}/historial?anio=&mes=` — detalle del historial (portería y parqueo) de un tipo de persona en el mes; `404` si el tipo no existe

## Seguridad y control de peticiones

- **Rate limiting** (`System.Threading.RateLimiting`):
  - `login`: 20 peticiones/minuto por cliente → `429 Too Many Requests`.
  - `global`: 300 peticiones/minuto por cliente en el resto de endpoints.
- **CORS**: política `PermitirApp` para los orígenes del frontend web
  (`http://localhost:5169`, `http://localhost:8388`, `http://localhost:4200`).
  La app móvil (nativa) no requiere CORS.
- **JWT**: firmado con `HMAC-SHA256`; claims de rol mapeados a `[Authorize(Roles=...)]`.
- **Usuarios creados en flujos de entrada** (`entrada-completa`) se crean
  **sin contraseña** (`PasswordHash = null`) y con **rol Visitante**: no pueden
  iniciar sesión en la aplicación. Solo un administrador crea cuentas con credenciales.
- **Manejo de errores**: middleware global; nunca se exponen detalles internos
  (`500` genérico). Las excepciones de negocio devuelven `400` con mensaje.

## Base de datos (normalizada)

```
Roles (1) ──< Usuarios (1) ──< Vehiculos (1) ──< RegistroParqueo
               │   │
               │   └──────< RegistrosPorteria
               └───< TiposUsuario
```

- 6 tablas, relaciones 1:N, sin columnas derivadas.
- `DocumentoIdentidad` y `Vehiculos.Matricula` con índice `UNIQUE`.
- Índices filtrantes sobre `FechaSalida IS NULL` para consultas de registros activos.
- Migraciones con `dotnet ef migrations add <Nombre>`.

## Pruebas de la API con `.http`

El archivo `ServiciosGenerales.Api.http` (extensión *REST Client* de VS Code)
contiene ejemplos listos para ejecutar.

## Documentos relacionados

- `ROADMAP.md` (raíz del repo): plan general del sistema.
