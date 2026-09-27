using MiniShop.Api.Entities;

namespace MiniShop.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);
    void Add(User user);
    Task<User?> GetByExternalIdAsync(string externalId, CancellationToken cancellationToken = default);
}