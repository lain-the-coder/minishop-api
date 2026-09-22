using Microsoft.AspNetCore.Mvc;
using MiniShop.Api.Services;

namespace MiniShop.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MeController(ICurrentUser currentUser, IClock clock) : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        var response = new
        {
            IsAuthenticated = currentUser.IsAuthenticated,
            ExternalId = currentUser.ExternalId,
            Name = currentUser.Name,
            Roles = currentUser.Roles,
            ServerTimeUtc = clock.UtcNow
        };

        return Ok(response);
    }
}