# Documentación del Backend — API de Control de Acceso y Parqueo UDI

API REST en **.NET 8** con **Clean Architecture**, **EF Core + SQL Server**, **JWT + refresh tokens**, **BCrypt**, **rate limiting** y un **reloj de servidor** (`IServerClock`) como fuente única de hora para el negocio.

- Solución: `ServiciosGenerales\ServiciosGenerales\ServiciosGenerales.sln`
- Proyecto de arranque: `ServiciosGenerales.Api`
- Ruta actual en ejecución: `http://100.106.35.85:5169` (perfil `http` del `launchSettings.json`)
- Complementa la app Flutter `app_universidad` (ver `DOCUMENTACION_FRONTEND.md`).

---

## 1. Stack tecnológico

| Componente | Tecnología |
|------------|-----------|
| Plataforma | .NET 8 (C# 12, nullable + implicit usings) |
| Arquitectura | Clean Architecture (Api / Aplicacion / Dominio / Infraestructura) |
| ORM | EF Core 8.0.11 (`Microsoft.EntityFrameworkCore.SqlServer`) |
| Base de datos | SQL Server (LocalDB por defecto en dev) |
| Autenticación | `Microsoft.AspNetCore.Authentication.JwtBearer` 8.0.11 |
| Hash de contraseñas | `BCrypt.Net-Next` 4.0.3 |
| Documentación API | Swashbuckle (Swagger UI), solo en Development |
| Pruebas | xUnit |

---

## 2. Arquitectura (Clean Architecture)

| Proyecto | Responsabilidad |
|----------|-----------------|
| `ServiciosGenerales.Api` | Presentación: controladores, middleware, Swagger, composición raíz (DI, CORS, rate limiting) |
| `ServiciosGenerales.Aplicacion` | Casos de uso (orquestan la lógica de negocio), DTOs, servicios de tokens y reloj |
| `ServiciosGenerales.Dominio` | Entidades puras y contratos (interfaces de repositorio, enums, utilidades) |
| `ServiciosGenerales.Infraestructura` | EF Core (`UniversidadDbContext`), repositorios, migraciones |
| `ServiciosGenerales.Tests` | Pruebas unitarias (xUnit) |

**Regla de dependencias** (nunca en sentido inverso):

```
Api → Aplicacion, Api → Infraestructura
Aplicacion → Dominio
Infraestructura → Dominio
```

**Flujo de una petición:**

```
Controlador (Api)
   └─> UseCase (Aplicacion) ── inyecta IServerClock / ITokenService / settings
         └─> Interfaz de repositorio (Dominio)
               └─> Repositorio EF Core (Infraestructura)
                     └─> UniversidadDbContext → SQL Server
```

**Inyección de dependencias:**
- `Program.cs` registra los `Settings` (Jwt, Auth) como `AddSingleton`.
- `AddInfraestructura(connectionString)` (`Infraestructura\DependencyInjection.cs`) registra `DbContext` + los 5 repositorios como `AddScoped`.
- `AddAplicacion()` (`Aplicacion\DependencyInjection.cs`) registra `IServerClock` como `AddSingleton` y todos los casos de uso como `AddScoped`.

---

## 3. Estructura de carpetas

```
ServiciosGenerales\
└── ServiciosGenerales\
    ├── ServiciosGenerales.sln
    ├── .env.example                      # Referencia de variables de entorno
    ├── ServiciosGenerales.Api\
    │   ├── Program.cs                    # Composición raíz
    │   ├── Controllers\
    │   │   ├── AuthController.cs
    │   │   ├── UsuarioController.cs
    │   │   ├── PorteriaController.cs
    │   │   ├── ParqueoController.cs
    │   │   ├── AdminController.cs
    │   │   └── SistemaController.cs
    │   ├── Data\DatabaseSeeder.cs        # Seed inicial
    │   ├── Middleware\ExceptionHandlingMiddleware.cs
    │   ├── Properties\launchSettings.json
    │   ├── appsettings.json
    │   ├── appsettings.Development.json
    │   └── ServiciosGenerales.Api.http   # Ejemplos REST Client (VS Code)
    ├── ServiciosGenerales.Aplicacion\
    │   ├── DependencyInjection.cs
    │   ├── Dtos\ (Auth, Usuarios, Porteria, Parqueo, Admin, Sistema)
    │   ├── Services\ (TokenService, RefreshTokenService, RefreshTokenFactory, IServerClock, OffsetUtcUtil)
    │   ├── Settings\ (JwtSettings, AuthSettings)
    │   └── UseCases\ (Auth, Usuarios, Porteria, Parqueo, Admin, Sistema)
    ├── ServiciosGenerales.Dominio\
    │   ├── Entidades\ (Usuario, Rol, TiposUsuario, Vehiculo, RegistroParqueo, RegistroPorteria, RefreshToken)
    │   ├── Interfaces\ (IUsuarioRepository, IVehiculoRepository, IRegistroPorteriaRepository,
    │   │                IRegistroParqueoRepository, IRefreshTokenRepository)
    │   └── Utils\TiposUsuarioEnum.cs
    ├── ServiciosGenerales.Infraestructura\
    │   ├── DependencyInjection.cs
    │   ├── Data\UniversidadDbContext.cs
    │   ├── Migrations\
    │   └── Repositories\
    └── ServiciosGenerales.Tests\
```

---

## 4. Configuración y variables de entorno

La API lee **variables de entorno** con respaldo en `appsettings.json` / `appsettings.Development.json` (`Program.cs` líneas 16–61).

| Variable | Obligatoria | Propósito | Valor por defecto |
|----------|:-----------:|-----------|-------------------|
| `DB_CONNECTION_STRING` | No* | Cadena de conexión a SQL Server | `ConnectionStrings:CadenaConexionSQL` de `appsettings.json` |
| `JWT_KEY` | **Sí (prod)** | Clave secreta para firmar tokens (mín. 32 caracteres) | `Jwt:Key` (solo en `appsettings.Development.json`) |
| `JWT_ISSUER` | No | Emisor del token | `UDI` |
| `JWT_AUDIENCE` | No | Audiencia del token | `app-universidad` |
| `JWT_EXPIRE_MINUTES` | No | Minutos de validez del access token | `180` |
| `JWT_REFRESH_TOKEN_DAYS` | No | Días de validez del refresh token | `7` |
| `AUTH_MAX_INTENTOS_FALLIDOS` | No | Máximo de intentos de login fallidos | `5` |
| `AUTH_DURACION_BLOQUEO_MIN` | No | Minutos de bloqueo por intentos fallidos | `15` |
| `SEED_ADMIN_PASSWORD` | No | Crea el administrador al arrancar | — |
| `SEED_ADMIN_NOMBRE` / `SEED_ADMIN_DOCUMENTO` / `SEED_ADMIN_TIPO_USUARIO_ID` / `SEED_ADMIN_ROL_ID` | No | Datos del admin de seed | `Administrador` / `admin` / `1` / `1` |
| `SEED_USUARIOS_DEMO` | No | Crea porteros de demostración (`true`/`false`) | `false` |
| `SEED_PORTERO_PARQUEO_PASSWORD` / `SEED_PORTERO_PORTERIA_PASSWORD` | No | Contraseñas de los porteros demo | `portero123` |
| `SHAREPOINT_BASE_URL` | No | URL base de SharePoint para fotos (fase futura) | — |

\* Si no existe, usa la conexión de `appsettings.json`.

> **Importante:** nunca exponer la `JWT_KEY` real en `appsettings.json`; definirla por variable de entorno (`JWT_KEY`) o en `appsettings.Development.json`. En Windows: `$env:JWT_KEY="..."` o `setx JWT_KEY "..."`.

**Configuración actual relevante:**
- `appsettings.json`: conexión LocalDB `Server=(localdb)\MSSQLLocalDB;Database=UniversidadDB;Trusted_Connection=True;TrustServerCertificate=True`; `Jwt.Issuer=UDI`, `Jwt.Audience=app-universidad`, `Jwt.ExpireMinutes=180`, `Jwt.RefreshTokenExpireDays=7`; `Auth.MaxIntentosFallidos=5`, `Auth.DuracionBloqueoMinutos=15`.
- `appsettings.Development.json`: `Jwt.Key=ClaveDeDesarrolloUDI-2026-NoUsarEnProduccion-12345`; `SEED_USUARIOS_DEMO=true`; `SEED_ADMIN_PASSWORD=admin123`; contraseñas de porteros demo `portero123`.

---

## 5. Autenticación y seguridad

### 5.1 Login con bloqueo por intentos fallidos

`LoginUseCase` (`Aplicacion\UseCases\Auth\LoginUseCase.cs`) implementa:

1. Si el usuario no existe **o no tiene `PasswordHash`** (usuarios creados por flujos de entrada): respuesta genérica `401`, **no** se cuentan intentos.
2. Si `BloqueoHasta > ahora` (servidor): `403` con `bloqueadoHasta` (hora hasta la que está bloqueado).
3. Si el usuario está deshabilitado (`Estado = false`): `401` sin acumular intentos.
4. Si la contraseña no verifica con BCrypt: incrementa `IntentosFallidos`; al alcanzar `MaxIntentosFallidos` setea `BloqueoHasta = Now + DuracionBloqueoMinutos`, reinicia el contador y devuelve `403`; si no, devuelve `401` con `intentosRestantes`.
5. Éxito: reinicia contadores, crea un refresh token y devuelve `LoginResponseDto`.

### 5.2 JWT

`TokenService` (`Aplicacion\Services\TokenService.cs`):
- Claims: `NameIdentifier` = documento, `Name` = nombre, `Role` = `RolNombre`.
- Firmado con **HMAC-SHA256** (`SymmetricSecurityKey`).
- `expires = DateTime.UtcNow.AddMinutes(ExpireMinutes)`.
- Validación en `Program.cs`: issuer, audience, lifetime y signing key.

### 5.3 Refresh tokens (con rotación)

- `RefreshTokenService`: genera tokens opacos de 64 bytes aleatorios (Base64) y almacena solo el **hash SHA-256**.
- `RefreshTokenFactory`: persiste el hash con expiración `UtcNow + RefreshTokenExpireDays` y devuelve el token en claro (una sola vez).
- `RenovarTokenUseCase`: valida el refresh token (no revocado ni expirado), verifica que el usuario exista y esté activo, **rota** el token (el usado queda revocado con `ReemplazadoPor = hash del nuevo`) y devuelve un nuevo `LoginResponseDto`.
- `RefreshTokenRepository` también expone `RevocarTodosPorUsuarioAsync` (logout en todos los dispositivos).

### 5.4 Rate limiting (`System.Threading.RateLimiting`)

| Política | Límite | Rechazo |
|----------|--------|---------|
| `login` | 20 peticiones/minuto | `429 Too Many Requests` |
| `global` | 300 peticiones/minuto | `429 Too Many Requests` |

- `QueueLimit = 0` (sin cola). Aplicado a todos los endpoints; `login` y `refresh` usan la política `login`, el resto `global`.

### 5.5 CORS

Política `PermitirApp` con `AllowAnyHeader`, `AllowAnyMethod`, `WithOrigins("http://localhost:5169", "http://localhost:8388")` y `AllowCredentials()`.

### 5.6 Manejo de errores

`ExceptionHandlingMiddleware`:
- `InvalidOperationException` → `400` con `{ error = mensaje }` (reglas de negocio, p. ej. documento duplicado).
- Cualquier otra excepción → `500` genérico `{ error = "Ocurrió un error interno en el servidor." }` (no se exponen detalles internos).

**Orden del pipeline:** Exception → (Swagger en dev) → HttpsRedirection → CORS → RateLimiter → Authentication → Authorization → `MapControllers()`.

La validación de modelo (`ModelState`) devuelve `400` con `{ error = "<mensajes de validación unidos>" }` (configuración en `Program.cs`, `InvalidModelStateResponseFactory`).

---

## 6. Roles y tipos de usuario

### Roles de sistema (`Roles` — controlan el acceso a la app)

| Id | Nombre | Acceso |
|----|--------|--------|
| 1 | `Administrador` | Todo (`AdminController`, portería, parqueo) |
| 2 | `PorteroParqueo` | Módulo de parqueo |
| 3 | `PorteroPorteria` | Módulo de portería |

### Tipos de usuario (`TiposUsuario` — identidad de la persona)

| Id | Nombre |
|----|--------|
| 1 | `Estudiante` |
| 2 | `Trabajador` |
| 3 | `Visitante` |

> **Distinción clave:** el **tipo de usuario** describe quién es la persona (estudiante/trabajador/visitante); el **rol de sistema** habilita el acceso a la app. Los usuarios creados por los flujos de entrada (`entrada-completa`) tienen `PasswordHash = null` y `RolId = null`: **no pueden iniciar sesión**. Solo el administrador crea cuentas con credenciales y rol.

---

## 7. Modelo de datos

Base: `UniversidadDB`. 6 tablas + tabla de refresh tokens (7). Relaciones 1:N. Índices únicos en `Usuarios.DocumentoIdentidad` y `Vehiculos.Matricula`. Índices filtrantes `FechaSalida IS NULL` en `RegistrosPorteria` y `RegistrosParqueo` (consultas de activos).

```
Roles (1) ──< Usuarios (1) ──< Vehiculos (1) ──< RegistroParqueo
                │   │
                │   └──────< RegistrosPorteria
                └───< TiposUsuario

Usuarios (1) ──< RefreshTokens
```

### Entidades (`Dominio\Entidades`)

**Usuario** (`Usuarios`)
| Campo | Tipo | Notas |
|-------|------|-------|
| Id | int | PK |
| NombreCompleto | string(200) | requerido |
| DocumentoIdentidad | string(20) | requerido, **UNIQUE** |
| TipoUsuarioId | int | FK → TiposUsuario |
| FotoUrl | string(500)? | |
| PasswordHash | string(500)? | `null` = sin credenciales |
| RolId | int? | FK → Roles (nullable, restrict) |
| FechaRegistro | datetime | default `GETDATE()` |
| Estado | bool | default `true` |
| IntentosFallidos | int | default 0 |
| BloqueoHasta | datetime? | |

**Rol** (`Roles`): Id, Nombre.
**TiposUsuario** (`TiposUsuario`): Id, Nombre.
**Vehiculo** (`Vehiculos`): Id, UsuarioId (FK), Matricula (string 20, **UNIQUE**), Marca(50)?, Modelo(50)?, Color(30)?, Activo (default true), FechaRegistro (default `GETDATE()`).
**RegistroParqueo** (`RegistrosParqueo`): Id, VehiculoId (FK), FechaIngreso (default `GETDATE()`), FechaSalida (nullable), PuertaAcceso (string 50, requerido), Observaciones (255)?.
**RegistroPorteria** (`RegistrosPorteria`): Id, UsuarioId (FK), FechaEntrada (default `GETDATE()`), FechaSalida (nullable), PuertaEntrada (50)?, PuertaSalida (50)?, MotivoVisita (500)?, AreaDestino (200)?.
**RefreshToken** (`RefreshTokens`): Id, UsuarioId (FK, cascade), TokenHash (64, **UNIQUE**), FechaExpiracion, FechaCreacion (default `SYSUTCDATETIME()`), FechaRevocacion (nullable), ReemplazadoPor (64)?. `EsValido` = `FechaRevocacion == null && FechaExpiracion > DateTime.UtcNow`.

> Las fechas de negocio se asignan con `IServerClock` (reloj del servidor), nunca con la hora del cliente. `FechaCreacion` de `RefreshToken` y defaults de BD usan `SYSUTCDATETIME()`/`GETDATE()` según corresponda.

### Migraciones (7)

| Migración | Contenido |
|-----------|-----------|
| `20260728124319_InitialCreate` | Esquema inicial (Usuarios, TiposUsuario, Vehiculos, Registros, etc.) |
| `20260728125935_AddPasswordHash` | Columna `PasswordHash` |
| `20260728135317_AddRolesTable` | Tabla `Roles` + seed |
| `20260731161706_AddVisitanteRoleYIndicesActivos` | Rol `Visitante` + índices filtrantes de activos |
| `20260731181450_MakeRolIdNullableSinRolVisitante` | `RolId` nullable; se elimina el rol `Visitante` de sistema |
| `20260803130456_AddRefreshTokens` | Tabla `RefreshTokens` |
| `20260803132324_AddBloqueoIntentosFallidos` | `IntentosFallidos` + `BloqueoHasta` |

---

## 8. Referencia de endpoints

**Convenciones comunes:**
- Respuestas correctas: `200 OK` con el JSON indicado (no se usa `201`).
- Errores: `400` (`{ error }` de validación o negocio), `401` (no autenticado), `403` (sin rol / cuenta bloqueada), `404` (`{ mensaje }`), `429` (rate limit).
- Formato de fechas: ISO 8601 (JSON de ASP.NET Core).
- Todos los endpoints protegidos requieren `Authorization: Bearer <token>` salvo los marcados como públicos.

### 8.1 Auth — `api/Auth` (público)

#### POST `/api/Auth/login` — Iniciar sesión
- Acceso: **público** · Rate limit `login`.
- Body:
  ```json
  {
    "documentoIdentidad": "admin",
    "password": "admin123"
  }
  ```
- **200** — `LoginResponseDto`:
  ```json
  {
    "token": "<jwt>",
    "refreshToken": "<opaco>",
    "expiraEnMinutos": 180,
    "nombreCompleto": "Administrador",
    "documentoIdentidad": "admin",
    "tipoUsuarioId": 1,
    "rolId": 1,
    "rolNombre": "Administrador",
    "fotoUrl": null
  }
  ```
- **401** — credenciales inválidas: `{ "mensaje": "Credenciales inválidas. Quedan N intentos.", "intentosRestantes": N }`.
- **403** — cuenta bloqueada: `{ "mensaje": "Demasiados intentos fallidos. Cuenta bloqueada temporalmente. Puede intentar nuevamente a las HH:mm.", "bloqueadoHasta": "<fecha>" }`.

#### POST `/api/Auth/refresh` — Renovar sesión
- Acceso: **público** · Rate limit `login`.
- Body: `{ "refreshToken": "<opaco>" }`.
- **200** — `LoginResponseDto` con token nuevo y refresh token rotado (el anterior queda revocado).
- **401** — `{ "mensaje": "Sesión expirada. Inicie sesión nuevamente." }`.

### 8.2 Usuario — `api/Usuario` (requiere JWT, cualquier rol)

#### GET `/api/Usuario/buscar/{dni}` — Buscar por documento
- **200** — `UsuarioDto`: `{ id, nombreCompleto, documentoIdentidad, tipoUsuarioId, fotoUrl }`.
- **404** — `{ "mensaje": "Usuario no encontrado" }`.

#### GET `/api/Usuario/sugerir/{termino}` — Autocompletar
- **200** — lista de `UsuarioDto` (hasta 5 coincidencias por documento inicial o nombre que contenga el término; solo usuarios `Estado = true`).

### 8.3 Portería — `api/Porteria` (rol `Administrador` o `PorteroPorteria`)

#### POST `/api/Porteria/entrada` — Entrada con usuario existente
- Body: `{ "usuarioId": 1, "puertaEntrada": "Principal", "motivoVisita": "...", "areaDestino": "..." }`.
- **200** — `{ "id": <nuevoId>, "mensaje": "Entrada registrada exitosamente" }`.

#### POST `/api/Porteria/entrada-completa` — Entrada buscando/creando usuario
- Body:
  ```json
  {
    "documentoIdentidad": "123456789",
    "nombreCompleto": "María Pérez",
    "tipoUsuarioId": 3,
    "puertaEntrada": "Principal",
    "motivoVisita": "Trámites",
    "areaDestino": "Rectoría"
  }
  ```
  (`tipoUsuarioId` 1–3, default `3`; si no existe, crea el usuario **sin credenciales ni rol**).
- **200** — `{ "id": <registroId>, "usuarioId": N, "usuarioCreado": true|false, "mensaje": "Entrada registrada exitosamente" }`.

#### POST `/api/Porteria/salida` — Registrar salida
- Body: `{ "id": <registroId>, "puertaSalida": "Principal" }`. La fecha la fija el servidor (`IServerClock`).
- **200** — `{ "mensaje": "Salida registrada exitosamente" }`.
- **404** — `{ "mensaje": "No se encontró el registro o no se pudo actualizar" }`.

#### GET `/api/Porteria/activos` — Visitas activas (sin salida)
- **200** — lista de `VisitaActivaDto`: `{ idRegistro, usuarioId, nombreCompleto, documentoIdentidad, fechaEntrada, motivoVisita, puertaEntrada }`.

### 8.4 Parqueo — `api/Parqueo` (rol `Administrador` o `PorteroParqueo`)

#### GET `/api/Parqueo/vehiculos/buscar-placa/{placa}` — Buscar vehículo
- **200** — `VehiculoDto`: `{ id, usuarioId, matricula, marca, modelo, color, activo, fechaRegistro }`.
- **404** — `{ "mensaje": "Vehículo no encontrado" }`.

#### GET `/api/Parqueo/vehiculos/{usuarioId}` — Vehículos de un usuario
- **200** — lista de `VehiculoDto`.

#### POST `/api/Parqueo/entrada-completa` — Entrada completa (usuario + vehículo)
- Body:
  ```json
  {
    "documentoIdentidad": "123456789",
    "nombreCompleto": "María Pérez",
    "tipoUsuarioId": "3",
    "matricula": "ABC123",
    "marca": "Toyota",
    "modelo": "Corolla",
    "color": "Rojo",
    "puertaAcceso": "Principal",
    "observaciones": null
  }
  ```
  Orquesta: busca/crea usuario (sin credenciales), busca/crea vehículo (matrícula normalizada a mayúsculas) y registra la entrada.
- **200** — `{ "id": <registroId>, "usuarioId": N, "vehiculoId": N, "usuarioCreado": bool, "vehiculoCreado": bool, "mensaje": "Entrada de parqueo registrada" }`.

#### POST `/api/Parqueo/entrada` — Entrada con vehículo existente
- Body: `{ "vehiculoId": 1, "puertaAcceso": "Principal", "observaciones": null }`.
- **200** — `{ "id": <nuevoId>, "mensaje": "Entrada de parqueo registrada" }`.

#### POST `/api/Parqueo/salida` — Registrar salida
- Body: `{ "id": <registroId> }`. Fecha fijada por el servidor.
- **200** — `{ "mensaje": "Salida de parqueo registrada" }`.
- **404** — `{ "mensaje": "Registro no encontrado" }`.

#### GET `/api/Parqueo/activos` — Parqueos activos
- **200** — lista de `ParqueoActivoDto`: `{ idRegistro, matricula, marca, modelo, color, nombreCompleto, documentoIdentidad, fechaIngreso, fechaSalida, puertaAcceso, observaciones }`.

#### GET `/api/Parqueo/historial/{usuarioId}` — Historial por usuario
- **200** — lista de registros de parqueo del usuario (a través de sus vehículos).

### 8.5 Admin — `api/Admin` (rol `Administrador`)

#### GET `/api/Admin/usuarios` — Listar usuarios
- **200** — lista de `UsuarioListadoDto`: `{ id, nombreCompleto, documentoIdentidad, tipoUsuario, rol, estado, fechaRegistro }`.

#### POST `/api/Admin/usuarios` — Crear usuario con credenciales
- Body:
  ```json
  {
    "nombreCompleto": "Juan Pérez",
    "documentoIdentidad": "12345678",
    "tipoUsuarioId": 1,
    "password": "min6chars",
    "rolId": 3
  }
  ```
  (`rolId` 1–3; si se envía `0` se asigna `3`). El password se almacena con BCrypt.
- **200** — `{ "id": N, "mensaje": "Usuario creado" }`.
- **400** — `{ "error": "Ya existe un usuario con ese documento de identidad." }`.

#### GET `/api/Admin/porteria/historial` — Historial de portería
- **200** — lista de `HistorialPorteriaDto`: `{ idRegistro, nombreCompleto, documentoIdentidad, fechaEntrada, fechaSalida, puertaEntrada, puertaSalida, motivoVisita, areaDestino }`.

#### GET `/api/Admin/parqueo/historial` — Historial de parqueo
- **200** — lista de `HistorialParqueoDto`: `{ idRegistro, matricula, marca, modelo, color, nombreCompleto, documentoIdentidad, fechaIngreso, fechaSalida, puertaAcceso, observaciones }`.

### 8.6 Sistema — `api/Sistema`

#### GET `/api/Sistema/hora` — Hora del servidor (público)
- Acceso: **público** (aunque el controlador declara `[Authorize]`, el endpoint tiene `[AllowAnonymous]`).
- **200** — `HoraServidorDto`:
  ```json
  {
    "fechaHoraUtc": "2026-07-31T19:30:00Z",
    "fechaHoraLocal": "2026-07-31T15:30:00",
    "offsetUtc": "-04:00",
    "zonaHoraria": "SA Pacific Standard Time"
  }
  ```
- Es la **fuente de verdad** para que la app móvil unifique horas y valide la zona horaria.

---

## 9. Seed inicial (`Data\DatabaseSeeder.cs`)

Al arrancar (`Program.cs`), la API aplica migraciones (`context.Database.Migrate()`) y ejecuta el seed si `Usuarios` está vacío:

- **Administrador** (si `SEED_ADMIN_PASSWORD` está definida): `DocumentoIdentidad = "admin"`, `TipoUsuarioId = 1`, `RolId = 1`, password hasheado con BCrypt. En dev: `admin` / `admin123`.
- **Usuarios demo** (si `SEED_USUARIOS_DEMO = true`):
  - `portero.parqueo` → `TipoUsuarioId = 2`, `RolId = 2` (password `SEED_PORTERO_PARQUEO_PASSWORD`, en dev `portero123`).
  - `portero.porteria` → `TipoUsuarioId = 2`, `RolId = 3` (password `SEED_PORTERO_PORTERIA_PASSWORD`, en dev `portero123`).

Los tipos de usuario y roles base se insertan como `HasData` en el `DbContext`.

---

## 10. Pruebas (`ServiciosGenerales.Tests`)

xUnit. Cubren:
- `TokenServiceTests` — generación de JWT (claims, expiración ~3 h, issuer/audience).
- `UsoDeRelojServidorTests` — entradas/salidas de portería y parqueo usan la hora del servidor (`IServerClock`).
- `ObtenerHoraServidorUseCaseTests` — hora UTC/local, offset y zona.
- `RefreshTokenTests` — hash/rotación.
- `OffsetUtcUtilTests` — formato `+HH:mm`/`Z`.
- `Fakes` / `FakesAuth` — dobles para inyección de hora fija y repositorios.

Ejecución:
```powershell
dotnet test ServiciosGenerales\ServiciosGenerales.sln
```

---

## 11. Buenas prácticas y notas

- **Hora única**: todo registro de negocio usa `IServerClock`; el cliente no puede influir en las fechas.
- **Refresh tokens hasheados**: en BD solo se almacena SHA-256; el valor en claro se entrega una sola vez.
- **Respuesta genérica de login**: no revela si el documento existe; los intentos fallidos solo se cuentan cuando el usuario tiene credenciales.
- **Usuarios de flujos de entrada sin credenciales**: `PasswordHash = null`, `RolId = null` → no inician sesión.
- **Errores sin fugas de información**: middleware global devuelve `500` genérico.
- **Rate limiting**: dos políticas (`login` 20/min, `global` 300/min).
- El `README.md` del backend es la versión anterior; este documento es la referencia actualizada.
