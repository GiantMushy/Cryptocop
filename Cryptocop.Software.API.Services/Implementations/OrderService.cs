using Cryptocop.Software.API.Models.Dtos;
using Cryptocop.Software.API.Models.InputModels;
using Cryptocop.Software.API.Services.Interfaces;
using Cryptocop.Software.API.Repositories.Interfaces;

namespace Cryptocop.Software.API.Services.Implementations;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IShoppingCartRepository _shoppingCartRepository;
    private readonly IQueueService _queueService;

    public OrderService(IOrderRepository orderRepository,
                        IShoppingCartRepository shoppingCartRepository,
                        IQueueService queueService)
    {
        _orderRepository = orderRepository;
        _shoppingCartRepository = shoppingCartRepository;
        _queueService = queueService;
    }

    public Task<IEnumerable<OrderDto>> GetOrders(string email)
        => _orderRepository.GetOrders(email);

    public async Task<OrderDto> CreateNewOrder(string email, OrderInputModel order)
    {
        var created = await _orderRepository.CreateNewOrder(email, order);
        await _shoppingCartRepository.DeleteCart(email); // Delete the current shopping cart
        await _queueService.PublishMessage("create-order", created); // Publish message to RabbitMQ

        return created;
    }
}