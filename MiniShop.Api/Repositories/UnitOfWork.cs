using MiniShop.Api.Data;

namespace MiniShop.Repositories;
public class UnitOfWork(MiniShopDbContext db) : IUnitOfWork
{
    public IProductRepository Products { get; } = new ProductRepository(db);
    public IOrderRepository Orders { get; } = new OrderRepository(db);
    public IUserRepository Users { get; } = new UserRepository(db);

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => db.SaveChangesAsync(ct);
}