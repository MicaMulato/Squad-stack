using DigitalArs.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalArs.Infrastructure.Data.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        // Identity ya configura la tabla como "AspNetRoles" con PK y Name
        // Aca configuramos solo las propiedades custom

        builder.Property(r => r.Description)
            .HasMaxLength(200);

        // La relacion Role 1:N User se configura desde UserConfiguration
    }
}
