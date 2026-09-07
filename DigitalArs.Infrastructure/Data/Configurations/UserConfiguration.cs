using DigitalArs.Domain.Entities;
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

        // Indice en IsDeleted: soporta el filtro global de soft-delete (WHERE IsDeleted = 0)
        builder.HasIndex(u => u.IsDeleted);

        // Filtro global de soft-delete: las consultas de User excluyen automaticamente
        // los usuarios dados de baja logica. Para incluirlos usar .IgnoreQueryFilters().
        builder.HasQueryFilter(u => !u.IsDeleted);

        // Relacion Role 1:N User (FK directa para simplificar consultas)
        builder.HasOne(u => u.Role)
            .WithMany(r => r.Users)
            .HasForeignKey(u => u.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        // === Data Seeding ===
        // Hashes pre-computados con PasswordHasher<User> de Identity (valores fijos para seed reproducible)
        // Credenciales: Admin123!, Roberto1!, Mohammed1!

        builder.HasData(
            new User
            {
                Id = 1,
                FirstName = "Admin",
                LastName = "DigitalArs",
                Email = "admin@digitalars.com",
                NormalizedEmail = "ADMIN@DIGITALARS.COM",
                UserName = "admin@digitalars.com",
                NormalizedUserName = "ADMIN@DIGITALARS.COM",
                EmailConfirmed = true,
                PasswordHash = "AQAAAAIAAYagAAAAENO87HGO7ibu/kR6bblZLBu39LF1P9oeSEu7bwGb0YRvny7KouBk+XFrlxztTvecMQ==",
                SecurityStamp = "SEED-ADMIN-SECURITY-STAMP",
                ConcurrencyStamp = "seed-admin-concurrency",
                RoleId = 1,
                IsDeleted = false,
                CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new User
            {
                Id = 2,
                FirstName = "Roberto",
                LastName = "Carlos",
                Email = "robercarlos3@gmail.com",
                NormalizedEmail = "ROBERCARLOS3@GMAIL.COM",
                UserName = "robercarlos3@gmail.com",
                NormalizedUserName = "ROBERCARLOS3@GMAIL.COM",
                EmailConfirmed = true,
                PasswordHash = "AQAAAAIAAYagAAAAELKOHh9OA8J6+VbOTSSbRWvcAbbwaPQ9dZcgFx02YAC1LeN/DPShnsiygednoXJUNQ==",
                SecurityStamp = "SEED-USER1-SECURITY-STAMP",
                ConcurrencyStamp = "seed-user1-concurrency",
                RoleId = 2,
                IsDeleted = false,
                CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new User
            {
                Id = 3,
                FirstName = "Mohammed",
                LastName = "Khan",
                Email = "mokha@gmail.com",
                NormalizedEmail = "MOKHA@GMAIL.COM",
                UserName = "mokha@gmail.com",
                NormalizedUserName = "MOKHA@GMAIL.COM",
                EmailConfirmed = true,
                PasswordHash = "AQAAAAIAAYagAAAAEErdfRgSxQZB1799p6YXG2T/bLL4bqGYPBRcYHsKas3tZrZUfw1cn6bK9oGtgvtkhA==",
                SecurityStamp = "SEED-USER2-SECURITY-STAMP",
                ConcurrencyStamp = "seed-user2-concurrency",
                RoleId = 2,
                IsDeleted = false,
                CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new User
            {
                Id = 4,
                FirstName = "Alejandro",
                LastName = "Silva",
                Email = "alejandro.silva@digitalars.com",
                NormalizedEmail = "ALEJANDRO.SILVA@DIGITALARS.COM",
                UserName = "alejandro.silva@digitalars.com",
                NormalizedUserName = "ALEJANDRO.SILVA@DIGITALARS.COM",
                EmailConfirmed = true,
                PasswordHash = "AQAAAAIAAYagAAAAEIF4BH6BgJcp+Hmu8tYbCiyDyfC8/R3A8lus7ILAex/9qAxhI8YRaq7+ERYrGrRrYg==",
                SecurityStamp = "SEED-USER4-SECURITY-STAMP",
                ConcurrencyStamp = "seed-user4-concurrency",
                RoleId = 2,
                IsDeleted = false,
                CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new User
            {
                Id = 5,
                FirstName = "Micaela",
                LastName = "Mulato",
                Email = "micaela.mulato@digitalars.com",
                NormalizedEmail = "MICAELA.MULATO@DIGITALARS.COM",
                UserName = "micaela.mulato@digitalars.com",
                NormalizedUserName = "MICAELA.MULATO@DIGITALARS.COM",
                EmailConfirmed = true,
                PasswordHash = "AQAAAAIAAYagAAAAEPlWIfeBvEa2UIgXOAlJgZkhZ9W+6n3zxsEIiVncqS9jY+6qmsMbOL+u+DeaM10S1w==",
                SecurityStamp = "SEED-USER5-SECURITY-STAMP",
                ConcurrencyStamp = "seed-user5-concurrency",
                RoleId = 2,
                IsDeleted = false,
                CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new User
            {
                Id = 6,
                FirstName = "Emmanuel",
                LastName = "Torres",
                Email = "emmanuel.torres@digitalars.com",
                NormalizedEmail = "EMMANUEL.TORRES@DIGITALARS.COM",
                UserName = "emmanuel.torres@digitalars.com",
                NormalizedUserName = "EMMANUEL.TORRES@DIGITALARS.COM",
                EmailConfirmed = true,
                PasswordHash = "AQAAAAIAAYagAAAAEISvy59LTmpoZ20JI0rhgziQdnw7hg1vq272APTffUCeMWtHb8rAl0V5Src75EltPA==",
                SecurityStamp = "SEED-USER6-SECURITY-STAMP",
                ConcurrencyStamp = "seed-user6-concurrency",
                RoleId = 2,
                IsDeleted = false,
                CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}
