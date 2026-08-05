# Roadmap — Sistema de Control de Acceso y Parqueo UDI

## Identidad Unificada: CARNET DE IDENTIDAD (DocumentoIdentidad)

**Decisión de diseño clave:** El identificador único para TODOS los usuarios (estudiantes, trabajadores y visitantes) es el **carnet de identidad** (`Usuarios.DocumentoIdentidad`).

| Tipo de usuario | DocumentoIdentidad |
|----------------|-------------------|
| Estudiante | Nº de estudiante institucional |
| Trabajador/Docente | Nº de empleado institucional |
| Visitante | Nº de carnet de identidad / pasaporte nacional |

**Esto unifica los registros de Parqueo y Portería bajo un mismo `UsuarioId`.**

### Flujo de ingreso (Parqueo y Portería):
1. Portero solicita **carnet de identidad** a la persona
2. Sistema busca en `Usuarios` por `DocumentoIdentidad`
3. Si existe → muestra datos y procede
4. Si no existe → crea usuario con ese carnet + nombre
5. Si es parqueo → adicionalmente pregunta placa (busca o crea vehículo)
6. Registra entrada (Portería y/o Parqueo apuntan al mismo `UsuarioId`)

La **placa** es solo un atributo del vehículo (`Vehiculos.Matricula`), nunca identifica a la persona. Los registros de parqueo y portería se relacionan por `UsuarioId` → `Usuarios.DocumentoIdentidad`.

## Estado Actual

### Backend (.NET 8)
- Clean Architecture con EF Core + migraciones
- JWT + BCrypt implementado
- Variables de entorno: `JWT_KEY`, `JWT_ISSUER`, `JWT_AUDIENCE`, `DB_CONNECTION_STRING`
- Controladores: `AuthController`, `UsuarioController`, `PorteriaController`
- BD: `UniversidadDB` con 5 tablas normalizadas

### Base de Datos
- 5 tablas con EF Core Migrations + seed data de TiposUsuario
- `RegistrosPorteria` sin `TiempoEstadiaMinutos` (normalizado)
- `DocumentoIdentidad` con UNIQUE INDEX
- `Vehiculos` y `RegistroParqueo` creadas

---

## Fase 1: Base de Datos — Migraciones + Nuevas Tablas + Normalización

**Objetivo:** Migrar de Dapper a EF Core, agregar estructura de parqueo, normalizar.

| # | Tarea | Detalle técnico |
|:-:|-------|-----------------|
| 1.1 | Instalar EF Core en Infraestructura | NuGet: `Microsoft.EntityFrameworkCore.SqlServer`, `Microsoft.EntityFrameworkCore.Tools` |
| 1.2 | Crear `UniversidadDbContext` | Configurar conexión desde `appsettings.json` + variables de entorno |
| 1.3 | Agregar entidad `Vehiculo` | `Id`, `UsuarioId` (FK → Usuarios), `Matricula` (UNIQUE), `Marca`, `Modelo`, `Color`, `Activo`, `FotoUrl`, `FechaRegistro` |
| 1.4 | Agregar entidad `RegistroParqueo` | `Id`, `VehiculoId` (FK → Vehiculos), `FechaIngreso`, `FechaSalida` (nullable), `PuertaAcceso`, `Observaciones` |
| 1.5 | Normalizar `RegistroPorteria` | Eliminar columna `TiempoEstadiaMinutos` — calcular con `DATEDIFF` en consulta |
| 1.6 | Agregar `UniqueIndex` a `DocumentoIdentidad` en `Usuarios` | Evitar duplicados |
| 1.7 | Crear migración inicial | `dotnet ef migrations add InitialCreate` |
| 1.8 | Reemplazar repositorios Dapper por EF Core | `UsuarioRepository`, `RegistroPorteriaRepository` |
| 1.9 | Crear `VehiculoRepository` y `RegistroParqueoRepository` | Con EF Core |
| 1.10 | Seed data inicial | `TiposUsuario`: Estudiante (1), Trabajador (2), Visitante (3) |
| 1.11 | Agregar columna `PasswordHash` a `Usuarios` | Para autenticación (Fase 2) |

### Diagrama BD Final

```
TiposUsuario (1) ──< Usuarios (1) ──< Vehiculos (1) ──< RegistroParqueo
                              │
                              └──< RegistrosPorteria
```

---

## Fase 2: Autenticación JWT

**Objetivo:** Login seguro con JWT, variables de entorno, endpoints protegidos.

