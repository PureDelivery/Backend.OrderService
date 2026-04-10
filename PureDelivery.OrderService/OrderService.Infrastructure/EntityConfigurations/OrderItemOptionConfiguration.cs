using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderService.Domain.Entities;

namespace OrderService.Infrastructure.EntityConfigurations;

public class OrderItemOptionConfiguration : IEntityTypeConfiguration<OrderItemOption>
{
    public void Configure(EntityTypeBuilder<OrderItemOption> builder)
    {
        builder.ToTable("OrderItemOptions");
        builder.HasKey(o => o.Id);

        builder.Property(o => o.OptionName).IsRequired().HasMaxLength(200);
        builder.Property(o => o.ChoiceName).IsRequired().HasMaxLength(200);
        builder.Property(o => o.AdditionalPrice).HasPrecision(18, 2);
    }
}

