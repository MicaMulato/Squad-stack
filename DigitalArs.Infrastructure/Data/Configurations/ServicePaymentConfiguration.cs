using DigitalArs.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalArs.Infrastructure.Data.Configurations;

public class ServicePaymentConfiguration : IEntityTypeConfiguration<ServicePayment>
{
    public void Configure(EntityTypeBuilder<ServicePayment> builder)
    {
        builder.ToTable("ServicePayments");

        builder.HasKey(sp => sp.Id);

        builder.Property(sp => sp.ReferenceNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(sp => sp.Amount)
            .HasPrecision(18, 2);

        builder.Property(sp => sp.ReceiptNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(sp => sp.PaymentDate)
            .HasDefaultValueSql("GETUTCDATE()");

        // Filtro global coincidente: solo pagos de usuarios activos
        builder.HasQueryFilter(sp => !sp.Account.User!.IsDeleted);

        builder.HasOne(sp => sp.Account)
            .WithMany(a => a.ServicePayments)
            .HasForeignKey(sp => sp.AccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(sp => sp.ServiceProvider)
            .WithMany(p => p.Payments)
            .HasForeignKey(sp => sp.ServiceProviderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(sp => sp.Transaction)
            .WithOne()
            .HasForeignKey<ServicePayment>(sp => sp.TransactionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(sp => sp.Reserve)
            .WithMany(r => r.ServicePayments)
            .HasForeignKey(sp => sp.ReserveId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
