namespace MiniShop.Api.Services;

public class CurrentUser : ICurrentUser
{
    public bool IsAuthenticated
    {
        get
        {
            return true;
        }
    }

    public string? ExternalId
    {
        get
        {
            return "a1b2c3d4-e5f6-7a8b-9c0d-1e2f3a4b5c6d";
        }
    }

    public string? Name
    {
        get
        {
            return "Dev User";
        }
    }

    public IReadOnlyList<string> Roles
    {
        get
        {
            // C# 12 collection expression: ["Admin"]
            return ["Admin"];
        }
    }
}