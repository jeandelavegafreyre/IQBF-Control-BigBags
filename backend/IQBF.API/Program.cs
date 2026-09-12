using System.Text;
using IQBF.Application;
using IQBF.Application.Interfaces;
using IQBF.API.Hubs;
using IQBF.API.Middleware;
using IQBF.API.Security;
using IQBF.API.Seed;
using IQBF.Domain.Entities;
using IQBF.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

var jwtKey = builder.Configuration["Jwt:Key"]
    ?? "IQBF-DESIGN-TIME-KEY-ONLY-NOT-FOR-PRODUCTION-123456789";

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSignalR();
builder.Services.AddSingleton<OperationsConnectionRegistry>();
builder.Services.AddSingleton<IUserSessionRevoker>(serviceProvider =>
    serviceProvider.GetRequiredService<OperationsConnectionRegistry>());
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<IAuthService, AuthService>();

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        if (allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        }
        else
        {
            // En desarrollo/Codespaces el frontend se sirve desde otro origen
            // (por ejemplo, el puerto 5173). Si no se configuraron orígenes
            // explícitos, permitimos cualquier origen para facilitar el entorno
            // local. En producción se deben definir Cors:AllowedOrigins.
            policy.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
    });
});

var issuer = builder.Configuration["Jwt:Issuer"];
var audience = builder.Configuration["Jwt:Audience"];

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.FromMinutes(1)
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/operations"))
                    context.Token = accessToken;
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "IQBF Control API",
        Version = "v1",
        Description = "API empresarial para Control IQBF - Recepción vs Despacho - Big Bags."
    });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingrese el token JWT."
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("Frontend");

var photoStoragePath = Path.Combine(AppContext.BaseDirectory, "storage", "photos");
Directory.CreateDirectory(photoStoragePath);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(photoStoragePath),
    RequestPath = "/photos"
});

app.UseAuthentication();
// Revalida contra la base de datos cada JWT autenticado. Esto hace que una
// desactivación o cambio de rol invalide inmediatamente las sesiones antiguas.
app.UseMiddleware<ActiveUserMiddleware>();
app.UseAuthorization();

app.MapControllers();
app.MapHub<OperationsHub>("/hubs/operations");

await AdminSeeder.SeedAsync(app.Services, app.Configuration);

app.Run();
