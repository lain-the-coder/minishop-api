using System.ComponentModel.DataAnnotations;

namespace MiniShop.Api.Dtos;

public class CreateProductRequest
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = null!;

    [MaxLength(2000)]
    public string? Description { get; set; }

    [Range(0.01, 100000)]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue)]
    public int Stock { get; set; }
}