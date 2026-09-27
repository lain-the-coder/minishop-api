using MiniShop.Dtos;

namespace MiniShop.Services;

public interface IOrderService
{
    Task<OrderDto> PlaceOrderAsync(CreateOrderRequest request);
    Task<List<OrderDto>> GetMyOrdersAsync();
}