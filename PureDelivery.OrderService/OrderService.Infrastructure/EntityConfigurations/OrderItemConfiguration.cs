using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderService.Domain.Entities;

namespace OrderService.Infrastructure.EntityConfigurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems");

        builder.HasKey(oi => oi.Id);

        builder.Property(oi => oi.Id)
            .IsRequired();

        builder.Property(oi => oi.OrderId)
            .IsRequired();

        builder.Property(oi => oi.MenuItemId)
            .IsRequired();

        builder.Property(oi => oi.MenuItemName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(oi => oi.MenuItemImageUrl)
            .HasMaxLength(500);

        builder.Property(oi => oi.UnitPrice)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(oi => oi.Quantity)
            .IsRequired();

        builder.Property(oi => oi.TotalPrice)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(oi => oi.SpecialInstructions)
            .HasMaxLength(500);

        builder.Property(oi => oi.CreatedAt)
            .IsRequired();

        // Indexes
        builder.HasIndex(oi => oi.OrderId);
        builder.HasIndex(oi => oi.MenuItemId);
    }
}

