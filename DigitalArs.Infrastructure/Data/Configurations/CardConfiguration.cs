using DigitalArs.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalArs.Infrastructure.Data.Configurations;

public class CardConfiguration : IEntityTypeConfiguration<Card>
{
    public void Configure(EntityTypeBuilder<Card> builder)
    {
        builder.ToTable("Cards");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.HolderName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.CardNumber)
            .HasMaxLength(19)
            .IsRequired();

        builder.HasIndex(c => c.CardNumber)
            .IsUnique();

        builder.Property(c => c.SecurityCode)
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(c => c.ExpirationDate)
            .IsRequired();

        builder.Property(c => c.Type)
            .IsRequired();

        builder.Property(c => c.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(c => c.IsActive)
            .HasDefaultValue(true);

        builder.Property(c => c.IsFrozen)
            .HasDefaultValue(false);

        // Filtro global: solo tarjetas de usuarios no eliminados
        builder.HasQueryFilter(c => !c.Account.User!.IsDeleted);

        // Relación 1:N con Account
        builder.HasOne(c => c.Account)
            .WithMany(a => a.Cards)
            .HasForeignKey(c => c.AccountId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
