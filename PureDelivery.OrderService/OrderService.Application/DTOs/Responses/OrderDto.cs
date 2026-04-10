using PureDelivery.Shared.Contracts.Domain.Enums;

namespace OrderService.Application.DTOs.Responses;

public class OrderDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public Guid RestaurantId { get; set; }
    public string RestaurantName { get; set; } = string.Empty;
    public List<OrderItemDto> Items { get; set; } = new();
    public Guid DeliveryAddressId { get; set; }
    public string DeliveryAddress { get; set; } = string.Empty;
    public decimal DeliveryLatitude { get; set; }
    public decimal DeliveryLongitude { get; set; }
    public string? DeliveryInstructions { get; set; }
    public Guid? PaymentId { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public Guid? CourierId { get; set; }
    public string? CourierName { get; set; }
    public OrderStatus Status { get; set; }
    public DeliveryStatus DeliveryStatus { get; set; }
    public decimal SubTotal { get; set; }
    public decimal DeliveryFee { get; set; }
    public decimal Tax { get; set; }
    public decimal Discount { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? PreparationStartedAt { get; set; }
    public DateTime? ReadyForPickupAt { get; set; }
    public DateTime? PickedUpAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public int EstimatedPreparationMinutes { get; set; }
    public int EstimatedDeliveryMinutes { get; set; }
    public string? SessionId { get; set; }
    public string? SpecialInstructions { get; set; }
    public string? CancellationReason { get; set; }
}

public class OrderItemDto
{
    public Guid Id { get; set; }
    public Guid MenuItemId { get; set; }
    public string MenuItemName { get; set; } = string.Empty;
    public string? MenuItemImageUrl { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
    public List<OrderItemOptionDto> SelectedOptions { get; set; } = new();
    public string? SpecialInstructions { get; set; }
}

public class OrderItemOptionDto
{
    public Guid Id { get; set; }
    public Guid OptionId { get; set; }
    public string OptionName { get; set; } = string.Empty;
    public Guid ChoiceId { get; set; }
    public string ChoiceName { get; set; } = string.Empty;
    public decimal? AdditionalPrice { get; set; }
}


