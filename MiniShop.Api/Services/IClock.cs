namespace MiniShop.Api.Services;

public interface IClock
{
    DateTime UtcNow { get; }
}