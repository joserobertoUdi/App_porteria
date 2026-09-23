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
| `PROVIDER_KEY` | Clave de autenticación con SharepointApi | `tu_clave_secreta` |

---

## Checklist de Verificación y Validación

### Estado de Servicios Locales

| Servicio | URL | Puerto | Estado |
|----------|-----|--------|--------|
| Backend API (.NET) | http://localhost:5169 | 5169 | ✅ Running |
| Swagger UI | http://localhost:5169/swagger | 5169 | ✅ Running |
| Panel Admin (Angular) | http://localhost:4200 | 4200 | ✅ Running |
| Flutter App (dispositivo) | — | — | ✅ Running |
| ADB Reverse Tunnel | tcp:5169 → tcp:5169 | 5169 | ✅ Active |
| SQL Server (10.1.210.10) | 10.1.210.10 | 1433 | ✅ Connected |

### Variables de Entorno — Backend (.env)

| Variable | Valor Configurado | Estado |
|----------|-------------------|--------|
| `DB_CONNECTION_STRING` | `Server=10.1.210.10;Database=UniversidadDB;...` | ✅ |
| `JWT_KEY` | `ClaveDeDesarrolloUDI-2026-NoUsarEnProduccion-12345` | ✅ |
| `JWT_ISSUER` | `UDI` | ✅ |
| `JWT_AUDIENCE` | `app-universidad` | ✅ |
| `JWT_EXPIRE_MINUTES` | `180` | ✅ |
| `JWT_REFRESH_TOKEN_DAYS` | `7` | ✅ |
| `AUTH_MAX_INTENTOS_FALLIDOS` | `5` | ✅ |
| `AUTH_DURACION_BLOQUEO_MIN` | `15` | ✅ |
| `SHAREPOINT_BASE_URL` | `http://10.1.210.10/SharepointApi` | ✅ |
| `PROVIDER_KEY` | `ProviderKeyParaSharepoint` | ✅ |
| `SEED_ADMIN_PASSWORD` | `admin123` | ✅ |
| `SEED_USUARIOS_DEMO` | `true` | ✅ |

### Variables de Entorno — Flutter (compile-time)

| Variable | Valor Inyectado | Estado |
|----------|-----------------|--------|
| `API_BASE_URL` | `http://localhost:5169` | ✅ via ADB reverse |
| `SHAREPOINT_BASE_URL` | `http://10.1.210.10/SharepointApi` | ✅ |
| `PROVIDER_KEY` | `ProviderKeyParaSharepoint` | ✅ |

---

### Fase 0: Seguridad y Configuración

| # | Verificación | Estado |
|:-:|-------------|:------:|
| A1.1 | `DB_CONNECTION_STRING` se lee desde variable de entorno (`Program.cs:16`) | ✅ |
| A1.2 | `JWT_KEY` se lee desde variable de entorno (`Program.cs:20`) | ✅ |
| A1.3 | `JWT_ISSUER/AUDIENCE/EXPIRE_MINUTES` se leen desde env vars con fallback | ✅ |
| A1.4 | `appsettings.json` y `appsettings.Development.json` eliminados de git tracking (`git rm --cached`) | ✅ |
| A1.5 | `.gitignore` del subdirectorio lista `appsettings*.json` | ✅ |
| A1.6 | `.env.example` documenta todas las variables incluyendo `PROVIDER_KEY` | ✅ |
| A1.7 | `.env.example` copiado a raíz del proyecto | ✅ |
| A2.1 | `SHAREPOINT_BASE_URL` en `sharepoint_constants.dart` se inyecta via `--dart-define` | ✅ |
| A2.2 | `PROVIDER_KEY` en `sharepoint_constants.dart` se inyecta via `--dart-define` | ✅ |
| A2.3 | `API_BASE_URL` en `api_constants.dart` se inyecta via `--dart-define` | ✅ |
| A2.4 | `SharepointConstants.isConfigured` verifica que PROVIDER_KEY no esté vacía | ✅ |
| A2.5 | `main.dart` imprime warning si PROVIDER_KEY no está configurada | ✅ |
| A2.6 | `PROVIDER_KEY` agregado al `.env` del backend | ✅ |
| A2.7 | `SHAREPOINT_BASE_URL` agregado al `.env` del backend | ✅ |

---

### Fase 1: Identidad Visual

