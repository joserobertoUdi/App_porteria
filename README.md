# App de Portería y Parqueo UDI

Proyecto completo del sistema de control de acceso y estacionamiento, compuesto por:

- API en .NET 8 / ASP.NET Core
- Panel administrativo en Angular
- Aplicación móvil en Flutter
- Base de datos SQL Server / LocalDB
- Documentación de despliegue e instalación

## Estructura principal

- `ServiciosGenerales/ServiciosGenerales/` — backend y lógica de negocio
- `panel_admin/` — panel administrativo web
- `app_universidad/` — aplicación móvil Flutter
- `database/` — scripts SQL y auditoría
- `UniversidadDB.bak` — respaldo de la base de datos del sistema
- `DESPLIEGUE.md` — guía de despliegue
- `GUIA_INSTALACION_Y_EJECUCION.md` — guía de instalación y ejecución
- `DOCUMENTACION_BACKEND.md` — documentación técnica del backend
- `DOCUMENTACION_FRONTEND.md` — documentación técnica del frontend
- `ROADMAP.md` — plan del proyecto

## Requisitos

### Backend
- .NET SDK 8+
- SQL Server LocalDB o SQL Server completo

### Panel web
- Node.js 20+
- Angular / npm

### App móvil
- Flutter SDK 3.x
- Android SDK

## Ejecución rápida

### 1. Base de datos
Restaurar o preparar la base `UniversidadDB` antes de arrancar la API.

### 2. API
```powershell
cd .\ServiciosGenerales\ServiciosGenerales
 dotnet restore
 dotnet run --project .\ServiciosGenerales.Api
```

### 3. Panel administrativo
```powershell
cd .\panel_admin
npm install
npm start
```

### 4. App Flutter
```powershell
cd .\app_universidad
flutter pub get
flutter run
```

## Documentación

- [DESPLIEGUE.md](DESPLIEGUE.md)
- [GUIA_INSTALACION_Y_EJECUCION.md](GUIA_INSTALACION_Y_EJECUCION.md)
- [DOCUMENTACION_BACKEND.md](DOCUMENTACION_BACKEND.md)
- [DOCUMENTACION_FRONTEND.md](DOCUMENTACION_FRONTEND.md)
- [ROADMAP.md](ROADMAP.md)

## Nota importante

La documentación de despliegue e instalación está incluida en este repositorio y es la que se refleja en la portada del proyecto para facilitar la puesta en marcha del sistema.