| # | Tarea | Detalle técnico |
|:-:|-------|-----------------|
| 2.1 | Agregar NuGet de autenticación | `Microsoft.AspNetCore.Authentication.JwtBearer` + `BCrypt.Net-Next` |
| 2.2 | Configurar JWT en `appsettings.json` | `"Jwt": { "Key": "...", "Issuer": "UDI", "Audience": "app-universidad", "ExpireMinutes": 120 }` |
| 2.3 | Leer configuración desde variables de entorno | `JWT_KEY`, `JWT_ISSUER`, `JWT_AUDIENCE` con fallback a `appsettings.json` |
| 2.4 | Leer conexión BD desde variable de entorno | `DB_CONNECTION_STRING` con fallback a `appsettings.json` |
| 2.5 | Crear `AuthController` | `POST /api/Auth/login` → `{ documentoIdentidad, password }` → devuelve `{ token, usuario }` |
| 2.6 | Registrar `AddAuthentication().AddJwtBearer()` en `Program.cs` | Reemplazar el `UseAuthentication()` huérfano |
| 2.7 | Proteger endpoints con `[Authorize]` | Todos excepto `POST /api/Auth/login` |
| 2.8 | Crear `LoginRequestDto` y `LoginResponseDto` | DTOs de autenticación |

---

## Fase 3: Imágenes con SharePoint

**Objetivo:** Almacenar URLs de SharePoint para fotos de usuarios y vehículos.

| # | Tarea | Detalle técnico |
|:-:|-------|-----------------|
| 3.1 | Configurar URL base de SharePoint | `appsettings.json`: `"SharePoint": { "BaseUrl": "https://udiedu.sharepoint.com/sites/tu-sitio" }` |
| 3.2 | Leer desde variable de entorno | `SHAREPOINT_BASE_URL` con fallback |
| 3.3 | Endpoint `POST /api/Usuario/upload-foto` | Recibe `IFormFile`, construye ruta SharePoint, guarda `FotoUrl` relativa |
| 3.4 | Almacenar ruta relativa en BD | `FotoUrl` = `"/sites/.../fotos/123.jpg"` → se combina con `BaseUrl` al servir |
| 3.5 | Extender a `Vehiculos` | Mismo mecanismo para foto del vehículo |

---

## Fase 4: Backend — Endpoints de Parqueo

**Objetivo:** Endpoints de parqueo con flujo basado en carnet de identidad (sin CRUD administrativo).

**Regla de negocio:** El portero solo interactúa con 2 pantallas (ingreso/salida). El sistema automáticamente crea usuarios y vehículos cuando no existen. No hay CRUD de gestión administrativa.

**Flujo de ingreso a parqueo:**
1. Portero ingresa **carnet de identidad** → `GET /api/Usuario/buscar/{carnet}`
2. Si el usuario no existe → crea usuario con ese carnet
3. Portero ingresa **placa** → `GET /api/Parqueo/vehiculos/buscar-placa/{placa}`
4. Si el vehículo no existe → lo crea asociado al usuario
5. Sistema registra la entrada → `POST /api/Parqueo/entrada`

| # | Tarea | Detalle técnico |
|:-:|-------|-----------------|
| 4.1 | DTOs de parqueo | `RegistrarEntradaParqueoDto` (con `usuarioId` + `vehiculoId` ya resueltos), `RegistrarSalidaParqueoDto` (`id`), `ParqueoActivoDto` |
| 4.2 | UseCase: `BuscarOCrearUsuarioPorCarnetUseCase` | Busca por `DocumentoIdentidad`; si no existe, crea `Usuario` con ese carnet y nombre ingresado |
| 4.3 | UseCase: `BuscarOCrearVehiculoUseCase` | Busca por matrícula; si no existe, crea `Vehiculo` asociado al `usuarioId` |
| 4.4 | UseCase: `RegistrarEntradaParqueoUseCase` | Crea `RegistroParqueo` con `VehiculoId`, setea `FechaIngreso` |
| 4.5 | UseCase: `RegistrarSalidaParqueoUseCase` | Actualiza `FechaSalida` del `RegistroParqueo` |
| 4.6 | UseCase: `ObtenerParqueosActivosUseCase` | Lista `RegistroParqueo` con `FechaSalida IS NULL` con datos de vehículo y usuario |
| 4.7 | UseCase: `ObtenerHistorialParqueoUseCase` | Lista registros por `usuarioId` o `vehiculoId` |
| 4.8 | Usar `IVehiculoRepository` (ya creado en Fase 1) | Buscar por matrícula, listar por usuario, crear |
| 4.9 | Usar `IRegistroParqueoRepository` (ya creado en Fase 1) | Entrada, salida, activos, historial |
| 4.10 | Crear `ParqueoController` | `POST /api/Parqueo/entrada`, `POST /api/Parqueo/salida`, `GET /api/Parqueo/activos`, `GET /api/Parqueo/historial/{usuarioId}`, `GET /api/Parqueo/vehiculos/{usuarioId}`, `GET /api/Parqueo/vehiculos/buscar-placa/{placa}`, `POST /api/Parqueo/vehiculos/crear` |
| 4.11 | Configurar CORS | Permitir origen del Flutter app |

