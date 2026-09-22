namespace MiniShop.Api.Services.Abstractions;

public interface ICurrentUser
{
    bool IsAuthenticated { get; }
    string? ObjectId { get; }
    string? Name { get; }
    IReadOnlyList<string> Roles { get; }
}