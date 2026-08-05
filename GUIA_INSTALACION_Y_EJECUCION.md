# Instalación y ejecución — Sistema de Estacionamiento UDI

Esta guía permite instalar y ejecutar los tres componentes del sistema en Windows:

- **API:** .NET 8 + SQL Server, en `ServiciosGenerales\ServiciosGenerales`.
- **Panel administrativo:** Angular, en `panel_admin`.
- **Aplicación operativa:** Flutter, en `app_universidad`.

> La carpeta `app_universidad\app_universidad` es un scaffold duplicado y obsoleto. No se debe usar.

## 1. Arquitectura y puertos

| Componente | Puerto / salida por defecto | Dirección de API configurada |
|---|---|---|
| API .NET | `http://localhost:5169` | — |
| Swagger (solo Development) | `http://localhost:5169/swagger` | — |
| Panel Angular (desarrollo) | `http://localhost:4200` | `http://localhost:5169` |
| App Flutter Android | APK o dispositivo | `http://100.106.35.85:5169` actualmente |

El panel y la app se comunican con la API; la API se comunica con SQL Server. Inicie siempre SQL Server antes de la API.

## 2. Software requerido en el nuevo equipo

Instale y compruebe estas herramientas desde PowerShell:

```powershell
dotnet --list-sdks       # Debe incluir un SDK 8.x (o posterior compatible con net8.0)
node --version           # Recomendado: Node.js 20 LTS o posterior
npm.cmd --version
flutter --version        # Debe incluir Dart 3.10.8 o posterior
flutter doctor
sqlcmd -?
```

Requisitos complementarios:

- SQL Server Express, Developer o Standard, y opcionalmente SQL Server Management Studio (SSMS).
- Para compilar Android: Android Studio, Android SDK, licencia aceptada (`flutter doctor --android-licenses`) y JDK 17.
- Para publicar el panel: un servidor web como IIS o Nginx. Para desarrollo no hace falta.

## 3. Base de datos

Hay dos opciones. Use solo una.

### Opción A: restaurar la copia existente

Esta opción conserva los datos que estén en `UniversidadDB.bak`. Copie ese archivo al equipo nuevo.

1. Abra SSMS y conéctese a la instancia SQL Server.
2. Ejecute primero para conocer los nombres lógicos del backup:

```sql
RESTORE FILELISTONLY
FROM DISK = 'C:\ruta\al\proyecto\UniversidadDB.bak';
```

3. Restaure reemplazando `UniversidadDB` y `UniversidadDB_log` por los nombres devueltos y ajustando las rutas `.mdf` y `.ldf` a su instancia:

```sql
RESTORE DATABASE UniversidadDB
FROM DISK = 'C:\ruta\al\proyecto\UniversidadDB.bak'
WITH
  MOVE 'UniversidadDB' TO 'C:\Program Files\Microsoft SQL Server\MSSQL16.SQLEXPRESS\MSSQL\DATA\UniversidadDB.mdf',
  MOVE 'UniversidadDB_log' TO 'C:\Program Files\Microsoft SQL Server\MSSQL16.SQLEXPRESS\MSSQL\DATA\UniversidadDB_log.ldf',
  RECOVERY;
```

No use `WITH REPLACE` salvo que haya confirmado que se puede sobrescribir una base existente.

### Opción B: base nueva mediante migraciones

No cree manualmente las tablas. Al primer arranque la API aplica sus migraciones automáticamente. El usuario que ejecute la API debe tener permiso para crear la base `UniversidadDB` en la instancia SQL Server seleccionada.

Para una instalación local con LocalDB, la configuración incluida ya apunta a:

```text
Server=(localdb)\MSSQLLocalDB;Database=UniversidadDB;Trusted_Connection=True;TrustServerCertificate=True;
```

Para SQL Server Express local o un servidor remoto, configure `DB_CONNECTION_STRING` antes de iniciar la API. Ejemplo de autenticación Windows:

```powershell
$env:DB_CONNECTION_STRING = 'Server=localhost\SQLEXPRESS;Database=UniversidadDB;Trusted_Connection=True;TrustServerCertificate=True;'
```

Ejemplo de autenticación SQL:

```powershell
$env:DB_CONNECTION_STRING = 'Server=SERVIDOR_SQL;Database=UniversidadDB;User Id=usuario_api;Password=CAMBIAR;TrustServerCertificate=True;'
```

## 4. Configuración de la API

Abra una primera consola PowerShell en la raíz del proyecto y ejecute:

```powershell
cd .\ServiciosGenerales\ServiciosGenerales
dotnet restore
dotnet test .\ServiciosGenerales.sln
dotnet run --project .\ServiciosGenerales.Api --launch-profile http
```

La API queda disponible en `http://localhost:5169`; confirme con `http://localhost:5169/swagger`.

