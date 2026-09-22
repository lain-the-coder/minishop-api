namespace MiniShop.Api.Services;

public class SystemClock : IClock
{
    public DateTime UtcNow
    {
        get
        {
            return DateTime.UtcNow;
        }
    }
}