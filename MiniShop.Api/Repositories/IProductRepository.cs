using MiniShop.Api.Entities;

namespace MiniShop.Repositories;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(int id);
    Task<List<Product>> ListAsync(string? search, int skip, int take);
    Task<int> CountAsync(string? search);
    Task<List<Product>> GetByIdsAsync(IEnumerable<int> ids);
    void Add(Product product);
    void Remove(Product product);
    IQueryable<Product> Query();
}