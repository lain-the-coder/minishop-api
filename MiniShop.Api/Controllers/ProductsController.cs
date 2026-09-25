using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniShop.Api.Data;
using MiniShop.Api.Dtos;
using MiniShop.Api.Entities;

namespace MiniShop.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController(MiniShopDbContext db) : ControllerBase
{
    // GET: /api/products
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetAll()
    {
        var products = await db.Products
            .AsNoTracking()
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Stock = p.Stock
            })
            .ToListAsync();

        return Ok(products);
    }

    // GET: /api/products/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductDto>> GetById(int id)
    {
        var product = await db.Products
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Stock = p.Stock
            })
            .FirstOrDefaultAsync();

        if (product == null)
        {
            return NotFound();
        }

        return Ok(product);
    }
    // POST: /api/products
    [HttpPost]
    public async Task<ActionResult<ProductDto>> Create(CreateProductRequest request)
    {
        // 1. Map DTO to new Entity
        var product = new Product
        {
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            Stock = request.Stock
        };

        // 2. Add to context (State = Added)
        db.Products.Add(product);

        // 3. Save to database
        // Pipeline: DetectChanges -> INSERT -> identity Id read back into product.Id -> Commit
        await db.SaveChangesAsync();

        // 4. Map back to ProductDto
        var responseDto = new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Stock = product.Stock
        };

        // 5. 201 Created with Location Header
        return CreatedAtAction(
            nameof(GetById),
            new { id = product.Id },
            responseDto
        );
    }
    // PUT: /api/products/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, CreateProductRequest request)
    {
        // 1. Fetch tracked entity (no AsNoTracking)
        var product = await db.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound();
        }

        // 2. Mutate properties in memory
        product.Name = request.Name;
        product.Description = request.Description;
        product.Price = request.Price;
        product.Stock = request.Stock;

        // 3. Save changes (NO db.Update call)
        // DetectChanges diffs properties against snapshot -> emits minimal UPDATE SQL
        await db.SaveChangesAsync();

        // 4. REST convention: 204 No Content
        return NoContent();
    }

    // DELETE: /api/products/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        // 1. Fetch tracked entity
        var product = await db.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound();
        }

        // 2. Mark state as Deleted
        db.Products.Remove(product);

        // 3. Commit DELETE to database
        await db.SaveChangesAsync();

        // 4. REST convention: 204 No Content
        return NoContent();
    }
}