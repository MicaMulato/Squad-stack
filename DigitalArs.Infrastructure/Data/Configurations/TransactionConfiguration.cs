using DigitalArs.Domain.Entities;
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
    }
}
