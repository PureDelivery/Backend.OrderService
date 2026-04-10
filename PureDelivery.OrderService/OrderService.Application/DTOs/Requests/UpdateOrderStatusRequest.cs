using System.ComponentModel.DataAnnotations;
using PureDelivery.Shared.Contracts.Domain.Enums;

namespace OrderService.Application.DTOs.Requests;

public class UpdateOrderStatusRequest
{
    [Required]
    public OrderStatus Status { get; set; }
    
    public string? Notes { get; set; }
}


