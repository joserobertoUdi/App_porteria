# Documentación del Frontend — App Flutter "Control de Acceso UDI"

Aplicación móvil **Flutter** que consume la API REST del backend UDI (`DOCUMENTACION_BACKEND.md`). Usa **Provider** para el estado y sigue una organización por capas (`core` / `data` / `domain` / `presentation`).

- Proyecto real: `app_universidad\` (raíz de esa carpeta)
- SDK Dart: `^3.10.8`
- APK release ≈ 47,9 MB

> **Ojo con la estructura:** existe `app_universidad\app_universidad\` (subcarpeta) que es un **scaffold duplicado y desactualizado** (plantilla por defecto + esqueleto inicial de "registro"). El proyecto que se compila y contiene toda la funcionalidad es el de la **raíz** `app_universidad\`.

---

## 1. Stack tecnológico

| Componente | Tecnología |
|------------|-----------|
| Framework | Flutter (Dart SDK ^3.10.8) |
| Estado | `provider` ^6.1.5+1 |
| HTTP | `http` ^1.6.0 |
| Autocompletado | `flutter_typeahead` ^5.2.0 |
| Serialización | `json_annotation` ^4.10.0 (+ `json_serializable` / `build_runner` en dev) |
| Lint | `flutter_lints` ^6.0.0 |

---

## 2. Arquitectura y principio de capas

El proyecto se organiza por capas tipo **Clean Architecture**, con las reglas habituales: `presentation` depende de `data` y `domain`; `data` depende de `domain` y `core`; `domain` no depende de nada externo.

- **`core/`** — infraestructura transversal: constantes de la API, cliente HTTP, tema, utilidades (zona horaria, formateadores).
- **`data/`** — implementaciones: datasources remotos (HTTP), modelos (`fromJson`) y repositorios (`Impl`).
- **`domain/`** — entidades, contratos de repositorio y casos de uso.
- **`presentation/`** — widgets: pantallas (pages), proveedores de estado (providers) y componentes (widgets).

**Estado:** `ChangeNotifierProvider` (Provider) con `MultiProvider` en `main.dart`. La mayoría de pantallas operativas usan `ApiClient`/datasources directamente vía `context.read<>()`, mientras que la autenticación se centraliza en `AuthProvider`.

---

## 3. Árbol de carpetas

```
app_universidad\
├── android\ / ios\ / web\ / linux\ ...   # plataformas
├── lib\
│   ├── main.dart                         # Punto de entrada + MultiProvider
│   ├── core\
│   │   ├── constants\api_constants.dart  # baseUrl y rutas
│   │   ├── network\api_client.dart       # Cliente HTTP con refresh 401
│   │   ├── theme\app_theme.dart          # Colores y tema UDI
│   │   └── utils\
│   │       ├── zona_horaria_util.dart    # Validación de zona horaria
│   │       └── uppercase_text_formatter.dart
│   ├── data\
│   │   ├── datasources\
│   │   │   ├── auth_remote_data_source.dart
│   │   │   ├── sistema_remote_data_source.dart
│   │   │   ├── parqueo_remote_data_source.dart
│   │   │   └── registro_remote_data_source.dart   # (legado)
│   │   ├── models\
│   │   │   ├── login_response_model.dart
│   │   │   ├── hora_servidor_model.dart
│   │   │   ├── parqueo_models.dart
│   │   │   └── registro_model.dart                 # (legado)
│   │   └── repositories\
│   │       ├── parqueo_repository_impl.dart
│   │       └── registro_repository_impl.dart       # (legado)
│   ├── domain\
│   │   ├── entities\registro.dart                  # (legado)
│   │   ├── repositories\
│   │   │   ├── parqueo_repository.dart
│   │   │   └── registro_repository.dart            # (legado)
│   │   └── usecases\                               # (legado, esqueleto inicial)
│   │       ├── registrar_entrada_usecase.dart
│   │       ├── registrar_salida_usecase.dart
│   │       └── buscar_registro_usecase.dart
│   └── presentation\
│       ├── providers\auth_provider.dart
│       ├── pages\
│       │   ├── login_page.dart
│       │   ├── home_page.dart
│       │   ├── registrar_entrada_page.dart
│       │   ├── registrar_salida_page.dart
│       │   ├── registrar_entrada_parqueo_page.dart
│       │   ├── registrar_salida_parqueo_page.dart
│       │   ├── parqueos_activos_page.dart
│       │   ├── admin_page.dart
│       │   ├── admin_usuarios_page.dart
│       │   ├── crear_usuario_admin_page.dart
│       │   ├── historial_porteria_page.dart
│       │   └── historial_parqueo_page.dart
│       └── widgets\custom_action_card.dart
├── test\                                  # Pruebas unitarias
├── pubspec.yaml
└── analysis_options.yaml
```

> **Legado:** la rama `registro_*` de `domain`/`data` (entidad `Registro`, `RegistroRepository`, `RegistrarEntradaUsecase`, `RegistrarSalidaUsecase`, `BuscarRegistroUseCase`) proviene del primer esqueleto y **no se usa** en las pantallas actuales; las pantallas operativas llaman a `ApiClient`/datasources directamente. Está pendiente de limpieza.

---

## 4. Conexión con la API

### 4.1 `ApiConstants` (`lib/core/constants/api_constants.dart`)

```dart
static const String baseUrl = 'http://100.106.35.85:5169';
```

> La `baseUrl` **no** incluye `/api`; las rutas sí lo incluyen (`/api/Auth/login`, etc.). Para cambiar el servidor basta con editar `baseUrl` y recompilar el APK.

Endpoints definidos: `login`, `usuarioBase` (buscar/sugerir), `porteriaBase` (entrada, entrada-completa, salida, activos), `adminBase` (usuarios, historiales), `parqueoBase` (entrada-completa, entrada, salida, activos, historial, vehículos).

### 4.2 `ApiClient` (`lib/core/network/api_client.dart`)

- Timeout de 20 s por petición.
- Mantiene `_token` y `_refreshToken` en memoria (no persiste en disco).
- Métodos: `get`, `getList` (desenvuelve `{'data': [...]}`), `post`, `put`.
- Cabeceras: `Content-Type: application/json` + `Authorization: Bearer <token>` cuando hay sesión.
- **Renovación automática ante 401** (`_send`): si la respuesta es `401`, hay token y no hay un refresh en curso, invoca `onRefreshRequested` (definido por `AuthProvider`) y **reintenta** la petición original una vez. Si la renovación falla o no hay callback, invoca `onSessionExpired`.
- `_handleResponse`: errores → `Exception(error)` usando `body['error']` o `body['mensaje']`.

### 4.3 `SistemaRemoteDataSource`

`obtenerHoraServidor()` → `GET /api/Sistema/hora` (público). Devuelve `HoraServidorModel` con la hora UTC/local del servidor, su offset y zona.

### 4.4 `AuthRemoteDataSource`

- `login(documento, password)` → `POST /api/Auth/login` → `LoginResponseModel`.
- `refresh(refreshToken)` → `POST /api/Auth/refresh` → `LoginResponseModel` (nuevo access + refresh rotado).

### 4.5 `ParqueoRemoteDataSource`

- `registrarEntradaCompleta(...)` → `POST /api/Parqueo/entrada-completa`.
- `registrarSalida(id)` → `POST /api/Parqueo/salida`.
- `obtenerActivos()` → `GET /api/Parqueo/activos`.
- `buscarVehiculo(placa)` → `GET /api/Parqueo/vehiculos/buscar-placa/{placa}` (devuelve `null` si falla).
- `obtenerVehiculosPorUsuario(usuarioId)` → `GET /api/Parqueo/vehiculos/{usuarioId}`.

### 4.6 Modelos

- `LoginResponseModel` — `token`, `refreshToken`, `nombreCompleto`, `documentoIdentidad`, `tipoUsuarioId`, `rolId`, `rolNombre`, `fotoUrl`.
- `HoraServidorModel` — `fechaHoraUtc`, `fechaHoraLocal`, `offsetUtc`, `zonaHoraria`.
- `ParqueoActivoModel` — `idRegistro`, `matricula`, `marca`, `modelo`, `color`, `nombreCompleto`, `documentoIdentidad`, `fechaIngreso`, `puertaAcceso`, `observaciones`.
- `EntradaCompletaResultadoModel` — `id`, `usuarioId`, `vehiculoId`, `usuarioCreado`, `vehiculoCreado`, `mensaje`.
- `UsuarioModel` — `id`, `nombreCompleto`, `documentoIdentidad`, `tipoUsuarioId`.

---

## 5. Sesión, zona horaria y refresh

### `AuthProvider` (`lib/presentation/providers/auth_provider.dart`)

**Login (`login`)**
1. Consulta la hora del servidor (`SistemaRemoteDataSource.obtenerHoraServidor`).
2. Valida que el offset UTC del dispositivo coincida con el del servidor (`ZonaHorariaUtil.esMismaZona`). Si no coinciden, **bloquea el acceso** con el mensaje: *"Zona horaria del dispositivo no coincide con la del servidor (...). Ajuste la hora del dispositivo y reintente."*
3. Llama al login real; en éxito aplica la sesión (guarda tokens y usuario).
4. En error, expone `error` para mostrarlo en la UI.

**Renovación (`_renovarSesion`)**
- Asignada como `ApiClient.onRefreshRequested`. Usa el `refreshToken` actual contra `/api/Auth/refresh`; en éxito aplica la nueva sesión. Evita refrescos simultáneos con `_refreshEnProceso`.

**Sesión expirada (`_manejarSesionExpirada`)**
- Asignada como `ApiClient.onSessionExpired`. Hace `logout()` y navega a la raíz (`/`, pantalla de login) con `pushNamedAndRemoveUntil`.

**Rol** — expone `esAdministrador`, `esPorteroParqueo`, `esPorteroPorteria` según `rolNombre`, y `rolNombre` / `token` / `nombreCompleto` como getters.

### `ZonaHorariaUtil` (`lib/core/utils/zona_horaria_util.dart`)

- `esMismaZona(Duration dispositivo, String offsetUtcServidor)` — compara el offset del dispositivo con el del servidor.
- Parsea `"Z" | "z" | "UTC"` como 0 y `"+HH:mm" / "-HH:mm"`; devuelve `false` ante formato inválido.

---

## 6. Navegación

- `main.dart` define `MaterialApp` con `initialRoute: '/'` y ruta `'/' → LoginPage`, y un `navigatorKey` global que `AuthProvider` usa para forzar la vuelta al login.
- El resto de la navegación se hace con `Navigator.push(MaterialPageRoute(...))`.
- `LoginPage` decide el destino tras un login exitoso: `AdminPage` si es administrador, `HomePage` en caso contrario.

### Flujo de pantallas

```
LoginPage
 ├── (admin) → AdminPage
 │               ├── AdminUsuariosPage → CrearUsuarioAdminPage
 │               ├── HistorialPorteriaPage
 │               └── HistorialParqueoPage
 └── (porteros) → HomePage  (menú por rol)
                   ├── RegistrarEntradaPage         (portería)
                   ├── RegistrarSalidaPage          (portería)
                   ├── RegistrarEntradaParqueoPage  (parqueo)
                   ├── RegistrarSalidaParqueoPage   (parqueo)
                   ├── ParqueosActivosPage          (parqueo)
                   └── AdminPage                    (solo admin)
