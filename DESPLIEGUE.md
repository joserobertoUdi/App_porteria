# Guía de Despliegue — Sistema de Control de Acceso y Parqueo UDI

Despliegue del backend **API .NET 8** (publicado como **carpeta**, sin contenedores) y de la app móvil **Flutter** (`app_universidad`). Complementa a `DOCUMENTACION_BACKEND.md` y `DOCUMENTACION_FRONTEND.md`.

---

## 1. Requisitos del entorno

| Para | Requisito |
|------|-----------|
| Backend (compilar/ejecutar) | .NET SDK 8.0+ (runtime .NET 8 en producción) |
| Backend (BD) | SQL Server LocalDB (dev) o SQL Server completo (prod) |
| Frontend (compilar APK) | Flutter SDK 3.x (Dart SDK ^3.10.8) + Android SDK |
| Clientes | Android (APK); también web/desktop vía Flutter si se compilan |

**Ruta del proyecto:** la solución vive en `ServiciosGenerales\ServiciosGenerales\ServiciosGenerales.sln`; el proyecto Flutter en `app_universidad\` (la subcarpeta `app_universidad\app_universidad\` es un scaffold duplicado y obsoleto, no usarla).

> El despliegue del backend **NO usa contenedores (Docker)**: se publica como **carpeta de publicación** (`dotnet publish`) y se ejecuta como proceso (o se hospeda en IIS).

---

## 2. Backend — Entorno de desarrollo

```powershell
cd ServiciosGenerales\ServiciosGenerales

# (Opcional) definir la clave JWT en la sesión; si no, usa appsettings.Development.json
$env:JWT_KEY = "clave-segura-de-al-menos-32-caracteres"

dotnet restore
dotnet run --project ServiciosGenerales.Api
```

Al arrancar, la API **aplica las migraciones automáticamente** (`context.Database.Migrate()`) y ejecuta el **seed** inicial.

- Swagger (solo en Development): `http://localhost:5169/swagger`
- Perfil `http` del `launchSettings.json`: escucha en `http://0.0.0.0:5169`.
- Credenciales de desarrollo (seed): `admin` / `admin123` · `portero.parqueo` / `portero123` · `portero.porteria` / `portero123`.

### Aplicar migraciones manualmente (alternativa)

```powershell
dotnet ef database update --project ServiciosGenerales.Infraestructura --startup-project ServiciosGenerales.Api
```

### Ejecutar pruebas del backend

```powershell
dotnet test ServiciosGenerales.sln
```

---

## 3. Backend — Producción (publicación en carpeta)

### 3.1 Compilar la publicación

```powershell
cd ServiciosGenerales\ServiciosGenerales
dotnet publish ServiciosGenerales.Api -c Release -o C:\publish\udi-api
```

El resultado es una **carpeta autocontenida/framework-dependent** con el ejecutable `ServiciosGenerales.Api.exe` y la configuración.

### 3.2 Configurar variables de entorno en el servidor

En Windows (variables a nivel de sistema o de servicio):

```powershell
setx DB_CONNECTION_STRING "Server=IP_SQL;Database=UniversidadDB;User Id=sa;Password=****;TrustServerCertificate=True"
setx JWT_KEY "clave-segura-aleatoria-de-al-menos-32-caracteres"
setx JWT_ISSUER "UDI"
setx JWT_AUDIENCE "app-universidad"
setx JWT_EXPIRE_MINUTES "180"
setx JWT_REFRESH_TOKEN_DAYS "7"
setx AUTH_MAX_INTENTOS_FALLIDOS "5"
setx AUTH_DURACION_BLOQUEO_MIN "15"
setx SEED_ADMIN_PASSWORD "una-contrasena-admin-segura"
setx SEED_USUARIOS_DEMO "false"
```

> `setx` afecta a procesos nuevos; para el servicio actual use `[Environment]::SetEnvironmentVariable(..., "Machine")` o reinicie el proceso/servicio tras definirlas.

**Obligatorias en producción:** `DB_CONNECTION_STRING` y `JWT_KEY`. Sin `JWT_KEY` la API no arranca (`InvalidOperationException`).

### 3.3 Ejecutar

```powershell
cd C:\publish\udi-api
.\ServiciosGenerales.Api.exe
```

o instalarlo como **servicio Windows** (ej. con `sc create` o NSSM) para que arranque con el sistema. También puede hospedarse en **IIS** (in-process) apuntando el sitio a la carpeta publicada y definiendo las variables en el ApplicationHost o en el `web.config`.

### 3.4 Notas de entorno

- **Swagger solo en Development** (por `app.Environment.IsDevelopment()`), por lo que en producción no se expone.
- En producción, `SEED_USUARIOS_DEMO` debe ser `false` y `SEED_ADMIN_PASSWORD` debe ser una contraseña segura (o no definirla si el administrador ya existe; el seed solo corre si `Usuarios` está vacío).
- La API usa **HTTP** por defecto (`UseHttpsRedirection` intenta forzar HTTPS si hay certificado). Se recomienda terminar **TLS** en IIS/reverse proxy o publicar el perfil `https` con certificado válido.
- Si la base de datos ya existe con la estructura previa, las migraciones se aplican automáticamente al primer arranque; haga **backup** antes (`UniversidadDB.bak` está disponible en la raíz del repo como referencia).

---

## 4. Frontend — Compilación y despliegue del APK

```powershell
cd app_universidad
flutter pub get
flutter analyze
flutter test
flutter build apk --release
```

- APK generado: `app_universidad\build\app\outputs\flutter-apk\app-release.apk` (~47,9 MB).
- La app se conecta a la API mediante `ApiConstants.baseUrl` en `lib\core\constants\api_constants.dart` (actualmente `http://100.106.35.85:5169`).

### Conexión app → servidor

