using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderService.Domain.Entities;

namespace OrderService.Infrastructure.EntityConfigurations;

public class OrderItemOptionConfiguration : IEntityTypeConfiguration<OrderItemOption>
{
    public void Configure(EntityTypeBuilder<OrderItemOption> builder)
    {
        builder.ToTable("OrderItemOptions");

        builder.HasKey(oio => oio.Id);

        builder.Property(oio => oio.Id)
            .IsRequired();

        builder.Property(oio => oio.OrderItemId)
            .IsRequired();

        builder.Property(oio => oio.OptionId)
            .IsRequired();

        builder.Property(oio => oio.OptionName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(oio => oio.ChoiceId)
            .IsRequired();

        builder.Property(oio => oio.ChoiceName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(oio => oio.AdditionalPrice)
            .HasPrecision(18, 2);

        // Indexes
        builder.HasIndex(oio => oio.OrderItemId);
    }
}

