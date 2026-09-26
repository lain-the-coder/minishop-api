namespace MiniShop.Repositories;
public interface IUnitOfWork
{
    IProductRepository Products { get; }
    IOrderRepository Orders { get; }
    IUserRepository Users { get; }
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}