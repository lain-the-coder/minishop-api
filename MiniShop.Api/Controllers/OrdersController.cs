using Microsoft.AspNetCore.Mvc;
using MiniShop.Dtos;
using MiniShop.Services;

namespace MiniShop.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController(IOrderService orderService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<OrderDto>> PlaceOrder([FromBody] CreateOrderRequest request)
    {
        // TODO: Day 3 Block 2b will map ICurrentUser.ExternalId -> User.Id.
        // For tonight, use Alice (seeded user Id = 1).
        const int callerId = 1;

        var order = await orderService.PlaceOrderAsync(callerId, request);

        return CreatedAtAction(nameof(GetMyOrders), new { id = order.Id }, order);
    }

    [HttpGet("mine")]
    public async Task<ActionResult<List<OrderDto>>> GetMyOrders()
    {
        // TODO: Day 3 Block 2b will map ICurrentUser.ExternalId -> User.Id.
        const int callerId = 1;

        var orders = await orderService.GetMyOrdersAsync(callerId);
        return Ok(orders);
    }
}