En el perfil **Development**, el archivo `ServiciosGenerales.Api\appsettings.Development.json` proporciona una clave JWT de desarrollo y los usuarios demo. No reutilice esos valores en producción.

### Variables de producción

En producción son obligatorias `DB_CONNECTION_STRING` y `JWT_KEY`. Defínalas para la sesión actual antes de ejecutar la API:

```powershell
$env:ASPNETCORE_ENVIRONMENT = 'Production'
$env:DB_CONNECTION_STRING = 'Server=SERVIDOR_SQL;Database=UniversidadDB;User Id=usuario_api;Password=CAMBIAR;TrustServerCertificate=True;'
$env:JWT_KEY = 'una-clave-aleatoria-unica-de-al-menos-32-caracteres'
$env:JWT_ISSUER = 'UDI'
$env:JWT_AUDIENCE = 'app-universidad'
$env:JWT_EXPIRE_MINUTES = '180'
$env:JWT_REFRESH_TOKEN_DAYS = '7'
$env:AUTH_MAX_INTENTOS_FALLIDOS = '5'
$env:AUTH_DURACION_BLOQUEO_MIN = '15'
$env:SEED_ADMIN_PASSWORD = 'CAMBIAR-por-una-contrasena-segura'
$env:SEED_USUARIOS_DEMO = 'false'

dotnet run --project .\ServiciosGenerales.Api
```

Para persistir una variable para futuros procesos se puede usar `setx NOMBRE "valor"`, pero debe abrir una consola nueva después. No guarde claves ni contraseñas en el repositorio.

La API ejecuta `Database.Migrate()` al iniciar. Antes de iniciar contra una base con información real, realice un respaldo. El seed solo se ejecuta si la tabla de usuarios está vacía.

### Publicar la API

```powershell
cd .\ServiciosGenerales\ServiciosGenerales
dotnet publish .\ServiciosGenerales.Api -c Release -o C:\udi\api
cd C:\udi\api
.\ServiciosGenerales.Api.exe
```

En producción hospédela detrás de IIS/reverse proxy con HTTPS y configure allí las variables de entorno. Swagger no se habilita en `Production`.

## 5. Configuración y arranque del panel administrativo

La URL de la API para el panel está en:

```text
panel_admin\src\app\core\constants\api.constants.ts
```

Para trabajar todo en un mismo equipo, mantenga:

```typescript
baseUrl: 'http://localhost:5169'
```

Si la API está en otro servidor, cambie el valor por su dirección, por ejemplo `http://192.168.1.10:5169` o `https://api.midominio.edu`, sin barra final.

Abra una segunda consola desde la raíz y ejecute:

```powershell
cd .\panel_admin
npm.cmd ci
npm.cmd start
```

Abra `http://localhost:4200` en el navegador.

Para exponer temporalmente el panel a la red local:

```powershell
cd .\panel_admin
npm.cmd start -- --host 0.0.0.0
```

Si el panel se abre desde otro origen (por ejemplo `http://192.168.1.10:4200`), la política CORS actual de la API no lo autoriza: agregue ese origen en `ServiciosGenerales.Api\Program.cs`, dentro de `WithOrigins(...)`, reinicie la API y no use `AllowAnyOrigin()` junto con credenciales.

Compilación de producción del panel:

```powershell
cd .\panel_admin
npm.cmd ci
npm.cmd run build
```

Los archivos estáticos se generan en `panel_admin\dist\panel_admin\browser`. Publíquelos en IIS/Nginx y permita rutas Angular SPA (redirigir rutas no físicas a `index.html`).

## 6. Configuración y ejecución de la aplicación Flutter

La URL de la API está en:

```text
app_universidad\lib\core\constants\api_constants.dart
```

Ajuste `ApiConstants.baseUrl` antes de compilar:

| Escenario | Valor de `baseUrl` |
|---|---|
| Emulador Android y API en la misma PC | `http://10.0.2.2:5169` |
| Teléfono físico en la misma red | `http://IP_LAN_DEL_SERVIDOR:5169` |
| Servidor publicado | `https://api.midominio.edu` |

No use `localhost` desde un teléfono o emulador: apunta al propio dispositivo. La aplicación permite HTTP mediante `usesCleartextTraffic="true"`; para producción se recomienda HTTPS.

En una tercera consola ejecute:

```powershell
cd .\app_universidad
flutter pub get
flutter analyze
flutter test
flutter devices
flutter run
```

Para crear el APK Android de distribución:

```powershell
cd .\app_universidad
flutter build apk --release
```

El resultado es `app_universidad\build\app\outputs\flutter-apk\app-release.apk`. Actualmente la compilación release usa la llave de depuración. Antes de distribuir fuera de pruebas, cambie `applicationId` (`com.example.app_universidad`) y configure un keystore de release propio en `android\app\build.gradle.kts`.

