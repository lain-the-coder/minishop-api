namespace MiniShop.Api.Entities;

public class User
{
    public int Id { get; set; }
    public required string Email { get; set; }
    public string? ExternalId { get; set; } // Nullable for additive migration
    public List<Order> Orders { get; set; } = [];
}