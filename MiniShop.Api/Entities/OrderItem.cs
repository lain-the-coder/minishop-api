namespace MiniShop.Api.Entities;

public class OrderItem
{
    public int Id { get; set; }

    // Relationship: Belongs to Order (Many:1)
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;

    // Relationship: Belongs to Product (Many:1)
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int Quantity { get; set; }
    public decimal PriceAtPurchase { get; set; }
}