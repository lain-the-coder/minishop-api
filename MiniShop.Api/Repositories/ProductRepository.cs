using Microsoft.EntityFrameworkCore;
using MiniShop.Api.Data;
using MiniShop.Api.Entities;

namespace MiniShop.Repositories;

public class ProductRepository(MiniShopDbContext db) : IProductRepository
{
    public Task<Product?> GetByIdAsync(int id)
        => db.Products.FindAsync(id).AsTask();

    public async Task<List<Product>> ListAsync(string? search, int skip, int take)
    {
        return await ApplyFilter(search)
            .OrderBy(p => p.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task<int> CountAsync(string? search)
    {
        return await ApplyFilter(search).CountAsync();
    }

    public async Task<List<Product>> GetByIdsAsync(IEnumerable<int> ids)
    {
        return await db.Products
            .Where(p => ids.Contains(p.Id))
            .ToListAsync();
    }

    public void Add(Product product)
    {
        db.Products.Add(product);
    }

    public void Remove(Product product)
    {
        db.Products.Remove(product);
    }

    public IQueryable<Product> Query()
    {
        return db.Products.AsQueryable();
    }

    private IQueryable<Product> ApplyFilter(string? search)
    {
        var q = db.Products.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            q = q.Where(p => p.Name.Contains(search));
        }

        return q;
    }
}