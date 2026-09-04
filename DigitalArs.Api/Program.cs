using System.Reflection;
using System.Text;
using DigitalArs.Api.Middlewares;
using DigitalArs.Application;
using DigitalArs.Application.Interfaces;
using DigitalArs.Application.Services;
using DigitalArs.Domain.Entities;
using DigitalArs.Infrastructure.Data;
using DigitalArs.Infrastructure.Repositories;
using DigitalArs.Infrastructure.Security;
using DigitalArs.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

namespace DigitalArs
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();

            // ============================================================
            // Configuración de CORS (HU-20)
            // ============================================================
            const string corsPolicyName = "AllowFrontend";
            var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                ?? new[] { "http://localhost:5173" };

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
            // Swagger / OpenAPI (HU-19)
            // ============================================================
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "DigitalArs API - Billetera Virtual",
                    Version = "v1",
                    Description = "API REST de DigitalArs para gestión de usuarios, cuentas bancarias, depósitos y transferencias monetarias.",
                    Contact = new OpenApiContact
                    {
                        Name = "Equipo DigitalArs",
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
            // DbContext
            // ============================================================
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Identity — habilita UserManager<User> / RoleManager<Role>
            builder.Services.AddIdentity<User, Role>(options =>
            {
                // Config minima para desarrollo, ajustar cuando la situacion lo requiera
                options.Password.RequireNonAlphanumeric = false;
            })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            //Configuracion de JWT
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

            //[Authorize] por defecto
            builder.Services.AddAuthorization(options =>
            {
                options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
            });

            // Servicio de hashing de contraseñas
            builder.Services.AddScoped<IPasswordHasher, PasswordHasherService>();

            //Mapea "JwtSettings" del appsettings.json a la clase JwtSettings
            builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

            // Token Generator y Servicio de Autenticación
            builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
            builder.Services.AddScoped<IAuthService, AuthService>();

            // ============================================================
            // Repository generico + Unit of Work
            // (solo para entidades que heredan BaseEntity: Account, Transaction, etc.
            //  User y Role se manejan con UserManager/RoleManager, no por aca)
            // ============================================================
            builder.Services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddApplication(); // ← registra Mapster + FluentValidation

            // ============================================================
            // Servicios de aplicación
            // ============================================================
            builder.Services.Configure<DigitalArs.Application.Settings.DepositSettings>(
                builder.Configuration.GetSection("DepositSettings"));
            builder.Services.AddScoped<DigitalArs.Application.Interfaces.IAccountService,
                DigitalArs.Infrastructure.Services.AccountService>();
            builder.Services.AddScoped<DigitalArs.Application.Interfaces.ITransactionService,
                DigitalArs.Infrastructure.Services.TransactionService>();

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
