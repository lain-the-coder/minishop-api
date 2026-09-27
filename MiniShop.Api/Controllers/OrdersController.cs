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
        var order = await orderService.PlaceOrderAsync(request);

        return CreatedAtAction(nameof(GetMyOrders), new { id = order.Id }, order);
    }

    [HttpGet("mine")]
    public async Task<ActionResult<List<OrderDto>>> GetMyOrders()
    {
        var orders = await orderService.GetMyOrdersAsync();
        return Ok(orders);
    }
}