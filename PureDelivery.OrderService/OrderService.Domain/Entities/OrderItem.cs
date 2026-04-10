namespace OrderService.Domain.Entities;

public class OrderItem
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid OrderId { get; set; }
    public virtual Order Order { get; set; } = null!;

    public Guid MenuItemId { get; set; }
    public string MenuItemName { get; set; } = string.Empty;
    public string? MenuItemImageUrl { get; set; }

    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }

    public virtual ICollection<OrderItemOption> SelectedOptions { get; set; } = new List<OrderItemOption>();
    public string? SpecialInstructions { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
