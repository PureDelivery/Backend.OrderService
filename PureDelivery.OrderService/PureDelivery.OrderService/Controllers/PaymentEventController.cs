using Microsoft.AspNetCore.Mvc;
using OrderService.Application.EventHandlers;
using OrderService.Application.Exceptions;
using OrderService.Application.Services;
using PureDelivery.Shared.Contracts.Common;
using PureDelivery.Shared.Contracts.Domain.Models;

namespace PureDelivery.OrderService.Controllers;

[ApiController]
[Route("api/v1/order/[controller]")]
[Produces("application/json")]
public class PaymentEventController : ControllerBase
{
    private readonly PaymentCompletedEventHandler _paymentEventHandler;
    private readonly ILogger<PaymentEventController> _logger;

    public PaymentEventController(
        PaymentCompletedEventHandler paymentEventHandler,
        ILogger<PaymentEventController> logger)
    {
        _paymentEventHandler = paymentEventHandler;
        _logger = logger;
    }

    [HttpPost("payment-completed")]
    public async Task<ActionResult<BaseResponse<bool>>> HandlePaymentCompleted(
        [FromBody] PaymentCompletedRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid || string.IsNullOrEmpty(request.RestaurantId))
            {
                return BadRequest(BaseResponse<bool>.Failure("Invalid request data: SessionId and RestaurantId are required"));
            }

            await _paymentEventHandler.HandlePaymentCompletedAsync(
                request.SessionId, 
                request.RestaurantId,
                request.PaymentId, 
                cancellationToken);

            return Ok(BaseResponse<bool>.Success(true));
        }
        catch (OrderException ex)
        {
            _logger.LogWarning(ex, "Order session error on payment: {ErrorCode}", ex.ErrorCode);
            return StatusCode(ex.StatusCode, BaseResponse<bool>.Failure(ex.UserMessage));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling payment completed event for session {SessionId}", request.SessionId);
            return StatusCode(500, BaseResponse<bool>.Failure("Internal server error"));
        }
    }
}

public class PaymentCompletedRequest
{
    public string SessionId { get; set; } = string.Empty;
    public string RestaurantId { get; set; } = string.Empty; // Добавили это
    public Guid PaymentId { get; set; }
}

