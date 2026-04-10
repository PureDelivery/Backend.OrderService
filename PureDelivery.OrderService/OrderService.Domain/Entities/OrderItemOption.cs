namespace OrderService.Domain.Entities;

public class OrderItemOption
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid OrderItemId { get; set; }
    public virtual OrderItem OrderItem { get; set; } = null!;

    public Guid OptionId { get; set; }
    public string OptionName { get; set; } = string.Empty;

    public Guid ChoiceId { get; set; }
    public string ChoiceName { get; set; } = string.Empty;
    public decimal? AdditionalPrice { get; set; }
}
