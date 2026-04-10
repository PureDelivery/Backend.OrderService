using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Infrastructure.EntityConfigurations
{
    public class OrderHistoryConfiguration : IEntityTypeConfiguration<OrderHistory>
    {
        public void Configure(EntityTypeBuilder<OrderHistory> builder)
        {
            builder.ToTable("OrderHistory");
            builder.HasKey(h => h.Id);
            builder.Property(h => h.Status).HasConversion<string>().HasMaxLength(50);
            builder.Property(h => h.Comment).HasMaxLength(1000);
            builder.Property(h => h.ChangedBy).HasMaxLength(100);
        }
    }
}
