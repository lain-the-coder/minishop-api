namespace MiniShop.Api.Services;

public interface ICurrentUser
{
    bool IsAuthenticated { get; }
    string? ExternalId { get; }
    string? Name { get; }
    IReadOnlyList<string> Roles { get; }
}