using PureDelivery.Shared.Contracts.Domain.Enums;

namespace OrderService.Domain.Entities;

public class OrderHistory
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid OrderId { get; set; }
    public virtual Order Order { get; set; } = null!;

    public OrderStatus Status { get; set; }

    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

    public string? ChangedBy { get; set; }

    public string? Comment { get; set; }
}