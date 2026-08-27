using DigitalArs.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalArs.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // Identity ya configura la tabla como "AspNetUsers" con PK, Email, PasswordHash, etc.
        // Aca configuramos solo las propiedades custom y relaciones adicionales

        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(u => u.IsDeleted)
            .HasDefaultValue(false);

        builder.Property(u => u.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        // Indice unico en Email (Identity no lo crea unico por defecto en la tabla)
        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasFilter("[Email] IS NOT NULL");

        // Relacion Role 1:N User (FK directa para simplificar consultas)
        builder.HasOne(u => u.Role)
            .WithMany(r => r.Users)
            .HasForeignKey(u => u.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        // === Data Seeding ===
        var hasher = new PasswordHasher<User>();

        // Admin: admin@digitalars.com / Admin123!
        var admin = new User
        {
            Id = 1,
            FirstName = "Admin",
            LastName = "DigitalArs",
            Email = "admin@digitalars.com",
            NormalizedEmail = "ADMIN@DIGITALARS.COM",
            UserName = "admin@digitalars.com",
            NormalizedUserName = "ADMIN@DIGITALARS.COM",
            EmailConfirmed = true,
            SecurityStamp = "SEED-ADMIN-SECURITY-STAMP",
            ConcurrencyStamp = "seed-admin-concurrency",
            RoleId = 1,
            IsDeleted = false,
            CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };
        admin.PasswordHash = hasher.HashPassword(admin, "Admin123!");

        // User 1: Roberto Carlos / robercarlos3@gmail.com / Roberto1!
        var user1 = new User
        {
            Id = 2,
            FirstName = "Roberto",
            LastName = "Carlos",
            Email = "robercarlos3@gmail.com",
            NormalizedEmail = "ROBERCARLOS3@GMAIL.COM",
            UserName = "robercarlos3@gmail.com",
            NormalizedUserName = "ROBERCARLOS3@GMAIL.COM",
            EmailConfirmed = true,
            SecurityStamp = "SEED-USER1-SECURITY-STAMP",
            ConcurrencyStamp = "seed-user1-concurrency",
            RoleId = 2,
            IsDeleted = false,
            CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };
        user1.PasswordHash = hasher.HashPassword(user1, "Roberto1!");

        // User 2: Mohammed Khan / mokha@gmail.com / Mohammed1!
        var user2 = new User
        {
            Id = 3,
            FirstName = "Mohammed",
            LastName = "Khan",
            Email = "mokha@gmail.com",
            NormalizedEmail = "MOKHA@GMAIL.COM",
            UserName = "mokha@gmail.com",
            NormalizedUserName = "MOKHA@GMAIL.COM",
            EmailConfirmed = true,
            SecurityStamp = "SEED-USER2-SECURITY-STAMP",
            ConcurrencyStamp = "seed-user2-concurrency",
            RoleId = 2,
            IsDeleted = false,
            CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };
        user2.PasswordHash = hasher.HashPassword(user2, "Mohammed1!");

        builder.HasData(admin, user1, user2);
    }
}
