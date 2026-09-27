using MiniShop.Dtos;

namespace MiniShop.Services;

public interface IOrderService
{
    Task<OrderDto> PlaceOrderAsync(int callerId, CreateOrderRequest request);
    Task<List<OrderDto>> GetMyOrdersAsync(int callerId);
}