using MiniShop.Api.Services.Abstractions;

namespace MiniShop.Api.Services;

public class UtcClock : IClock
{
    public DateTime UtcNow
    {
        get
        {
            return DateTime.UtcNow;
        }
    }
}