namespace MiniShop.Dtos;

public class OrderDto
{
    public int Id { get; init; }
    public required string Status { get; init; }
    public DateTime CreatedAt { get; init; }
    public decimal Total { get; init; }
    public List<OrderItemDto> Items { get; init; } = [];
}

public class OrderItemDto
{
    public int ProductId { get; init; }
    public required string ProductName { get; init; }
    public int Quantity { get; init; }
    public decimal PriceAtPurchase { get; init; }
}