---

## Fase 5: Flutter — Conexión Real con la API

**Objetivo:** Reemplazar datos mock por llamadas HTTP reales con JWT.

| # | Tarea | Detalle técnico |
|:-:|-------|-----------------|
| 5.1 | Completar `api_constants.dart` | `baseUrl = "http://localhost:5169/api"`, `sharePointBaseUrl = "https://..."` |
| 5.2 | Crear `AuthRemoteDataSource` | Llamadas a `POST /api/Auth/login` |
| 5.3 | Crear `PorteriaRemoteDataSourceReal` | Llamadas a `/api/Porteria/*` |
| 5.4 | Crear `ParqueoRemoteDataSourceReal` | Llamadas a `/api/Parqueo/*` |
| 5.5 | Swappear DataSources vía Provider | `ProxyProvider` o `ChangeNotifierProvider` |
| 5.6 | Agregar `http` headers con JWT | Helper que adjunta `Authorization: Bearer <token>` |
| 5.7 | Crear `UsuarioModel` + `ParqueoModel` | Modelos con `fromJson`/`toJson` alineados con los DTOs del backend |

---

## Fase 6: Flutter — Autenticación y Login Real

**Objetivo:** Login funcional con persistencia de sesión.

| # | Tarea | Detalle técnico |
|:-:|-------|-----------------|
| 6.1 | Agregar `shared_preferences` o `flutter_secure_storage` | Persistir token JWT localmente |
| 6.2 | Crear `AuthProvider` (ChangeNotifier) | `login(documento, password)`, `logout()`, `tryAutoLogin()`, `isAuthenticated`, `token`, `currentUser` |
| 6.3 | Conectar `LoginPage` al `AuthProvider` | Llamar `authProvider.login()` en lugar de navegar directo |
| 6.4 | Mostrar errores de autenticación | SnackBar con mensaje de error del backend |
| 6.5 | Proteger rutas con `Redirect` | Si no hay token → redirigir a LoginPage |
| 6.6 | Alinear modelo Flutter con backend | Renombrar `ci` → `documentoIdentidad`, `tipo` → `tipoUsuarioId`, `puerta` → `puertaEntrada`, etc. |

---

## Fase 7: Flutter — Integración Portería + Parqueo

**Objetivo:** Pantallas de parqueo y dashboard unificado.

| # | Tarea | Detalle técnico |
|:-:|-------|-----------------|
| 7.1 | Crear `PorteriaProvider` | `registrarEntrada()`, `registrarSalida()`, `obtenerActivos()`, `buscarUsuario()` |
| 7.2 | Crear `ParqueoProvider` | `registrarEntrada()`, `registrarSalida()`, `listarVehiculos()`, `registrarVehiculo()` |
| 7.3 | Refactor `RegistrarEntradaPage` | Conectar a `PorteriaProvider`, TypeAhead real con `GET /api/Usuario/sugerir/{termino}` |
| 7.4 | Crear `RegistrarSalidaPage` real | Buscar visita activa por documento, confirmar salida |
| 7.5 | Crear `RegistrarEntradaParqueoPage` | Seleccionar usuario → seleccionar/registrar vehículo → confirmar ingreso |
| 7.6 | Crear `RegistrarSalidaParqueoPage` | Buscar por matrícula o usuario → confirmar salida |
| 7.7 | Crear `DashboardPage` unificada | Vista que muestra registros del día (portería + parqueo) del usuario autenticado |
| 7.8 | Agregar tarjeta de parqueo al HomePage | Nueva `CustomActionCard` "REGISTRAR PARQUEO" |

---

## Fase 8: Limpieza y Buenas Prácticas

**Objetivo:** Código limpio, sin archivos muertos, consistente.

| # | Tarea | Detalle técnico |
|:-:|-------|-----------------|
| 8.1 | Eliminar `Class1.cs` | Archivo placeholder en Aplicacion |
| 8.2 | Eliminar `RegistoRemoteDataSourceMock` | O moverlo a carpeta `test/` como respaldo |
| 8.3 | Hacer `TiposUsuarioEnum` público | Cambiar `internal` → `public` |
| 8.4 | Unificar `CrearUsuarioAsync` / `RegistrarUsuarioAsync` | Eliminar método duplicado, mantener solo uno |
| 8.5 | Renombrar `RegistoRemoteDataSourceMock` | Corregir typo: `Registo` → `Registro` |
| 8.6 | Agregar `analysis_options.yaml` consistente | Reglas de lint para ambos proyectos |
| 8.7 | Documentar endpoints con Swagger | Asegurar que todos los DTOs tengan `[Required]`, `[StringLength]`, etc. |

