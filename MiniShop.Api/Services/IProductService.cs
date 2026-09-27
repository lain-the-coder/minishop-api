using MiniShop.Api.Dtos;
using MiniShop.Dtos;

namespace MiniShop.Services;

public interface IProductService
{
    Task<PagedResult<ProductDto>> GetPagedAsync(string? search, int page, int pageSize);
    Task<ProductDto> GetByIdAsync(int id);
    Task<ProductDto> CreateAsync(CreateProductRequest request);
    Task UpdateAsync(int id, CreateProductRequest request);
    Task DeleteAsync(int id);
}