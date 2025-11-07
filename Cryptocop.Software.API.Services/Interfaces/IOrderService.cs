using Cryptocop.Software.API.Models.Dtos;
using Cryptocop.Software.API.Models.InputModels;

namespace Cryptocop.Software.API.Services.Interfaces;

public interface IOrderService
{
    Task<IEnumerable<OrderDto>> GetOrders(string email);
    Task<OrderDto> CreateNewOrder(string email, OrderInputModel order);
}