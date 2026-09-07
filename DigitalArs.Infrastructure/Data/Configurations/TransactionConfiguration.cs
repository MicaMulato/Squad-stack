using DigitalArs.Domain.Entities;
using DigitalArs.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalArs.Infrastructure.Data.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("Transactions");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Amount)
            .HasPrecision(18, 2);

        builder.Property(t => t.Type)
            .IsRequired();

        builder.Property(t => t.Concept)
            .HasMaxLength(200);

        builder.Property(t => t.Date)
            .HasDefaultValueSql("GETUTCDATE()");

        // Indice en Date para consultas por rango de fechas 
        builder.HasIndex(t => t.Date);

        // Filtro global coincidente en cascada con User/Account: una transaccion se
        // oculta cuando la cuenta origen pertenece a un usuario dado de baja logica.
        // Necesario porque Account (extremo requerido de esta relacion) tiene filtro
        // global; EF exige filtros coincidentes en ambos extremos requeridos.
        builder.HasQueryFilter(t => !t.Account!.User!.IsDeleted);

        // Relacion Account 1:N Transaction (cuenta origen)
        builder.HasOne(t => t.Account)
            .WithMany(a => a.Transactions)
            .HasForeignKey(t => t.AccountId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relacion Account 1:N Transaction (cuenta destino, nullable)
        // Cascade deshabilitado para evitar ciclos (una Account puede ser origen Y destino)
        builder.HasOne(t => t.ToAccount)
            .WithMany()
            .HasForeignKey(t => t.ToAccountId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        // === Data Seeding con Motivos Realistas de Billetera Virtual ===
        builder.HasData(
            new Transaction
            {
                Id = 1,
                AccountId = 4,
                ToAccountId = null,
                Amount = 43730.50m,
                Type = TransactionType.Deposit,
                Concept = "Haberes",
                Date = new DateTime(2026, 8, 1, 10, 0, 0, DateTimeKind.Utc)
            },
            new Transaction
            {
                Id = 2,
                AccountId = 2,
                ToAccountId = null,
                Amount = 239000.00m,
                Type = TransactionType.Deposit,
                Concept = "Haberes",
                Date = new DateTime(2026, 8, 1, 10, 0, 0, DateTimeKind.Utc)
            },
            new Transaction
            {
                Id = 3,
                AccountId = 3,
                ToAccountId = null,
                Amount = 193500.50m,
                Type = TransactionType.Deposit,
                Concept = "Haberes",
                Date = new DateTime(2026, 8, 1, 10, 0, 0, DateTimeKind.Utc)
            },
            new Transaction
            {
                Id = 4,
                AccountId = 5,
                ToAccountId = null,
                Amount = 340000.00m,
                Type = TransactionType.Deposit,
                Concept = "Haberes",
                Date = new DateTime(2026, 8, 1, 10, 0, 0, DateTimeKind.Utc)
            },
            new Transaction
            {
                Id = 5,
                AccountId = 6,
                ToAccountId = null,
                Amount = 404000.00m,
                Type = TransactionType.Deposit,
                Concept = "Haberes",
                Date = new DateTime(2026, 8, 1, 10, 0, 0, DateTimeKind.Utc)
            },
            // Alejandro (4) -> Roberto Carlos (2) $15,000.00: Alquiler
            new Transaction
            {
                Id = 6,
                AccountId = 4,
                ToAccountId = 2,
                Amount = 15000.00m,
                Type = TransactionType.TransferOut,
                Concept = "Alquiler",
                Date = new DateTime(2026, 8, 10, 14, 30, 0, DateTimeKind.Utc)
            },
            new Transaction
            {
                Id = 7,
                AccountId = 2,
                ToAccountId = 4,
                Amount = 15000.00m,
                Type = TransactionType.TransferIn,
                Concept = "Alquiler",
                Date = new DateTime(2026, 8, 10, 14, 30, 0, DateTimeKind.Utc)
            },
            // Micaela (5) -> Alejandro (4) $25,000.00: Honorarios
            new Transaction
            {
                Id = 8,
                AccountId = 5,
                ToAccountId = 4,
                Amount = 25000.00m,
                Type = TransactionType.TransferOut,
                Concept = "Honorarios profesionales",
                Date = new DateTime(2026, 8, 18, 11, 15, 0, DateTimeKind.Utc)
            },
            new Transaction
            {
                Id = 9,
                AccountId = 4,
                ToAccountId = 5,
                Amount = 25000.00m,
                Type = TransactionType.TransferIn,
                Concept = "Honorarios profesionales",
                Date = new DateTime(2026, 8, 18, 11, 15, 0, DateTimeKind.Utc)
            },
            // Alejandro (4) -> Emmanuel Torres (6) $12,000.00: Servicios
            new Transaction
            {
                Id = 10,
                AccountId = 4,
                ToAccountId = 6,
                Amount = 12000.00m,
                Type = TransactionType.TransferOut,
                Concept = "Cuentas y servicios",
                Date = new DateTime(2026, 8, 25, 16, 45, 0, DateTimeKind.Utc)
            },
            new Transaction
            {
                Id = 11,
                AccountId = 6,
                ToAccountId = 4,
                Amount = 12000.00m,
                Type = TransactionType.TransferIn,
                Concept = "Cuentas y servicios",
                Date = new DateTime(2026, 8, 25, 16, 45, 0, DateTimeKind.Utc)
            },
            // Alejandro (4) -> Micaela Mulato (5) $5,000.00: Comidas
            new Transaction
            {
                Id = 12,
                AccountId = 4,
                ToAccountId = 5,
                Amount = 5000.00m,
                Type = TransactionType.TransferOut,
                Concept = "Comidas y bebidas",
                Date = new DateTime(2026, 9, 1, 18, 20, 0, DateTimeKind.Utc)
            },
            new Transaction
            {
                Id = 13,
                AccountId = 5,
                ToAccountId = 4,
                Amount = 5000.00m,
                Type = TransactionType.TransferIn,
                Concept = "Comidas y bebidas",
                Date = new DateTime(2026, 9, 1, 18, 20, 0, DateTimeKind.Utc)
            },
            // Mohammed Khan (3) -> Alejandro (4) $8,500.00: Educacion
            new Transaction
            {
                Id = 14,
                AccountId = 3,
                ToAccountId = 4,
                Amount = 8500.00m,
                Type = TransactionType.TransferOut,
                Concept = "Educación",
                Date = new DateTime(2026, 9, 2, 10, 0, 0, DateTimeKind.Utc)
            },
            new Transaction
            {
                Id = 15,
                AccountId = 4,
                ToAccountId = 3,
                Amount = 8500.00m,
                Type = TransactionType.TransferIn,
                Concept = "Educación",
                Date = new DateTime(2026, 9, 2, 10, 0, 0, DateTimeKind.Utc)
            },
            // Emmanuel Torres (6) -> Roberto Carlos (2) $6,000.00: Transporte
            new Transaction
            {
                Id = 16,
                AccountId = 6,
                ToAccountId = 2,
                Amount = 6000.00m,
                Type = TransactionType.TransferOut,
                Concept = "Transporte",
                Date = new DateTime(2026, 9, 3, 9, 30, 0, DateTimeKind.Utc)
            },
            new Transaction
            {
                Id = 17,
                AccountId = 2,
                ToAccountId = 6,
                Amount = 6000.00m,
                Type = TransactionType.TransferIn,
                Concept = "Transporte",
                Date = new DateTime(2026, 9, 3, 9, 30, 0, DateTimeKind.Utc)
            }
        );
    }
}
