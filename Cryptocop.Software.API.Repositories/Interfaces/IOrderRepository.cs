using Cryptocop.Software.API.Models.Dtos;
using Cryptocop.Software.API.Models.InputModels;

namespace Cryptocop.Software.API.Repositories.Interfaces;

public interface IOrderRepository
{
    Task<IEnumerable<OrderDto>> GetOrdersAsync(string email);
    Task<OrderDto> CreateNewOrderAsync(string email, OrderInputModel order);
}