| # | Verificación | Estado |
|:-:|-------------|:------:|
| M1.1 | Logo ajustado a 160x90 en `login_page.dart` | ✅ |
| M1.2 | Texto "CONTROL DE ACCESO" en LoginPage | ✅ |
| M2.1 | App renombrada a "Control de Acceso UDI" en `main.dart` | ✅ |
| M2.2 | `applicationId` = `com.udi.controlacceso` en `build.gradle.kts` | ✅ |
| M2.3 | Nombre en `Info.plist` (iOS) = "Control de Acceso UDI" | ✅ |
| M2.4 | Nombre en `AndroidManifest.xml` = "Control de Acceso UDI" | ✅ |
| M5.1 | Logo UDI a la izquierda del AppBar en 11 páginas | ✅ |

---

### Fase 2: Login Mejorado (Biometría + Último DNI)

| # | Verificación | Estado |
|:-:|-------------|:------:|
| M3.1 | `local_auth: ^2.3.0` en `pubspec.yaml` | ✅ |
| M3.2 | Permisos biométricos en `AndroidManifest.xml` | ✅ |
| M3.3 | `NSFaceIDUsageDescription` en `Info.plist` | ✅ |
| M3.4 | `BiometricService` verifica soporte y autentica | ✅ |
| M3.5 | `AuthProvider.loginWithBiometric()` usa credenciales cacheadas | ✅ |
| M3.6 | Login biométrico completa login con JWT del servidor | ✅ |
| M3.7 | Auto-trigger de biometría al abrir Login si `canUseBiometric` | ✅ |
| M4.1 | `shared_preferences: ^2.5.3` en `pubspec.yaml` | ✅ |
| M4.2 | `UserPrefs` persiste último DNI + preferencia biométrica + contraseña | ✅ |
| M4.3 | `UserPrefs.savePassword/getPassword/clearPassword` implementados | ✅ |
| M4.4 | `LoginPage` pre-llena campo DNI con último usuario | ✅ |

---

### Fase 3: Mensajes y Manejo de Errores

| # | Verificación | Estado |
|:-:|-------------|:------:|
| M6.1 | `error_messages.dart` — catálogo `AppErrors` con todos los errores | ✅ |
| M6.2 | `api_client.dart` — `_send()` captura excepciones tipadas | ✅ |
| M6.3 | `_handleResponse()` mapea HTTP status a excepciones específicas | ✅ |
| M7.1 | `_buscarActivo()` en `registrar_salida_parqueo_page.dart` tiene try/catch | ✅ |
| M7.2 | `catch (_)` silenciosos eliminados en 5 archivos | ✅ |
| M7.3 | `error_view.dart` — widget reutilizable con retry | ✅ |
| M7.4 | `logger_service.dart` — servicio centralizado de logging | ✅ |
| M7.5 | `LoggerService` integrado en `api_client.dart` | ✅ |

---

### Fase 4: Foto Integrada (SharePoint via Backend)

| # | Verificación | Estado |
|:-:|-------------|:------:|
| M9.1 | `ISharepointService` — interfaz en Aplicacion | ✅ |
| M9.2 | `SharepointService` — implementación con `HttpClient` + `ProviderKey` | ✅ |
| M9.3 | `SharepointSettings` — modelo de configuración | ✅ |
| M9.4 | `PorteriaController` — endpoint `entrada-completa-foto` multipart | ✅ |
| M9.5 | `RegistrarEntradaPorteriaCompletaUseCase.EjecutarConFoto()` | ✅ |
| M9.6 | `registrar_entrada_page.dart` envía foto via `api.postMultipart()` | ✅ |
| M9.7 | `Program.cs` carga `SHAREPOINT_BASE_URL` y `PROVIDER_KEY` desde env vars | ✅ |
| M9.8 | `DependencyInjection.cs` registra `SharepointService` | ✅ |
| M9.9 | Foto es opcional (entrada funciona sin foto) | ✅ |
| M9.10 | `registrar_salida_page.dart` muestra foto de entrada para verificación | ✅ |

---

### Fase 5: Dapper Migration

| # | Verificación | Estado |
|:-:|-------------|:------:|
| M8.1 | `Dapper 2.1.66` + `Microsoft.Data.SqlClient 5.2.3` en `.csproj` | ✅ |
| M8.2 | `DapperContext.cs` — wrapper de `SqlConnection` | ✅ |
| M8.3 | `TiposUsuarioRepository` migrado a Dapper | ✅ |
| M8.4 | `RefreshTokenRepository` migrado a Dapper | ✅ |
| M8.5 | `UsuarioRepository` migrado a Dapper | ✅ |
| M8.6 | `RegistroPorteriaRepository` migrado a Dapper | ✅ |
| M8.7 | `VehiculoRepository` migrado a Dapper | ✅ |
| M8.8 | `RegistroParqueoRepository` migrado a Dapper | ✅ |
| M8.9 | EF Core retornado solo para migraciones + seed | ✅ |
| M8.10 | `DependencyInjection.cs` registra `DapperContext` (singleton) + repos (scoped) | ✅ |
| M8.11 | `dotnet build` — 0 errores, 0 warnings | ✅ |
| M8.12 | `dotnet test` — 81/81 tests pasan (48 unit + 33 integration) | ✅ |

