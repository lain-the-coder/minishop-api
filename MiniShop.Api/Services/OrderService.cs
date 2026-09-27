using MiniShop.Api.Entities;
using MiniShop.Api.Services;
using MiniShop.Dtos;
using MiniShop.Repositories;

namespace MiniShop.Services;

public class OrderService(IUnitOfWork uow, IClock clock, ICurrentUser currentUser) : IOrderService
{
    public async Task<OrderDto> PlaceOrderAsync(CreateOrderRequest request)
    {
        var callerId = await ResolveCallerIdAsync();
        // 1. Validate request shape
        if (request.Items is null || request.Items.Count == 0)
        {
            throw new ValidationException("Order must contain at least one item.");
        }

        if (request.Items.Any(i => i.Quantity < 1))
        {
            throw new ValidationException("Each item quantity must be at least 1.");
        }

        // 2. Fetch all products in one round-trip (prevents N+1)
        var requestedIds = request.Items.Select(i => i.ProductId).Distinct().ToList();
        var products = await uow.Products.GetByIdsAsync(requestedIds);
        var productMap = products.ToDictionary(p => p.Id);

        // 3. Ensure all requested products exist
        foreach (var id in requestedIds)
        {
            if (!productMap.ContainsKey(id))
            {
                throw new NotFoundException($"Product with ID {id} was not found.");
            }
        }

        // 4. Validate stock for all items before making any state mutations
        foreach (var item in request.Items)
        {
            var product = productMap[item.ProductId];
            if (product.Stock < item.Quantity)
            {
                throw new ValidationException(
                    $"Insufficient stock for product '{product.Name}'. Requested: {item.Quantity}, Available: {product.Stock}.");
            }
        }

        // 5. Build the Order aggregate and snapshot current price
        var order = new Order
        {
            UserId = callerId,
            Status = OrderStatus.Pending, // Or "Pending" depending on your model
            CreatedAt = clock.UtcNow,
            Items = []
        };

        foreach (var item in request.Items)
        {
            var product = productMap[item.ProductId];

            // 6. Mutate tracked entity state: decrement stock
            product.Stock -= item.Quantity;

            // 7. Add child entity to aggregate with price snapshot
            order.Items.Add(new OrderItem
            {
                ProductId = product.Id,
                Quantity = item.Quantity,
                PriceAtPurchase = product.Price
            });
        }

        // 8. Add aggregate root to repository and commit the single atomic transaction
        uow.Orders.Add(order);
        await uow.SaveChangesAsync();

        return ToDto(order, productMap);
    }

    public async Task<List<OrderDto>> GetMyOrdersAsync()
    {
        var callerId = await ResolveCallerIdAsync();
        var orders = await uow.Orders.ListByUserAsync(callerId);

        // Fetch products to map product names in the DTOs
        var productIds = orders
            .SelectMany(o => o.Items)
            .Select(i => i.ProductId)
            .Distinct()
            .ToList();

        var products = await uow.Products.GetByIdsAsync(productIds);
        var productMap = products.ToDictionary(p => p.Id);

        return orders.Select(o => ToDto(o, productMap)).ToList();
    }

    private static OrderDto ToDto(Order order, Dictionary<int, Product> productMap) => new()
    {
        Id = order.Id,
        Status = order.Status.ToString(),
        CreatedAt = order.CreatedAt,
        Total = order.Items.Sum(i => i.PriceAtPurchase * i.Quantity),
        Items = order.Items.Select(i => new OrderItemDto
        {
            ProductId = i.ProductId,
            ProductName = productMap.TryGetValue(i.ProductId, out var p) ? p.Name : "Unknown",
            Quantity = i.Quantity,
            PriceAtPurchase = i.PriceAtPurchase
        }).ToList()
    };

    private async Task<int> ResolveCallerIdAsync(CancellationToken cancellationToken = default)
    {
        var externalId = currentUser.ExternalId
            ?? throw new InvalidOperationException("Cannot resolve caller ID without an authenticated user.");

        var user = await uow.Users.GetByExternalIdAsync(externalId, cancellationToken);
        if (user is null)
        {
            // Just-in-time provisioning: create the local row on first contact
            user = new User
            {
                Email = $"{externalId}@minishop.test",
                ExternalId = externalId
            };

            uow.Users.Add(user);
            await uow.SaveChangesAsync(cancellationToken);
        }

        return user.Id;
    }
}