namespace MiniShop.Api.Entities;

public class Order
{
    public int Id { get; set; }

    // Relationship: Child of User (Many:1)
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public OrderStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }

    // Relationship: Parent of OrderItems (1:Many)
    public List<OrderItem> Items { get; set; } = [];
}