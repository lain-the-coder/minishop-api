namespace MiniShop.Api.Services.Abstractions;

public interface IClock
{
    DateTime UtcNow { get; }
}