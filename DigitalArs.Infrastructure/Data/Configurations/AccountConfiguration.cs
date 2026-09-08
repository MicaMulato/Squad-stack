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

        // CVU único de 22 dígitos
        builder.Property(a => a.Cvu)
            .IsRequired()
            .HasMaxLength(22);

        builder.HasIndex(a => a.Cvu)
            .IsUnique();

        // Alias único
        builder.Property(a => a.Alias)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(a => a.Alias)
            .IsUnique();

        // Indice unico en UserId (refuerza relacion 1:1 con User)
        builder.HasIndex(a => a.UserId)
            .IsUnique();

        // Relacion User 1:1 Account
        builder.HasOne(a => a.User)
            .WithOne(u => u.Account)
            .HasForeignKey<Account>(a => a.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Filtro global coincidente con el de User: una cuenta se oculta cuando su
        // usuario esta dado de baja logica.
        builder.HasQueryFilter(a => !a.User!.IsDeleted);

        // === Data Seeding ===
        builder.HasData(
            new Account
            {
                Id = 1,
                UserId = 1, // Admin
                Money = 500000.00m,
                IsBlocked = false,
                Cvu = "0000003100010000000001",
                Alias = "admin.digital.ars",
                CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Account
            {
                Id = 2,
                UserId = 2, // Roberto Carlos
                Money = 260000.00m,
                IsBlocked = false,
                Cvu = "0000003100010000000002",
                Alias = "roberto.carlos.ars",
                CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Account
            {
                Id = 3,
                UserId = 3, // Mohammed Khan
                Money = 185000.50m,
                IsBlocked = false,
                Cvu = "0000003100010000000003",
                Alias = "mohammed.khan.ars",
                CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Account
            {
                Id = 4,
                UserId = 4, // Alejandro Silva
                Money = 45230.50m,
                IsBlocked = false,
                Cvu = "0000003100010000000004",
                Alias = "alejandro.silva.ars",
                CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Account
            {
                Id = 5,
                UserId = 5, // Micaela Mulato
                Money = 320000.00m,
                IsBlocked = false,
                Cvu = "0000003100010000000005",
                Alias = "micaela.mulato.ars",
                CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Account
            {
                Id = 6,
                UserId = 6, // Emmanuel Torres
                Money = 410000.00m,
                IsBlocked = false,
                Cvu = "0000003100010000000006",
                Alias = "emmanuel.torres.ars",
                CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}
