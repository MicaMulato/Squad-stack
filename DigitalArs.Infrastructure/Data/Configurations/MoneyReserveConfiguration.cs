using DigitalArs.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalArs.Infrastructure.Data.Configurations;

public class MoneyReserveConfiguration : IEntityTypeConfiguration<MoneyReserve>
{
    public void Configure(EntityTypeBuilder<MoneyReserve> builder)
    {
        builder.ToTable("MoneyReserves");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(r => r.CurrentBalance)
            .HasPrecision(18, 2)
            .HasDefaultValue(0m);

        builder.Property(r => r.TargetAmount)
            .HasPrecision(18, 2);

        builder.Property(r => r.Icon)
            .HasMaxLength(50);

        builder.Property(r => r.Color)
            .HasMaxLength(20);

        builder.Property(r => r.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(r => r.IsActive)
            .HasDefaultValue(true);

        // Filtro global coincidente: solo reservas de usuarios activos
        builder.HasQueryFilter(r => !r.Account.User!.IsDeleted);

        builder.HasOne(r => r.Account)
            .WithMany(a => a.Reserves)
            .HasForeignKey(r => r.AccountId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
