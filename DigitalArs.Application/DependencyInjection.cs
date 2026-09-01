using System.Reflection;
using FluentValidation;
using FluentValidation.AspNetCore;
using Mapster;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;

namespace DigitalArs.Application;

/// <summary>
/// Registro centralizado de los servicios de la capa de aplicacion (HU-08):
/// mapeo con Mapster y validadores con FluentValidation. La capa Api solo llama
/// a services.AddApplication() en Program.cs.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // === Mapster ===
        // Escanea el ensamblado en busca de clases IRegister (MappingRegister) y
        // arma una configuracion global reutilizable, registrada como singleton.
        var mapsterConfig = TypeAdapterConfig.GlobalSettings;
        mapsterConfig.Scan(assembly);
        services.AddSingleton(mapsterConfig);
        services.AddScoped<IMapper, ServiceMapper>();

        // === FluentValidation ===
        services.AddFluentValidationAutoValidation();
        // Requiere el paquete FluentValidation.AspNetCore
        // Registra todos los AbstractValidator<T> del ensamblado.
        services.AddValidatorsFromAssembly(assembly);


        return services;
    }
}
