using DigitalArs.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalArs.Infrastructure.Data.Configurations;

public class FixedTermDepositConfiguration : IEntityTypeConfiguration<FixedTermDeposit>
{
    public void Configure(EntityTypeBuilder<FixedTermDeposit> builder)
    {
        builder.ToTable("FixedTermDeposits");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(f => f.InterestRate)
            .HasPrecision(5, 2)
            .IsRequired();

        builder.Property(f => f.InterestEarned)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(f => f.FinalAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(f => f.CreationDate)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(f => f.ClosingDate)
            .IsRequired();

        builder.Property(f => f.Status)
            .IsRequired();

        // Filtro global para usuarios activos
        builder.HasQueryFilter(f => !f.Account.User!.IsDeleted);

        // Relación 1:N con Account
        builder.HasOne(f => f.Account)
            .WithMany(a => a.FixedTermDeposits)
            .HasForeignKey(f => f.AccountId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
