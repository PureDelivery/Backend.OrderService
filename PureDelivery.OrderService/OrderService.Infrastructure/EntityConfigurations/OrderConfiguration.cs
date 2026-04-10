using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderService.Domain.Entities;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");
        builder.HasKey(o => o.Id);

        builder.Property(o => o.RestaurantName).IsRequired().HasMaxLength(200);
        builder.Property(o => o.SessionId).HasMaxLength(200);
        builder.Property(o => o.CreatedAt).IsRequired();

        // Конфигурация Money как Owned Type
        builder.OwnsOne(o => o.Money, m =>
        {
            m.Property(p => p.SubTotal).HasColumnName("SubTotal").HasPrecision(18, 2);
            m.Property(p => p.DeliveryFee).HasColumnName("DeliveryFee").HasPrecision(18, 2);
            m.Property(p => p.Tax).HasColumnName("Tax").HasPrecision(18, 2);
            m.Property(p => p.Discount).HasColumnName("Discount").HasPrecision(18, 2);
            m.Property(p => p.TotalAmount).HasColumnName("TotalAmount").HasPrecision(18, 2);
        });

        // Конфигурация Address как Owned Type
        builder.OwnsOne(o => o.DeliveryAddress, a =>
        {
            a.Property(p => p.FullAddressString).HasColumnName("DeliveryAddress").HasMaxLength(500);
            a.Property(p => p.Latitude).HasColumnName("DeliveryLatitude").HasPrecision(18, 10);
            a.Property(p => p.Longitude).HasColumnName("DeliveryLongitude").HasPrecision(18, 10);
            a.Property(p => p.City).HasColumnName("DeliveryCity").HasMaxLength(100);
            a.Property(p => p.Building).HasColumnName("DeliveryBuilding").HasMaxLength(100);
            a.Property(p => p.Apartment).HasColumnName("DeliveryApartment").HasMaxLength(50);
            a.Property(p => p.Floor).HasColumnName("DeliveryFloor").HasMaxLength(20);
        });

        // Enums как строки
        builder.Property(o => o.Status).HasConversion<string>().HasMaxLength(50);
        builder.Property(o => o.PaymentStatus).HasConversion<string>().HasMaxLength(50);

        // Связи
        builder.HasMany(o => o.Items)
            .WithOne(i => i.Order)
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(o => o.History)
            .WithOne(h => h.Order)
            .HasForeignKey(h => h.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}