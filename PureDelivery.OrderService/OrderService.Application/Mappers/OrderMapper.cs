using OrderService.Application.DTOs.Requests;
using OrderService.Application.DTOs.Responses;
using OrderService.Domain.Entities;
using PureDelivery.Shared.Contracts.Domain.Enums;

namespace OrderService.Application.Mappers;

public class OrderMapper : IOrderMapper
{
    // Steps visible to the customer — internal Cart/Checkout/Payment are pre-order and not shown
    private static readonly (OrderStatus Status, string Label)[] FlowSteps =
    [
        (OrderStatus.Confirmed,       "Confirmed"),
        (OrderStatus.InPreparation,   "In Preparation"),
        (OrderStatus.ReadyForPickup,  "Ready for Pickup"),
        (OrderStatus.Delivery,        "Out for Delivery"),
        (OrderStatus.Completed,       "Delivered"),
    ];

    public OrderDto MapToDto(Order order)
    {
        var addr = order.DeliveryAddress;
        var fullAddress = addr?.FullAddressString is { Length: > 0 } s && s != "Unknown"
            ? s
            : BuildAddressFallback(addr);

        var statusLabel = order.Status.ToString();
        var statusFlow  = BuildStatusFlow(order.Status);

        return new OrderDto
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            RestaurantId = order.RestaurantId,
            RestaurantName = order.RestaurantName,
            SessionId = order.SessionId,
            PaymentId = order.PaymentId,
            PaymentMethod = order.PaymentMethod,
            PaymentMethodLabel = order.PaymentMethod.ToString(),
            PaymentStatus = order.PaymentStatus,
            PaymentStatusLabel = order.PaymentStatus.ToString(),
            Status = order.Status,
            StatusLabel = statusLabel,
            StatusFlow = statusFlow,
            CreatedAt = order.CreatedAt,

            DeliveryAddress = fullAddress,
            DeliveryCity = addr?.City ?? string.Empty,
            DeliveryLatitude = addr?.Latitude ?? 0,
            DeliveryLongitude = addr?.Longitude ?? 0,

            SubTotal = order.Money?.SubTotal ?? 0,
            DeliveryFee = order.Money?.DeliveryFee ?? 0,
            Tax = order.Money?.Tax ?? 0,
            Discount = order.Money?.Discount ?? 0,
            TotalAmount = order.Money?.TotalAmount ?? 0,

            Items = order.Items.Select(MapItemToDto).ToList()
        };
    }

    private static List<StatusFlowStepDto> BuildStatusFlow(OrderStatus current)
    {
        if (current == OrderStatus.Cancelled)
        {
            return [new StatusFlowStepDto
            {
                Status = "Cancelled",
                Label = "Cancelled",
                IsCompleted = true,
                IsCurrent = true
            }];
        }

        return FlowSteps.Select(step => new StatusFlowStepDto
        {
            Status = step.Status.ToString(),
            Label = step.Label,
            IsCompleted = current >= step.Status,
            IsCurrent = current == step.Status,
        }).ToList();
    }

    private static string BuildAddressFallback(AddressSnapshot? addr)
    {
        if (addr == null) return string.Empty;
        var parts = new[] { addr.Building, addr.Apartment, addr.City }
            .Where(p => !string.IsNullOrWhiteSpace(p));
        return string.Join(", ", parts);
    }

    private static OrderItemDto MapItemToDto(OrderItem item)
    {
        return new OrderItemDto
        {
            Id = item.Id,
            MenuItemId = item.MenuItemId,
            MenuItemName = item.MenuItemName,
            MenuItemImageUrl = item.MenuItemImageUrl,
            UnitPrice = item.UnitPrice,
            Quantity = item.Quantity,
            TotalPrice = item.TotalPrice,
            SpecialInstructions = item.SpecialInstructions,
            SelectedOptions = item.SelectedOptions.Select(MapOptionToDto).ToList()
        };
    }

    private static OrderItemOptionDto MapOptionToDto(OrderItemOption option)
    {
        return new OrderItemOptionDto
        {
            Id = option.Id,
            OptionId = option.OptionId,
            OptionName = option.OptionName,
            ChoiceId = option.ChoiceId,
            ChoiceName = option.ChoiceName,
            AdditionalPrice = option.AdditionalPrice ?? 0
        };
    }

    public Order MapFromCreateRequest(CreateOrderRequest request)
    {
        return new Order
        {
            CustomerId = request.CustomerId,
            RestaurantId = request.RestaurantId,
            SessionId = request.SessionId,
            Status = OrderStatus.Cart,
            CreatedAt = DateTime.UtcNow,
            Items = request.Items.Select(MapItemFromRequest).ToList()
        };
    }

    private static OrderItem MapItemFromRequest(OrderItemRequest request)
    {
        return new OrderItem
        {
            MenuItemId = request.MenuItemId,
            Quantity = request.Quantity,
            SpecialInstructions = request.SpecialInstructions,
            SelectedOptions = request.SelectedOptions?.Select(MapOptionFromRequest).ToList() ?? []
        };
    }

    private static OrderItemOption MapOptionFromRequest(OrderItemOptionRequest request)
    {
        return new OrderItemOption
        {
            OptionId = request.OptionId,
            ChoiceId = request.ChoiceId
        };
    }
}
