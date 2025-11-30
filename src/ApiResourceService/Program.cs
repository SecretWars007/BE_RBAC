using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

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
// Autorización
// =====================
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(
        "InventoryRead",
        policy => policy.RequireClaim("permission", "inventory.read")
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

    // Configurar JWT Bearer
    c.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization", // Nombre del header
            Type = SecuritySchemeType.Http, // Tipo HTTP
            Scheme = "bearer", // Es un Bearer token
            BearerFormat = "JWT", // Formato JWT
            In = ParameterLocation.Header, // Se envía en el header
            Description = "Ingrese 'Bearer {token}' para autenticación",
        }
    );

    // Requerir JWT en todos los endpoints
    c.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer", // Debe coincidir con el SecurityDefinition
                    },
                },
                Array.Empty<string>() // scopes, no aplican en JWT simple
            },
        }
    );
});

var app = builder.Build();

// =====================
// Pipeline de middleware
// =====================
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "RBAC API v1");
    c.RoutePrefix = string.Empty; // Para que Swagger quede en la raíz: http://localhost:5001/
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
