using MiniShop.Api.Dtos;
using MiniShop.Api.Entities;
using MiniShop.Api.Services;
using MiniShop.Dtos;
using MiniShop.Repositories;

namespace MiniShop.Services;

public class ProductService(IUnitOfWork uow) : IProductService
{
    private const int MaxPageSize = 100;

    public async Task<PagedResult<ProductDto>> GetPagedAsync(string? search, int page, int pageSize)
    {
        // 1. Clamp inputs to defend against DoS or invalid paging
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, MaxPageSize);

        var skip = (page - 1) * pageSize;

        // 2. Query count and page slice using the repository
        var total = await uow.Products.CountAsync(search);
        var products = await uow.Products.ListAsync(search, skip, pageSize);

        // 3. Map entities to DTOs
        var dtos = products.Select(ToDto).ToList();

        return new PagedResult<ProductDto>
        {
            Items = dtos,
            Total = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<ProductDto> GetByIdAsync(int id)
    {
        var product = await uow.Products.GetByIdAsync(id)
            ?? throw new NotFoundException($"Product with ID {id} was not found.");

        return ToDto(product);
    }

    public async Task<ProductDto> CreateAsync(CreateProductRequest request)
    {
        var product = new Product
        {
            Name = request.Name,
            Price = request.Price,
            Stock = request.Stock
        };

        uow.Products.Add(product);
        await uow.SaveChangesAsync();

        // EF Core populates product.Id after SaveChangesAsync executes
        return ToDto(product);
    }

    public async Task UpdateAsync(int id, CreateProductRequest request)
    {
        var product = await uow.Products.GetByIdAsync(id)
            ?? throw new NotFoundException($"Product with ID {id} was not found.");

        // Mutate tracked entity properties
        product.Name = request.Name;
        product.Price = request.Price;
        product.Stock = request.Stock;

        // No Update() call needed; the change tracker detects mutations
        await uow.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var product = await uow.Products.GetByIdAsync(id)
            ?? throw new NotFoundException($"Product with ID {id} was not found.");

        uow.Products.Remove(product);
        await uow.SaveChangesAsync();
    }

    private static ProductDto ToDto(Product p) => new()
    {
        Id = p.Id,
        Name = p.Name,
        Price = p.Price,
        Stock = p.Stock
    };
}