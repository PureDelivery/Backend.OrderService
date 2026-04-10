using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderService.Domain.Entities;

namespace OrderService.Infrastructure.EntityConfigurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id)
            .IsRequired();

        builder.Property(o => o.CustomerId)
            .IsRequired();

        builder.Property(o => o.CustomerName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(o => o.CustomerEmail)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(o => o.CustomerPhone)
            .HasMaxLength(50);

        builder.Property(o => o.RestaurantId)
            .IsRequired();

        builder.Property(o => o.RestaurantName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(o => o.DeliveryAddress)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(o => o.DeliveryLatitude)
            .HasPrecision(18, 8);

        builder.Property(o => o.DeliveryLongitude)
            .HasPrecision(18, 8);

        builder.Property(o => o.DeliveryInstructions)
            .HasMaxLength(1000);

        builder.Property(o => o.PaymentMethod)
            .IsRequired();

        builder.Property(o => o.PaymentStatus)
            .IsRequired();

        builder.Property(o => o.Status)
            .IsRequired();

        builder.Property(o => o.DeliveryStatus)
            .IsRequired();

        builder.Property(o => o.SubTotal)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(o => o.DeliveryFee)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(o => o.Tax)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(o => o.Discount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(o => o.TotalAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(o => o.CreatedAt)
            .IsRequired();

        builder.Property(o => o.UpdatedAt)
            .IsRequired();

        builder.Property(o => o.SessionId)
            .HasMaxLength(200);

        builder.Property(o => o.SpecialInstructions)
            .HasMaxLength(1000);

        builder.Property(o => o.CancellationReason)
            .HasMaxLength(500);

        // Indexes
        builder.HasIndex(o => o.CustomerId);
        builder.HasIndex(o => o.RestaurantId);
        builder.HasIndex(o => o.Status);
        builder.HasIndex(o => o.CreatedAt);
        builder.HasIndex(o => o.SessionId);
    }
}

