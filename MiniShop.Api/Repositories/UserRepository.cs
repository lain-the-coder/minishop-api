using Microsoft.EntityFrameworkCore;
using MiniShop.Api.Data;
using MiniShop.Api.Entities;

namespace MiniShop.Repositories;

public class UserRepository(MiniShopDbContext db) : IUserRepository
{
    public Task<User?> GetByIdAsync(int id)
        => db.Users.FindAsync(id).AsTask();

    public void Add(User user)
    {
        db.Users.Add(user);
    }
    public async Task<User?> GetByExternalIdAsync(string externalId, CancellationToken cancellationToken = default)
    {
        return await db.Users
            .FirstOrDefaultAsync(u => u.ExternalId == externalId, cancellationToken);
    }
}