---

## Diagrama de Arquitectura Final

```
┌──────────────────────────────────────────────────────────────────┐
│                        FLUTTER APP                               │
│                                                                  │
│  ┌──────────┐  ┌────────────────┐  ┌────────────────────────┐   │
│  │ AuthPage │  │ PorteriaPages  │  │ ParqueoPages           │   │
│  └────┬─────┘  └──────┬─────────┘  └───────────┬────────────┘   │
│       │               │                        │                │
│  ┌────┴───────────────┴────────────────────────┴────────────┐   │
│  │              Provider (State Management)                  │   │
│  │  AuthProvider | PorteriaProvider | ParqueoProvider        │   │
│  └───────────────────────────┬──────────────────────────────┘   │
│                              │ HTTP (JWT Bearer Token)          │
└──────────────────────────────┼──────────────────────────────────┘
                               │
┌──────────────────────────────┼──────────────────────────────────┐
│                     .NET 8 WEB API (C#)                        │
│                                                               │
│  ┌──────────┐  ┌──────────────┐  ┌────────────────────────┐   │
│  │ Auth     │  │ Porteria     │  │ Parqueo                │   │
│  │Controller│  │ Controller   │  │ Controller             │   │
│  └────┬─────┘  └──────┬───────┘  └─────────┬──────────────┘   │
│       │               │                    │                  │
│  ┌────┴───────────────┴────────────────────┴──────────────┐   │
│  │              Use Cases (Aplicacion)                     │   │
│  └────────────────────────┬───────────────────────────────┘   │
│                           │                                   │
│  ┌────────────────────────┴───────────────────────────────┐   │
│  │          EF Core DbContext + Repositories               │   │
│  │          (Infraestructura)                              │   │
│  └────────────────────────┬───────────────────────────────┘   │
│                           │                                   │
│  ┌────────────────────────┴───────────────────────────────┐   │
│  │  Variables de Entorno:                                  │   │
│  │  • DB_CONNECTION_STRING                                 │   │
│  │  • JWT_KEY / JWT_ISSUER / JWT_AUDIENCE                 │   │
│  │  • SHAREPOINT_BASE_URL                                  │   │
│  └────────────────────────────────────────────────────────┘   │
└───────────────────────────────┼───────────────────────────────┘
                                │
┌───────────────────────────────┴───────────────────────────────┐
│                    SQL Server (UniversidadDB)                  │
│                                                               │
│  ┌────────────────────────────────────────────────────────┐   │
│  │  TiposUsuario (1) ──< Usuarios (1) ──< Vehiculos      │   │
│  │                                    (1) ──< Registro    │   │
│  │                                          Parqueo        │   │
│  │                              │                         │   │
│  │                              └──< RegistrosPorteria    │   │
│  └────────────────────────────────────────────────────────┘   │
│                                                               │
│  • Usuarios.FotoUrl → URL relativa SharePoint                 │
│  • Vehiculos.Matricula → UNIQUE                               │
│  • Usuarios.DocumentoIdentidad → UNIQUE                       │
│  • RegistrosPorteria sin TiempoEstadiaMinutos (calculado)     │
└───────────────────────────────────────────────────────────────┘
```

---

## Orden de Prioridad y Tiempos Estimados

| Fase | Descripción | Prioridad | Tiempo |
|:----:|-------------|:---------:|:------:|
| **1** | BD + Migraciones + Tablas nuevas | 🔴 Alta | 2-3 días |
| **2** | JWT + Autenticación + Variables entorno | 🔴 Alta | 1-2 días |
| **3** | SharePoint para imágenes | 🟡 Media | 1 día |
| **4** | Endpoints de parqueo | 🔴 Alta | 2-3 días |
| **5** | Flutter conexión real API | 🔴 Alta | 2 días |
| **6** | Flutter login + auth real | 🔴 Alta | 1-2 días |
| **7** | Flutter vistas parqueo + dashboard | 🟡 Media | 2-3 días |
| **8** | Limpieza y pulido | 🟢 Baja | 1 día |

**Total estimado: 12-17 días hábiles**

---

## Variables de Entorno Requeridas

| Variable | Propósito | Ejemplo |
|----------|-----------|---------|
| `DB_CONNECTION_STRING` | Conexión a SQL Server | `Server=...;Database=UniversidadDB;...` |
| `JWT_KEY` | Clave secreta para firmar tokens | `minimo-32-caracteres-segura-12345` |
| `JWT_ISSUER` | Emisor del token | `UDI` |
| `JWT_AUDIENCE` | Audiencia del token | `app-universidad` |
| `SHAREPOINT_BASE_URL` | URL base de SharePoint para imágenes | `https://udiedu.sharepoint.com/sites/...` |
