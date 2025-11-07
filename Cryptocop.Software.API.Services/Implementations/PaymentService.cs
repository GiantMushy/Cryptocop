using Cryptocop.Software.API.Models.Dtos;
using Cryptocop.Software.API.Models.InputModels;
using Cryptocop.Software.API.Services.Interfaces;
using Cryptocop.Software.API.Repositories.Interfaces;

namespace Cryptocop.Software.API.Services.Implementations;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _repo;

    public PaymentService(IPaymentRepository repo)
    {
        _repo = repo;
    }

    public Task AddPaymentCard(string email, PaymentCardInputModel paymentCard)
        => _repo.AddPaymentCard(email, paymentCard);

    public Task<IEnumerable<PaymentCardDto>> GetStoredPaymentCards(string email)
        => _repo.GetStoredPaymentCards(email);
}