using DigitalArs.Api.Middlewares;
using DigitalArs.Application;
using DigitalArs.Infrastructure;
using DigitalArs.Infrastructure.Data;
using DigitalArs.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Reflection;
using System.Text;

namespace DigitalArs.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ============================================================
            // Controladores
            // ============================================================
            builder.Services.AddControllers();

            // ============================================================
            // Configuración de CORS (HU-20)
            // ============================================================
            var corsPolicyName = "AllowFrontendDev";
            var allowedOrigins = builder.Configuration
                .GetSection("Cors:AllowedOrigins")
                .Get<string[]>()
                ?? new[] { "http://localhost:5173", "http://localhost:3000" };

            builder.Services.AddCors(options =>
            {
                options.AddPolicy(corsPolicyName, policy =>
                {
                    policy.WithOrigins(allowedOrigins)
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                });
            });

            // ============================================================
            // OpenAPI / Swagger con soporte JWT Bearer (HU-19)
            // ============================================================
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "DigitalArs API",
                    Version = "v1",
                    Description = "API REST de billetera virtual - Squad Stack (Alkymer)",
                    Contact = new OpenApiContact
                    {
                        Name = "Squad Stack Team",
                        Url = new Uri("https://github.com/MicaMulato/Squad-stack")
                    }
                });

                // Configuración de esquema Bearer JWT para el botón Authorize (HU-19)
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Ingrese su token JWT (no es necesario escribir 'Bearer ')"
                });

                options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
                });

                // Incluir documentación XML de la API
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    options.IncludeXmlComments(xmlPath);
                }
            });
            builder.Services.AddOpenApi();

            // ============================================================
            // Inyección de Dependencias por Capas (Clean Architecture)
            // ============================================================
            builder.Services.AddApplication();
            builder.Services.AddInfrastructure(builder.Configuration);

            // ============================================================
            // Autenticación JWT y Autorización
            // ============================================================
            var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
            var secretKey = Encoding.UTF8.GetBytes(jwtSettings!.SecretKey);

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(secretKey),
                    ClockSkew = TimeSpan.Zero
                };
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"[JWT FAILED] Error: {context.Exception.Message}");
                        Console.ResetColor();
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"[JWT CHALLENGE] Error: {context.Error}, Desc: {context.ErrorDescription}");
                        Console.ResetColor();
                        return Task.CompletedTask;
                    }
                };
            });

            // [Authorize] por defecto en toda la API
            builder.Services.AddAuthorization(options =>
            {
                options.FallbackPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
            });

            var app = builder.Build();

            // Manejo global de excepciones (HU-18)
            app.UseGlobalExceptionHandler();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                try
                {
                    using var scope = app.Services.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    db.Database.Migrate();
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"[DB MIGRATION] Advertencia al migrar: {ex.Message}");
                    Console.ResetColor();
                }

                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "DigitalArs API v1");
                    c.RoutePrefix = "swagger"; // Accesible en /swagger
                });
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            // CORS (HU-20) — debe ejecutarse antes de UseAuthentication y UseAuthorization
            app.UseCors(corsPolicyName);

            // UseAuthentication SIEMPRE antes de UseAuthorization.
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
