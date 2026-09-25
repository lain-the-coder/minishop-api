using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniShop.Api.Data;
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

    [HttpGet("test-products")]
    public async Task<IActionResult> GetTestProducts([FromServices] MiniShopDbContext db)
    {
        Console.WriteLine(">>> STEP 1: Referencing DbSet and building query...");
        var query = db.Products.Where(p => p.Price > 10);

        Console.WriteLine(">>> STEP 2: Waiting 3 seconds (No SQL should have run yet)...");
        await Task.Delay(3000);

        Console.WriteLine(">>> STEP 3: Now calling ToListAsync()...");
        var products = await query.ToListAsync();

        Console.WriteLine(">>> STEP 4: ToListAsync finished.");
        return Ok(products);
    }
}