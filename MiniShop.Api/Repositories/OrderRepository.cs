using Microsoft.EntityFrameworkCore;
using MiniShop.Api.Data;
using MiniShop.Api.Entities;

namespace MiniShop.Repositories;

public class OrderRepository(MiniShopDbContext db) : IOrderRepository
{
    public Task<Order?> GetByIdAsync(int id)
        => db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);

    public async Task<List<Order>> ListByUserAsync(int userId)
    {
        return await db.Orders
            .Include(o => o.Items)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public void Add(Order order)
    {
        db.Orders.Add(order);
    }

    public IQueryable<Order> Query()
    {
        return db.Orders.AsQueryable();
    }
}