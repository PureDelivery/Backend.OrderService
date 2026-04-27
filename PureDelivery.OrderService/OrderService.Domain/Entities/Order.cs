using PureDelivery.Shared.Contracts.Domain.Enums;

namespace OrderService.Domain.Entities;

public class Order
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public Guid RestaurantId { get; set; }

    public string RestaurantName { get; set; } = string.Empty;

    public virtual ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();

    public AddressSnapshot DeliveryAddress { get; set; } = null!;
    public OrderMoney Money { get; set; } = null!;

    public OrderStatus Status { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public PaymentStatus PaymentStatus { get; set; }

    public Guid? PaymentId { get; set; }
    public string? SessionId { get; set; }

    /// <summary>Short human-readable order identifier, e.g. "483921". Visible to restaurant, courier, customer.</summary>
    public string OrderNumber { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
    public virtual ICollection<OrderHistory> History { get; set; } = new List<OrderHistory>();
}
