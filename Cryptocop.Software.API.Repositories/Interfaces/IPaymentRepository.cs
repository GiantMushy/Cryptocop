using Cryptocop.Software.API.Models.Dtos;
using Cryptocop.Software.API.Models.InputModels;

namespace Cryptocop.Software.API.Repositories.Interfaces;

public interface IPaymentRepository
{
    Task AddPaymentCard(string email, PaymentCardInputModel paymentCard);
    Task<IEnumerable<PaymentCardDto>> GetStoredPaymentCards(string email);
    Task<PaymentCardDto?> GetPaymentCardById(string email, int id);
}