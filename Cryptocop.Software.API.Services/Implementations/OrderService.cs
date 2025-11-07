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
        // Create order in repository (returns unmasked credit card per spec)
        var created = await _orderRepository.CreateNewOrder(email, order);

        // Delete the current shopping cart
        await _shoppingCartRepository.DeleteCart(email);

        // Publish message to RabbitMQ with routing key 'create-order'
        await _queueService.PublishMessage("create-order", created);

        return created;
    }
}