## 7. Usuarios iniciales

Estos usuarios se crean solamente en una base vacía al ejecutar con `ASPNETCORE_ENVIRONMENT=Development` y la configuración incluida:

| Usuario | Contraseña | Rol |
|---|---|---|
| `admin` | `admin123` | Administrador |
| `portero.parqueo` | `portero123` | Portero de parqueo |
| `portero.porteria` | `portero123` | Portero de portería |

En producción, defina `SEED_ADMIN_PASSWORD` con una contraseña segura y `SEED_USUARIOS_DEMO=false` **antes del primer inicio**. Si se restaura una base que ya tiene usuarios, estos valores no crean ni cambian cuentas: use el panel administrativo o la base de datos según la política de administración definida.

## 8. Orden de inicio diario (desarrollo)

1. Inicie SQL Server / LocalDB.
2. En consola 1, ejecute la API:

   ```powershell
   cd .\ServiciosGenerales\ServiciosGenerales
   dotnet run --project .\ServiciosGenerales.Api --launch-profile http
   ```

3. En consola 2, inicie el panel:

   ```powershell
   cd .\panel_admin
   npm.cmd start
   ```

4. En consola 3, inicie Flutter en el dispositivo seleccionado:

   ```powershell
   cd .\app_universidad
   flutter run
   ```

5. Valide: Swagger responde, el panel abre en el puerto 4200, y la app puede iniciar sesión y registrar una entrada/salida.

## 9. Problemas frecuentes

| Síntoma | Acción |
|---|---|
| API indica que falta la cadena de conexión | Defina `DB_CONNECTION_STRING` o use LocalDB con la configuración incluida. |
| API indica que falta `JWT_KEY` | En Production defina `JWT_KEY`; en Development use el perfil `http`. |
| Panel o teléfono no conecta | Revise `baseUrl`, que la API escuche en 5169, firewall de Windows y conectividad de red. |
| Panel remoto recibe error CORS | Agregue el origen exacto del panel a `WithOrigins` en `Program.cs`. |
| App en emulador no conecta a `localhost` | Use `http://10.0.2.2:5169`. |
| `npm` no ejecuta por la política de PowerShell | Use `npm.cmd`, como en los comandos de esta guía. |
| Swagger no aparece | Arranque con Development; Swagger se deshabilita en Production. |
| Login bloqueado | Tras 5 intentos fallidos la cuenta se bloquea 15 minutos por defecto. |
| La API falla al aplicar una migración `CHECK` | Hay datos históricos con fechas incoherentes, por ejemplo una salida anterior a la entrada. Esta versión instala la restricción con `WITH NOCHECK`: protege nuevas inserciones/actualizaciones sin alterar los datos existentes. Ejecute `database\04_validar_integridad_y_normalizacion.sql`, corrija cada fila con criterio operativo y después valide las restricciones con `ALTER TABLE ... WITH CHECK CHECK CONSTRAINT ...`. |

## 10. Recuperación ante datos históricos con fechas inválidas

Al arrancar, la API ejecuta las migraciones pendientes. Si una restricción de fechas no puede aplicarse porque existen movimientos históricos inválidos, la API se detiene para proteger la integridad de la base.

La migración `EnforceIntegrityAndRestrictDeletes` se configuró con `WITH NOCHECK`: conserva los registros históricos y obliga a que **toda inserción o actualización futura** cumpla las reglas. La deuda histórica debe corregirse con información operativa real, no inventando fechas.

Para localizar registros de portería inválidos, ejecute en SSMS sobre `UniversidadDB`:

```sql
SELECT Id, UsuarioId, FechaEntrada, FechaSalida, PuertaEntrada, PuertaSalida
FROM dbo.RegistrosPorteria
WHERE FechaSalida IS NOT NULL
  AND FechaSalida < FechaEntrada
ORDER BY FechaEntrada;
```

Si una visita permanece abierta, se puede dejar su salida en `NULL`; si ya concluyó, registre la hora real de salida. Tras depurar todos los resultados de `database\04_validar_integridad_y_normalizacion.sql`, haga que SQL Server valide también los datos históricos:

```sql
ALTER TABLE dbo.RegistrosPorteria WITH CHECK CHECK CONSTRAINT CK_RegistrosPorteria_Fechas;
ALTER TABLE dbo.RegistrosParqueo WITH CHECK CHECK CONSTRAINT CK_RegistrosParqueo_Fechas;
ALTER TABLE dbo.RefreshTokens WITH CHECK CHECK CONSTRAINT CK_RefreshTokens_Fechas;
```

Después, inicie normalmente:

```powershell
cd ServiciosGenerales\ServiciosGenerales
dotnet run --project .\ServiciosGenerales.Api --launch-profile http
```
