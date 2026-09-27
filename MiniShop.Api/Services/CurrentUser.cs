using System.Security.Claims;

namespace MiniShop.Api.Services;

public class CurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    private ClaimsPrincipal? Principal => accessor.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;

    // Checks "sub", falling back to NameIdentifier if mapping is ever enabled
    public string? ExternalId => Principal?.FindFirst("sub")?.Value
                              ?? Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    // Checks "name", falling back to "unique_name" (which dotnet user-jwts includes)
    public string? Name => Principal?.FindFirst("name")?.Value
                        ?? Principal?.FindFirst("unique_name")?.Value;

    // Reads both short "role" and ClaimTypes.Role to be safe across environments
    public IReadOnlyList<string> Roles => Principal?.FindAll("role")
        .Concat(Principal.FindAll(ClaimTypes.Role))
        .Select(c => c.Value)
        .Distinct()
        .ToList() ?? [];
}