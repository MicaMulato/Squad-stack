using DigitalArs.Domain.Entities;
using DigitalArs.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalArs.Infrastructure.Data.Configurations;

public class ServiceProviderConfiguration : IEntityTypeConfiguration<ServiceProvider>
{
    public void Configure(EntityTypeBuilder<ServiceProvider> builder)
    {
        builder.ToTable("ServiceProviders");

        builder.HasKey(sp => sp.Id);

        builder.Property(sp => sp.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(sp => sp.CodeLabel)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(sp => sp.IconName)
            .HasMaxLength(50);

        builder.Property(sp => sp.IsActive)
            .HasDefaultValue(true);

        builder.HasData(
            // Electricidad (Luz)
            new ServiceProvider { Id = 1, Name = "Edenor", Category = ServiceCategory.Electricity, CodeLabel = "Número de Cuenta / Cliente (10 dígitos)", IconName = "bolt", IsActive = true },
            new ServiceProvider { Id = 2, Name = "Edesur", Category = ServiceCategory.Electricity, CodeLabel = "Número de Cliente (8 dígitos)", IconName = "bolt", IsActive = true },
            new ServiceProvider { Id = 3, Name = "EPEC Córdoba", Category = ServiceCategory.Electricity, CodeLabel = "Código de Pago Electrónico (14 dígitos)", IconName = "bolt", IsActive = true },
            // Agua
            new ServiceProvider { Id = 4, Name = "AySA", Category = ServiceCategory.Water, CodeLabel = "Número de Cuenta de Servicios (10 dígitos)", IconName = "water_drop", IsActive = true },
            new ServiceProvider { Id = 5, Name = "Aguas Cordobesas", Category = ServiceCategory.Water, CodeLabel = "Código de Unidad de Facturación (8 dígitos)", IconName = "water_drop", IsActive = true },
            // Gas
            new ServiceProvider { Id = 6, Name = "Metrogas", Category = ServiceCategory.Gas, CodeLabel = "Número de Referencia de Pago (11 dígitos)", IconName = "local_fire_department", IsActive = true },
            new ServiceProvider { Id = 7, Name = "Naturgy", Category = ServiceCategory.Gas, CodeLabel = "Número de Cuenta de Factura (10 dígitos)", IconName = "local_fire_department", IsActive = true },
            new ServiceProvider { Id = 8, Name = "Camuzzi Gas", Category = ServiceCategory.Gas, CodeLabel = "Código Link / Banelco (12 dígitos)", IconName = "local_fire_department", IsActive = true },
            // Telefonía / Internet
            new ServiceProvider { Id = 9, Name = "Claro", Category = ServiceCategory.TelephonyAndInternet, CodeLabel = "Número de Línea (10 dígitos con código de área)", IconName = "phone_iphone", IsActive = true },
            new ServiceProvider { Id = 10, Name = "Personal Flow", Category = ServiceCategory.TelephonyAndInternet, CodeLabel = "Número de Línea o Código de Pago (10 dígitos)", IconName = "phone_iphone", IsActive = true },
            new ServiceProvider { Id = 11, Name = "Movistar", Category = ServiceCategory.TelephonyAndInternet, CodeLabel = "Número de Celular o Cuenta (10 dígitos)", IconName = "phone_iphone", IsActive = true },
            new ServiceProvider { Id = 12, Name = "Telecentro", Category = ServiceCategory.TelephonyAndInternet, CodeLabel = "Número de Cliente (8 dígitos)", IconName = "router", IsActive = true },
            // Impuestos
            new ServiceProvider { Id = 13, Name = "ARCA / AFIP (VEP)", Category = ServiceCategory.Taxes, CodeLabel = "Número de VEP (Volante Electrónico de Pago)", IconName = "account_balance", IsActive = true },
            new ServiceProvider { Id = 14, Name = "AGIP Rentas CABA", Category = ServiceCategory.Taxes, CodeLabel = "Código de Pago Electrónico / Partida", IconName = "receipt", IsActive = true },
            new ServiceProvider { Id = 15, Name = "ARBA Buenos Aires", Category = ServiceCategory.Taxes, CodeLabel = "Código de Pago Electrónico (14 dígitos)", IconName = "receipt", IsActive = true }
        );
    }
}
