using MiniShop.Api.Entities;

namespace MiniShop.Repositories;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(int id);
    Task<List<Order>> ListByUserAsync(int userId);
    void Add(Order order);
    IQueryable<Order> Query();
}