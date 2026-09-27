namespace MiniShop.Dtos;

public class CreateOrderRequest
{
    public List<CreateOrderItemRequest> Items { get; init; } = [];
}

public class CreateOrderItemRequest
{
    public int ProductId { get; init; }
    public int Quantity { get; init; }
}