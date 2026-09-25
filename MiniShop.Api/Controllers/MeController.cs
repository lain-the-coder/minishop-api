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

    [HttpGet("test-include")]
    public async Task<IActionResult> TestInclude([FromServices] MiniShopDbContext db)
    {
        var orders = await db.Orders
            .Include(o => o.Items)
            .ToListAsync();

        return Ok(orders);
    }

    [HttpGet("test-projection")]
    public async Task<IActionResult> TestProjection([FromServices] MiniShopDbContext db)
    {
        var orders = await db.Orders
            .Select(o => new
            {
                OrderId = o.Id,
                OrderStatus = o.Status,
                ItemCount = o.Items.Count()
            })
            .ToListAsync();

        return Ok(orders);
    }

    [HttpGet("test-tracker")]
    public async Task<IActionResult> TestTracker([FromServices] MiniShopDbContext db)
    {
        // 1. Fetch Product #1
        var product = await db.Products.FindAsync(1);
        if (product == null) return NotFound("Product 1 not found");

        var entry = db.Entry(product);
        Console.WriteLine($"\n[1] State immediately after query: {entry.State}");

        // 2. Mutate Stock (Notice: zero EF Core methods called here!)
        product.Stock -= 1;
        Console.WriteLine($"[2] State after changing property: {entry.State}");

        // 3. Inspect modified properties
        var modifiedProps = entry.Properties
            .Where(p => p.IsModified)
            .Select(p => p.Metadata.Name)
            .ToList();
        Console.WriteLine($"[3] Modified properties detected: {string.Join(", ", modifiedProps)}");

        // 4. Save and inspect post-commit state
        Console.WriteLine("--> Calling SaveChangesAsync()...");
        await db.SaveChangesAsync();
        Console.WriteLine($"[4] State after SaveChanges: {entry.State}\n");

        return Ok(new { product.Id, product.Stock });
    }

    [HttpGet("test-untracked")]
    public async Task<IActionResult> TestUntracked([FromServices] MiniShopDbContext db)
    {
        // 1. Fetch Product #1 with AsNoTracking()
        var product = await db.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == 1);

        if (product == null) return NotFound();

        var entry = db.Entry(product);
        Console.WriteLine($"\n[Untracked 1] State immediately after query: {entry.State}");

        // 2. Mutate Stock in C# memory
        product.Stock -= 10;
        db.ChangeTracker.DetectChanges();
        Console.WriteLine($"[Untracked 2] State after mutation: {entry.State}");

        // 3. Attempt to save changes
        Console.WriteLine("--> Calling SaveChangesAsync()...");
        var rowsAffected = await db.SaveChangesAsync();
        Console.WriteLine($"--> Rows affected: {rowsAffected}\n");

        return Ok(new { product.Id, product.Stock, rowsAffected });
    }
}