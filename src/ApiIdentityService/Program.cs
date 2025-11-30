using System.Text;
using ApiIdentityService.Application.Security;
using ApiIdentityService.Application.Services;
using ApiIdentityService.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// =====================
// Configuración de PostgreSQL
// =====================
var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? builder.Configuration["DATABASE_URL"]; // Render → env var

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

// =====================
// Servicios de aplicación
// =====================
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IMfaService, MfaService>();

// =====================
// Configuración de JWT
// =====================
var jwtSecret = builder.Configuration["Jwt:SecretKey"] ?? "super_dev_secret_1234567890";
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));

builder
    .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            ClockSkew = TimeSpan.FromMinutes(1),
        };
    });

// =====================
// Autorización basada en claims
// =====================
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(
        "InventoryRead",
        policy => policy.RequireClaim("permission", "inventory.read")
    );
});

// =====================
// CORS para el frontend
// =====================
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowLocalhostFrontend",
        policy =>
            policy
                .WithOrigins("http://localhost:3000") // Cambia según el puerto de tu frontend
                .AllowAnyHeader()
                .AllowAnyMethod()
    );
});

// =====================
// Controladores y Swagger
// =====================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "RBAC API", Version = "v1" });

    // 🔐 Seguridad JWT en Swagger
    c.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Ingrese el token JWT con el formato: Bearer {token}",
        }
    );

    c.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer",
                    },
                },
                Array.Empty<string>()
            },
        }
    );
});

var app = builder.Build();

// =====================
// Migración / creación de BD (solo desarrollo)
// =====================
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

// aplicar migraciones auto al arrancar

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// =====================
// Pipeline
// =====================
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "RBAC API v1");
    c.RoutePrefix = string.Empty; // Swagger disponible en http://localhost:5001/
});

// ⚠️ CORS debe ir antes de Authentication/Authorization
app.UseCors("AllowLocalhostFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
