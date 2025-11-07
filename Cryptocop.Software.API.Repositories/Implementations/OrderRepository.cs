using Cryptocop.Software.API.Models.Dtos;
using Cryptocop.Software.API.Models.InputModels;
using Cryptocop.Software.API.Repositories.Helpers;
using Cryptocop.Software.API.Repositories.Interfaces;
using Cryptocop.Software.API.Repositories.Data;
using Microsoft.EntityFrameworkCore;

namespace Cryptocop.Software.API.Repositories.Implementations;

public class OrderRepository : IOrderRepository
{
    private readonly IUserRepository _userRepository;
    private readonly IAddressRepository _addressRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IShoppingCartRepository _shoppingCartRepository;
    private readonly CryptocopDbContext _db;

    public OrderRepository(IUserRepository userRepository,
                           IAddressRepository addressRepository,
                           IPaymentRepository paymentRepository,
                           IShoppingCartRepository shoppingCartRepository,
                           CryptocopDbContext db)
    {
        _userRepository = userRepository;
        _addressRepository = addressRepository;
        _paymentRepository = paymentRepository;
        _shoppingCartRepository = shoppingCartRepository;
        _db = db;
    }

    public async Task<IEnumerable<OrderDto>> GetOrders(string email)
    {
        var userId = await _db.Users.Where(u => u.Email == email).Select(u => u.Id).FirstOrDefaultAsync();
        if (userId == 0) return Enumerable.Empty<OrderDto>();

        var orders = await _db.Orders
            .Include(o => o.User)
            .Include(o => o.OrderItems)
            .Where(o => o.UserId == userId)
            .OrderBy(o => o.Id)
            .ToListAsync();

        return orders.Select(o => new OrderDto
        {
            Id = o.Id,
            Email = email,
            FullName = o.User?.FullName ?? string.Empty,
            StreetName = o.StreetName,
            HouseNumber = o.HouseNumber,
            ZipCode = o.ZipCode,
            Country = o.Country,
            City = o.City,
            CardholderName = o.CardholderName,
            CreditCard = o.CreditCard, // masked stored
            OrderDate = o.OrderDate.ToString("dd.MM.yyyy"),
            TotalPrice = (float)o.TotalPrice,
            OrderItems = o.OrderItems.Select(oi => new OrderItemDto
            {
                Id = oi.Id,
                ProductIdentifier = oi.ProductIdentifier,
                Quantity = oi.Quantity,
                UnitPrice = (float)oi.UnitPrice,
                TotalPrice = (float)oi.TotalPrice
            })
        });
    }

    public async Task<OrderDto> CreateNewOrder(string email, OrderInputModel order)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email)
                   ?? throw new InvalidOperationException("User not found");

    var address = await _addressRepository.GetAddressById(email, order.AddressId)
                      ?? throw new InvalidOperationException("Address not found");

    var card = await _paymentRepository.GetPaymentCardById(email, order.PaymentCardId)
                   ?? throw new InvalidOperationException("Payment card not found");

    var cartItems = (await _shoppingCartRepository.GetCartItems(email)).ToList();
        if (cartItems.Count == 0)
        {
            throw new InvalidOperationException("Shopping cart is empty");
        }

        var now = DateTime.UtcNow;
        var items = cartItems.Select(ci => new Entities.OrderItem
        {
            ProductIdentifier = ci.ProductIdentifier,
            Quantity = ci.Quantity,
            UnitPrice = (decimal)ci.UnitPrice,
            TotalPrice = (decimal)(ci.UnitPrice * ci.Quantity)
        }).ToList();

        var entity = new Entities.Order
        {
            UserId = user.Id,
            OrderDate = now,
            TotalPrice = items.Sum(i => i.TotalPrice),
            CardholderName = card.CardholderName,
            CreditCard = PaymentCardHelper.MaskPaymentCard(card.CardNumber),
            StreetName = address.StreetName,
            HouseNumber = address.HouseNumber,
            ZipCode = address.ZipCode,
            Country = address.Country,
            City = address.City,
            OrderItems = items
        };

        _db.Orders.Add(entity);
        await _db.SaveChangesAsync();

        // Clear the cart once order saved
    await _shoppingCartRepository.ClearCart(email);

        // Return unmasked card for creation response
        return new OrderDto
        {
            Id = entity.Id,
            Email = email,
            FullName = user.FullName,
            StreetName = entity.StreetName,
            HouseNumber = entity.HouseNumber,
            ZipCode = entity.ZipCode,
            Country = entity.Country,
            City = entity.City,
            CardholderName = entity.CardholderName,
            CreditCard = card.CardNumber,
            OrderDate = now.ToString("dd.MM.yyyy"),
            TotalPrice = (float)entity.TotalPrice,
            OrderItems = items.Select(oi => new OrderItemDto
            {
                Id = oi.Id,
                ProductIdentifier = oi.ProductIdentifier,
                Quantity = oi.Quantity,
                UnitPrice = (float)oi.UnitPrice,
                TotalPrice = (float)oi.TotalPrice
            })
        };
    }
}