using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Entities;
using OrderService.Infrastructure.EntityConfigurations;

namespace OrderService.Infrastructure.Data;

public class OrderDbContext : DbContext
{
    public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options)
    {
    }

    // DbSets
    public DbSet<Order> Orders { get; set; } = null!;
    public DbSet<OrderItem> OrderItems { get; set; } = null!;
    public DbSet<OrderItemOption> OrderItemOptions { get; set; } = null!;
    public DbSet<OrderHistory> OrderHistories { get; set; } = null!; // Добавили историю

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Конфигурируем Value Objects (Money и Address)
        ConfigureOrderValueObjects(modelBuilder);

        // Конфигурируем связи
        ConfigureRelationships(modelBuilder);

        // Применяем маппинг полей (точность decimal, длины строк и т.д.)
        ApplyEntityConfigurations(modelBuilder);
    }

    private static void ConfigureOrderValueObjects(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(builder =>
        {
            // Настройка денег внутри таблицы Orders
            builder.OwnsOne(o => o.Money, m =>
            {
                m.Property(p => p.SubTotal).HasColumnName("SubTotal").HasPrecision(18, 2);
                m.Property(p => p.DeliveryFee).HasColumnName("DeliveryFee").HasPrecision(18, 2);
                m.Property(p => p.Tax).HasColumnName("Tax").HasPrecision(18, 2);
                m.Property(p => p.Discount).HasColumnName("Discount").HasPrecision(18, 2);
                m.Property(p => p.TotalAmount).HasColumnName("TotalAmount").HasPrecision(18, 2);
            });

            // Настройка адреса внутри таблицы Orders
            builder.OwnsOne(o => o.DeliveryAddress, a =>
            {
                a.Property(p => p.AddressId).HasColumnName("DeliveryAddressId");
                a.Property(p => p.FullAddressString).HasColumnName("DeliveryFullAddress").HasMaxLength(500);
                a.Property(p => p.City).HasColumnName("DeliveryCity").HasMaxLength(100);
                a.Property(p => p.Building).HasColumnName("DeliveryBuilding").HasMaxLength(100);
                a.Property(p => p.Apartment).HasColumnName("DeliveryApartment").HasMaxLength(50);
                a.Property(p => p.Floor).HasColumnName("DeliveryFloor").HasMaxLength(20);
                a.Property(p => p.Latitude).HasColumnName("DeliveryLatitude").HasPrecision(18, 10);
                a.Property(p => p.Longitude).HasColumnName("DeliveryLongitude").HasPrecision(18, 10);
            });
        });
    }

    private static void ApplyEntityConfigurations(ModelBuilder modelBuilder)
    {
        // Указываем точность для цен в айтемах, иначе EF обрежет копейки
        modelBuilder.Entity<OrderItem>(builder => {
            builder.Property(p => p.UnitPrice).HasPrecision(18, 2);
            builder.Property(p => p.TotalPrice).HasPrecision(18, 2);
        });

        modelBuilder.Entity<OrderItemOption>(builder => {
            builder.Property(p => p.AdditionalPrice).HasPrecision(18, 2);
        });

        // Тут можно вызвать твои классы конфигураций, если они уже есть
        // modelBuilder.ApplyConfiguration(new OrderConfiguration());
    }

    private static void ConfigureRelationships(ModelBuilder modelBuilder)
    {
        // Order -> OrderItems
        modelBuilder.Entity<Order>()
            .HasMany(o => o.Items)
            .WithOne(oi => oi.Order)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // OrderItem -> OrderItemOptions
        modelBuilder.Entity<OrderItem>()
            .HasMany(oi => oi.SelectedOptions)
            .WithOne(oio => oio.OrderItem)
            .HasForeignKey(oio => oio.OrderItemId)
            .OnDelete(DeleteBehavior.Cascade);

        // Order -> OrderHistory (История статусов)
        modelBuilder.Entity<Order>()
            .HasMany(o => o.History)
            .WithOne(oh => oh.Order)
            .HasForeignKey(oh => oh.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}