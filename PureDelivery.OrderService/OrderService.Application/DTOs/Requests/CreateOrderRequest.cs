using System.ComponentModel.DataAnnotations;
using PureDelivery.Shared.Contracts.DTOs.Session;

namespace OrderService.Application.DTOs.Requests;

public class CreateOrderRequest
{
    [Required]
    public Guid CustomerId { get; set; }
    
    [Required]
    public Guid RestaurantId { get; set; }
    
    [Required]
    public Guid DeliveryAddressId { get; set; }
    
    [Required]
    public List<OrderItemRequest> Items { get; set; } = new();
    
    public string? DeliveryInstructions { get; set; }
    
    public string? SpecialInstructions { get; set; }
    
    public string? SessionId { get; set; }
}

public class OrderItemRequest
{
    [Required]
    public Guid MenuItemId { get; set; }
    
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
    public int Quantity { get; set; }
    
    public List<OrderItemOptionRequest>? SelectedOptions { get; set; }
    
    public string? SpecialInstructions { get; set; }
}

public class OrderItemOptionRequest
{
    [Required]
    public Guid OptionId { get; set; }
    
    [Required]
    public Guid ChoiceId { get; set; }
}