| Escenario | `baseUrl` |
|-----------|-----------|
| Emulador (misma máquina que la API) | `http://10.0.2.2:5169` (Android emulator) |
| Dispositivo físico en la misma red | `http://<IP_LAN_del_servidor>:5169` (p. ej. la actual `http://100.106.35.85:5169`) |
| Producción con dominio | `https://api.dominio.edu` (sin `/api` al final) |

> Editar `baseUrl`, recompilar y redistribuir el APK. El `AndroidManifest.xml` actual permite HTTP (`usesCleartextTraffic="true"`); si despliega con HTTPS conviene revisarlo. La firma del release aún usa llaves de **debug** (`build.gradle.kts`): configurar un **keystore propio** antes de distribuir.

---

## 5. Variables de entorno (resumen)

| Variable | Backend/Frontend | Propósito |
|----------|:---:|-----------|
| `DB_CONNECTION_STRING` | Backend | Cadena a SQL Server (obligatoria en prod) |
| `JWT_KEY` | Backend | Clave de firma JWT ≥ 32 caracteres (obligatoria en prod) |
| `JWT_ISSUER` | Backend | Emisor (`UDI`) |
| `JWT_AUDIENCE` | Backend | Audiencia (`app-universidad`) |
| `JWT_EXPIRE_MINUTES` | Backend | Validez del access token (default 180) |
| `JWT_REFRESH_TOKEN_DAYS` | Backend | Validez del refresh token (default 7) |
| `AUTH_MAX_INTENTOS_FALLIDOS` | Backend | Máx. intentos de login (default 5) |
| `AUTH_DURACION_BLOQUEO_MIN` | Backend | Minutos de bloqueo (default 15) |
| `SEED_ADMIN_PASSWORD` | Backend | Password del administrador inicial |
| `SEED_ADMIN_NOMBRE` / `SEED_ADMIN_DOCUMENTO` / `SEED_ADMIN_TIPO_USUARIO_ID` / `SEED_ADMIN_ROL_ID` | Backend | Datos del admin de seed |
| `SEED_USUARIOS_DEMO` | Backend | Crea porteros demo (`false` en prod) |
| `SEED_PORTERO_PARQUEO_PASSWORD` / `SEED_PORTERO_PORTERIA_PASSWORD` | Backend | Passwords de porteros demo |
| `SHAREPOINT_BASE_URL` | Backend | URL base de fotos (fase futura) |
| `ApiConstants.baseUrl` (código) | Frontend | URL del servidor para la app |

Referencia completa en `ServiciosGenerales\ServiciosGenerales\.env.example`.

---

## 6. Checklist de puesta en producción

- [ ] Publicar la API: `dotnet publish ServiciosGenerales.Api -c Release -o <carpeta>`.
- [ ] Definir variables de entorno del servidor: al menos `DB_CONNECTION_STRING` y `JWT_KEY`.
- [ ] Verificar que la BD destino existe y respaldarla (backup) antes del primer arranque.
- [ ] Deshabilitar seed de demo: `SEED_USUARIOS_DEMO=false`; definir `SEED_ADMIN_PASSWORD` seguro (o pre-crear el admin).
- [ ] Confirmar que la API arranca y responde: `GET /api/Sistema/hora` (público) y login con `POST /api/Auth/login`.
- [ ] Asegurar TLS (IIS/reverse proxy con certificado) o verificar el escenario de red previsto.
- [ ] Configurar **firma propia** del APK release (keystore) antes de distribuir.
- [ ] Ajustar `ApiConstants.baseUrl` al servidor final y recompilar el APK.
- [ ] Probar en un dispositivo físico real (login, entrada/salida portería y parqueo, parqueos activos, admin).
- [ ] Validar que la **zona horaria** del dispositivo coincide con la del servidor (la app bloquea el acceso si no coincide).
- [ ] Revisar `flutter analyze` y `flutter test`, y `dotnet test` antes de cada release.

---

## 7. Seguridad (recomendaciones)

- **JWT key**: aleatoria, ≥ 32 caracteres, única por entorno. Nunca exponer `ClaveDeDesarrolloUDI-2026-NoUsarEnProduccion-12345` en producción.
- **HTTPS**: el tráfico actual es HTTP; en producción terminar TLS y eliminar `usesCleartextTraffic` del APK si aplica.
- **Rate limiting**: ya activo (login 20/min, resto 300/min).
- **Bloqueo de cuentas**: 5 intentos fallidos → 15 min de bloqueo (configurable).
- **Usuarios sin credenciales**: los creados por los flujos de entrada no pueden iniciar sesión.
- **Refresh tokens**: almacenados hasheados y rotados en cada renovación.
- **Backups** periódicos de `UniversidadDB`.

---

## 8. Solución de problemas

| Síntoma | Causa probable / solución |
|---------|---------------------------|
| La API no arranca: "Cadena de conexión no configurada" | Falta `DB_CONNECTION_STRING` o la conexión LocalDB del `appsettings.json` |
| La API no arranca: "JWT_KEY no configurada" | Falta `JWT_KEY` (obligatoria en prod) |
| `429 Too Many Requests` | Se excedió el rate limit (login 20/min) |
| `401` al renovar sesión | Refresh token revocado/expirado → volver a iniciar sesión |
| `403` en login | Cuenta bloqueada por intentos fallidos (esperar el tiempo configurado) |
| La app no conecta | `baseUrl` incorrecta o el dispositivo no alcanza la IP/puerto (revisar firewall, puerto 5169) |
| Error de zona horaria en la app | La hora/offset del dispositivo no coincide con el servidor |
| Swagger no aparece | La API corre en `Production` (Swagger solo en Development) |
| APK con firma debug | `build.gradle.kts` usa `signingConfigs.debug`; configurar keystore propio |
