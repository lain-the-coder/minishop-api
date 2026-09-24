namespace MiniShop.Api.Entities;

public class User
{
    public int Id { get; set; }
    public required string Email { get; set; }
    public List<Order> Orders { get; set; } = [];
}