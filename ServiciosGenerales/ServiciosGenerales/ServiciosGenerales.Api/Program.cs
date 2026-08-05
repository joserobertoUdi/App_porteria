using ServiciosGenerales.Api.Data;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ServiciosGenerales.Aplicacion;
using ServiciosGenerales.Aplicacion.Settings;
using ServiciosGenerales.Api.Middleware;
using ServiciosGenerales.Infraestructura;
using ServiciosGenerales.Infraestructura.Data;
var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetValue<string>("DB_CONNECTION_STRING")
    ?? builder.Configuration.GetConnectionString("CadenaConexionSQL")
    ?? throw new InvalidOperationException("Cadena de conexión no configurada.");

var jwtKey = builder.Configuration.GetValue<string>("JWT_KEY")
    ?? builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("JWT_KEY no configurada.");

var jwtIssuer = builder.Configuration.GetValue<string>("JWT_ISSUER")
    ?? builder.Configuration["Jwt:Issuer"]
    ?? "UDI";

var jwtAudience = builder.Configuration.GetValue<string>("JWT_AUDIENCE")
    ?? builder.Configuration["Jwt:Audience"]
    ?? "app-universidad";

var jwtExpireMinutes = builder.Configuration.GetValue<int?>("JWT_EXPIRE_MINUTES")
    ?? builder.Configuration.GetValue<int?>("Jwt:ExpireMinutes")
    ?? 180;

var refreshTokenExpireDays = builder.Configuration.GetValue<int?>("JWT_REFRESH_TOKEN_DAYS")
    ?? builder.Configuration.GetValue<int?>("Jwt:RefreshTokenExpireDays")
    ?? 7;

builder.Services.AddSingleton(new JwtSettings
{
    Key = jwtKey,
    Issuer = jwtIssuer,
    Audience = jwtAudience,
    ExpireMinutes = jwtExpireMinutes,
    RefreshTokenExpireDays = refreshTokenExpireDays,
});

var maxIntentosFallidos = builder.Configuration.GetValue<int?>("AUTH_MAX_INTENTOS_FALLIDOS")
    ?? builder.Configuration.GetValue<int?>("Auth:MaxIntentosFallidos")
    ?? 5;

var duracionBloqueoMinutos = builder.Configuration.GetValue<int?>("AUTH_DURACION_BLOQUEO_MIN")
    ?? builder.Configuration.GetValue<int?>("Auth:DuracionBloqueoMinutos")
    ?? 15;

builder.Services.AddSingleton(new AuthSettings
{
    MaxIntentosFallidos = maxIntentosFallidos,
    DuracionBloqueoMinutos = duracionBloqueoMinutos,
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddProblemDetails();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles)
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errores = context.ModelState
                .Where(m => m.Value?.Errors.Count > 0)
                .SelectMany(m => m.Value!.Errors.Select(e => e.ErrorMessage))
                .ToList();

            return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(
                new { error = string.Join(" ", errores) });
        };
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ServiciosGenerales API",
        Version = "v1",
        Description = "API del sistema de control de acceso y parqueo UDI.",
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingrese el token JWT: Bearer {token}",
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" },
            },
            Array.Empty<string>()
        },
    });

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirApp", policy =>
    {
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .WithOrigins(
                  "http://localhost:5169",
                  "http://localhost:8388",
                  "http://localhost:4200")
              .AllowCredentials();
    });
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddFixedWindowLimiter("login", opt =>
    {
        opt.PermitLimit = 20;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 0;
    });

    options.AddFixedWindowLimiter("global", opt =>
    {
        opt.PermitLimit = 300;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 0;
    });
});

builder.Services.AddInfraestructura(connectionString);
builder.Services.AddAplicacion();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<UniversidadDbContext>();
    context.Database.Migrate();
    await DatabaseSeeder.SeedAsync(context, builder.Configuration);
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("PermitirApp");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
