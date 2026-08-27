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

        // Indice unico en UserId (refuerza relacion 1:1 con User)
        builder.HasIndex(a => a.UserId)
            .IsUnique();

        // Relacion User 1:1 Account
        builder.HasOne(a => a.User)
            .WithOne(u => u.Account)
            .HasForeignKey<Account>(a => a.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // La relacion Account 1:N Transaction se configura desde TransactionConfiguration
    }
}
