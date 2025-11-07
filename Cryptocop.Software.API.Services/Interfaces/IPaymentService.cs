using Cryptocop.Software.API.Models.Dtos;
using Cryptocop.Software.API.Models.InputModels;

namespace Cryptocop.Software.API.Services.Interfaces;

public interface IPaymentService
{
    Task AddPaymentCard(string email, PaymentCardInputModel paymentCard);
    Task<IEnumerable<PaymentCardDto>> GetStoredPaymentCards(string email);
}