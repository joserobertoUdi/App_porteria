# Cargar variables de entorno desde .env
$envFile = "ServiciosGenerales\ServiciosGenerales\.env"
Get-Content $envFile | ForEach-Object {
    $line = $_.Trim()
    if ($line -and -not $line.StartsWith('#') -and $line -match '^\s*(\S+?)\s*=\s*(.*?)\s*$') {
        [Environment]::SetEnvironmentVariable($matches[1], $matches[2], 'Process')
    }
}

$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:ASPNETCORE_URLS = "http://0.0.0.0:5169"

Write-Host "=== Variables de entorno cargadas ===" -ForegroundColor Cyan
Write-Host "  DB_CONNECTION_STRING: $( $env:DB_CONNECTION_STRING.Substring(0, [Math]::Min(50, $env:DB_CONNECTION_STRING.Length)) )..."
Write-Host "  JWT_KEY: $env:JWT_KEY"
Write-Host "  JWT_ISSUER: $env:JWT_ISSUER"
Write-Host "  JWT_AUDIENCE: $env:JWT_AUDIENCE"
Write-Host "  SHAREPOINT_BASE_URL: $env:SHAREPOINT_BASE_URL"
Write-Host "  PROVIDER_KEY: $env:PROVIDER_KEY"
Write-Host ""
Write-Host "=== Iniciando API en http://0.0.0.0:5169 ===" -ForegroundColor Green
Write-Host ""

Set-Location "ServiciosGenerales\ServiciosGenerales\ServiciosGenerales.Api"
dotnet run --no-launch-profile