---

### Fase 6: Comunicación y Conectividad

| # | Verificación | Estado |
|:-:|-------------|:------:|
| C1.1 | Backend responde en `http://localhost:5169` | ✅ |
| C1.2 | Swagger UI accesible en `/swagger` | ✅ |
| C1.3 | Login `POST /api/Auth/login` retorna JWT + refresh token | ✅ |
| C1.4 | JWT contiene claims correctos (nombre, rol, documento) | ✅ |
| C1.5 | `flutter analyze` — 0 errores (18 info pre-existentes) | ✅ |
| C1.6 | App Flutter corriendo en dispositivo `IZYTC679MB9T7PLF` | ✅ |
| C1.7 | ADB reverse tunnel activo `tcp:5169 → tcp:5169` | ✅ |
| C1.8 | Panel Admin accesible en `http://localhost:4200` | ✅ |
| C1.9 | SQL Server remoto accesible (10.1.210.10) | ✅ |
| C1.10 | CORS configurado para `localhost:4200`, `localhost:5169`, `localhost:8388` | ✅ |

---

### Credenciales de Prueba

| Usuario | Contraseña | Rol | Puerto |
|---------|-----------|-----|--------|
| `admin` | `admin123` | Administrador | — |
| `portero1` | `portero123` | PorteroParqueo | — |
| `portero2` | `portero123` | PorteroPorteria | — |

---

### Pruebas Pendientes (pendientes de implementar)

| # | Prueba | Tipo | Prioridad |
|:-:|--------|------|:---------:|
| T1 | Login con credenciales válidas retorna 200 + JWT | API test | Alta |
| T2 | Login con credenciales inválidas retorna 401 | API test | Alta |
| T3 | Login con cuenta bloqueada retorna 429 | API test | Alta |
| T4 | Refresh token renueva sesión correctamente | API test | Alta |
| T5 | Entrada portería sin foto registra correctamente | API test | Alta |
| T6 | Entrada portería con foto sube a SharepointApi | API test | Alta |
| T7 | Salida portería muestra foto de entrada | API test | Media |
| T8 | SharepointService sube archivo correctamente | Unit test | Alta |
| T9 | SharepointService maneja stream vacío | Unit test | Media |
| T10 | SharepointService maneja error HTTP 500 | Unit test | Media |
| T11 | UserPrefs guarda/recupera DNI | Flutter test | Alta |
| T12 | UserPrefs guarda/recupera contraseña | Flutter test | Alta |
| T13 | UserPrefs guarda/recupera preferencia biométrica | Flutter test | Alta |
| T14 | BiometricService retorna soporte del dispositivo | Flutter test | Alta |
| T15 | AuthProvider login exitoso obtiene JWT | Flutter test | Alta |
| T16 | AuthProvider login biométrico usa credenciales cacheadas | Flutter test | Alta |
| T17 | ApiClient postMultipart envía campos y archivo | Flutter test | Alta |
| T18 | ApiClient maneja 401 con refresh automático | Flutter test | Alta |
| T19 | LoginPage pre-llena DNI desde UserPrefs | Widget test | Media |
| T20 | LoginPage auto-dispara biometría si canUseBiometric | Widget test | Media |
| T21 | ErrorView muestra retry button y ejecuta callback | Widget test | Media |
| T22 | UdiAppBarLogo renderiza imagen | Widget test | Baja |
| T23 | Verificar flujo completo: login → entrada con foto → salida | E2E test | Alta |
| T24 | Verificar rate limiting (21+ intentos en 1 min) | API test | Media |
| T25 | Verificar conexión caída muestra error en español | E2E test | Media |

---

### Comandos de Inicio Rápido

```powershell
# 1. Cargar variables de entorno y levantar Backend API
cd porteria\ServiciosGenerales\ServiciosGenerales
# Copiar .env.example a .env y configurar valores
.\start-api.ps1

# 2. Configurar túnel ADB
adb reverse tcp:5169 tcp:5169

# 3. Levantar Flutter app en dispositivo
cd porteria\app_universidad
flutter run -d <DEVICE_ID> `
  --dart-define=API_BASE_URL=http://localhost:5169 `
  --dart-define=SHAREPOINT_BASE_URL=http://10.1.210.10/SharepointApi `
  --dart-define=PROVIDER_KEY=ProviderKeyParaSharepoint

# 4. Levantar Panel Admin
cd porteria\panel_admin
npm install
npx ng serve --host 0.0.0.0 --port 4200
```
