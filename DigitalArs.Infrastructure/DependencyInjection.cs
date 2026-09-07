using DigitalArs.Application.Interfaces;
using DigitalArs.Application.Settings;
using DigitalArs.Domain.Entities;
using DigitalArs.Infrastructure.Data;
using DigitalArs.Infrastructure.Repositories;
using DigitalArs.Infrastructure.Security;
using DigitalArs.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DigitalArs.Infrastructure;

/// <summary>
/// Registro centralizado de dependencias de la capa de Infraestructura (Clean Architecture):
/// Base de datos, Identity, Repositorios, Unit of Work y Servicios de Infraestructura.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // ============================================================
        // DbContext
        // ============================================================
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // ============================================================
        // Identity — habilita UserManager<User> / RoleManager<Role>
        // ============================================================
        services.AddIdentity<User, Role>(options =>
        {
            options.Password.RequireNonAlphanumeric = false;
        })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        // ============================================================
        // Configuración y Servicios de Seguridad
        // ============================================================
        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
        services.AddScoped<IPasswordHasher, PasswordHasherService>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

        // ============================================================
        // Repositorios genéricos y Unit of Work
        // ============================================================
        services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // ============================================================
        // Configuración de Opciones y Servicios de Negocio / Infraestructura
        // ============================================================
        services.Configure<DepositSettings>(configuration.GetSection("DepositSettings"));
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<ITransactionService, TransactionService>();
        services.AddScoped<IFixedTermDepositService, FixedTermDepositService>();
        services.AddScoped<ICardService, CardService>();

        return services;
    }
}