```

---

## 7. Pantallas (detalle)

### 7.1 `LoginPage`
- Formulario de "usuario" (carnet) y contraseña con fondo degradado UDI.
- Llama `AuthProvider.login()`; muestra errores con `SnackBar` (incluidos bloqueo de cuenta e inconsistencias de zona horaria).
- Valida que los campos no estén vacíos; deshabilita el botón mientras carga.

### 7.2 `HomePage` — Panel de control
- `AppBar` con nombre del usuario y botón de cerrar sesión.
- Menú tipo grid (2 columnas, responsive hasta 520 px) con `CustomActionCard`, mostrando solo las tarjetas habilitadas por rol:
  - `Administrador`/`PorteroPorteria`: REGISTRAR ENTRADA, REGISTRAR SALIDA (portería).
  - `Administrador`/`PorteroParqueo`: ENTRADA PARQUEO, SALIDA PARQUEO, PARQUEOS ACTIVOS.
  - `Administrador`: ADMIN.

### 7.3 `RegistrarEntradaPage` — Entrada a portería
- Búsqueda rápida por C.I. → `GET /api/Usuario/buscar/{dni}` (pre-llena nombre, documento y tipo de usuario).
- Campos: nombre, C.I., tipo de usuario (1–3), puerta de acceso (Principal/Parqueo), hora (lectura desde `GET /api/Sistema/hora`), motivo.
- Al confirmar → `POST /api/Porteria/entrada-completa`.

### 7.4 `RegistrarSalidaPage` — Salida de portería
- Busca la visita activa por C.I. en `GET /api/Porteria/activos` (filtra localmente por `documentoIdentidad`).
- Muestra confirmación y registra → `POST /api/Porteria/salida` con el `idRegistro` y `puertaSalida`.

### 7.5 `RegistrarEntradaParqueoPage`
- Busca por carnet (`GET /api/Usuario/buscar/{dni}`); si el usuario existe, carga su primer vehículo (`GET /api/Parqueo/vehiculos/{usuarioId}`).
- Campos: carnet, nombre, placa (mayúsculas automáticas), tipo de usuario, marca, modelo, puerta (Principal/Parqueo Este/Parqueo Oeste), color, observaciones.
- Al confirmar → `POST /api/Parqueo/entrada-completa`.

### 7.6 `RegistrarSalidaParqueoPage`
- Busca por placa en `GET /api/Parqueo/activos` (filtra por matrícula).
- Muestra ficha del vehículo y confirma → `POST /api/Parqueo/salida`.

### 7.7 `ParqueosActivosPage`
- Lista `GET /api/Parqueo/activos` con pull-to-refresh y botón de recarga.
- Cada tarjeta permite cerrar la salida directamente (`POST /api/Parqueo/salida`).

### 7.8 `AdminPage`
- Menú de administración: USUARIOS, HISTORIAL PORTERÍA, HISTORIAL PARQUEO. Cierre de sesión en la `AppBar`.

### 7.9 `AdminUsuariosPage`
- Lista `GET /api/Admin/usuarios` con indicador de estado (avatar verde/gris) y botón para crear usuario.

### 7.10 `CrearUsuarioAdminPage`
- Formulario de nombre, carnet, contraseña, tipo de persona (1–3) y rol de sistema (1–3).
- Al confirmar → `POST /api/Admin/usuarios`.

### 7.11 `HistorialPorteriaPage` / `HistorialParqueoPage`
- Listan `GET /api/Admin/porteria/historial` y `GET /api/Admin/parqueo/historial` respectivamente; marcan como "[Activo]" los registros sin salida.

### 7.12 `CustomActionCard` (widget)
- Tarjeta responsiva con ícono + título centrado, sombra y color UDI. Se escala según el ancho disponible.

---

## 8. Tema e identidad visual (`lib/core/theme/app_theme.dart`)

- `udiRed = Color(0xFFD50000)`, `udiDarkRed = Color(0xFFA30000)`.
- `udiGradient` — degradado vertical rojo (login y pantallas de formulario).
- `ThemeData` con `colorScheme` derivado del rojo UDI, inputs con subrayado blanco, y `ElevatedButton` blanco con texto rojo y bordes redondeados.

---

## 9. Pruebas (`test/`)

| Archivo | Cubre |
|---------|-------|
| `api_client_refresh_test.dart` | Renovación de token ante 401, reintento, sesión expirada, sin recursión en refresh, 401 sin token (login fallido) no renueva |
| `zona_horaria_util_test.dart` | Comparación de offsets (incluye `Z`, minutos, formatos inválidos) |
| `login_response_model_test.dart` | Parsing de `LoginResponseModel` |
| `hora_servidor_model_test.dart` | Parsing y defaults de `HoraServidorModel` |

Ejecución:
```powershell
flutter test
```

Análisis estático:
```powershell
flutter analyze
```

---

## 10. Construcción del APK

```powershell
cd app_universidad
flutter pub get
flutter build apk --release
```

- Salida: `app_universidad\build\app\outputs\flutter-apk\app-release.apk` (~47,9 MB).
- `AndroidManifest.xml` principal: permiso `INTERNET` y `usesCleartextTraffic="true"` (permite HTTP en producción — para despliegues con HTTPS conviene revisarlo).
- `build.gradle.kts`: `applicationId = "com.example.app_universidad"`; el build **release** usa actualmente las llaves de **debug** (pendiente configurar firma propia antes de distribuir).

---

## 11. Notas y pendientes

- La sesión se mantiene **solo en memoria** (`ApiClient`): al cerrar/abrir la app se vuelve a iniciar sesión. No se usa `shared_preferences`/`secure_storage`.
- La capa `domain`/`data` de `registro_*` es legado y no se invoca desde las pantallas.
- La subcarpeta `app_universidad\app_universidad\` es un proyecto duplicado y obsoleto; conviene eliminarla para evitar confusiones.
- Para probar en dispositivo físico, la `baseUrl` debe apuntar a la IP de la máquina donde corre la API (misma red), no a `localhost`.
