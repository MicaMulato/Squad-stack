using DigitalArs.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalArs.Infrastructure.Data.Configurations;

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("Accounts");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Money)
            .HasPrecision(18, 2);

        builder.Property(a => a.IsBlocked)
            .HasDefaultValue(false);

        builder.Property(a => a.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        // Indice unico en UserId (refuerza relacion 1:1 con User)
        builder.HasIndex(a => a.UserId)
            .IsUnique();

        // Relacion User 1:1 Account
        builder.HasOne(a => a.User)
            .WithOne(u => u.Account)
            .HasForeignKey<Account>(a => a.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Filtro global coincidente con el de User: una cuenta se oculta cuando su
        // usuario esta dado de baja logica. Necesario porque User (extremo requerido
        // de la relacion 1:1) tiene HasQueryFilter(!IsDeleted); sin este filtro
        // coincidente EF advierte por posibles resultados inconsistentes.
        builder.HasQueryFilter(a => !a.User!.IsDeleted);

        // La relacion Account 1:N Transaction se configura desde TransactionConfiguration

        // === Data Seeding ===
        // Saldo inicial basado en sueldo minimo argentino (~$260.000 ARS aprox.)
        builder.HasData(
            new Account
            {
                Id = 1,
                UserId = 1, // Admin
                Money = 500000.00m,
                IsBlocked = false,
                CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Account
            {
                Id = 2,
                UserId = 2, // Roberto Carlos
                Money = 260000.00m,
                IsBlocked = false,
                CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Account
            {
                Id = 3,
                UserId = 3, // Mohammed Khan
                Money = 185000.50m,
                IsBlocked = false,
                CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Account
            {
                Id = 4,
                UserId = 4, // Alejandro Silva
                Money = 45230.50m,
                IsBlocked = false,
                CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Account
            {
                Id = 5,
                UserId = 5, // Micaela Mulato
                Money = 320000.00m,
                IsBlocked = false,
                CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Account
            {
                Id = 6,
                UserId = 6, // Emmanuel Torres
                Money = 410000.00m,
                IsBlocked = false,
                CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}
