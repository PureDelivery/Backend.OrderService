using System.ComponentModel.DataAnnotations;

namespace OrderService.Application.DTOs.Requests;

public class CancelOrderRequest
{
    [Required]
    [MaxLength(500)]
    public string Reason { get; set; } = string.Empty;
}


