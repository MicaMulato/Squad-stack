using DigitalArs.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalArs.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.Password)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(u => u.IsDeleted)
            .HasDefaultValue(false);

        builder.Property(u => u.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        // Indice unico en Email 
        builder.HasIndex(u => u.Email)
            .IsUnique();

        // Relacion Role 1:N User
        builder.HasOne(u => u.Role)
            .WithMany(r => r.Users)
            .HasForeignKey(u => u.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relacion User 1:1 Account (configurada desde AccountConfiguration)
    }
}
