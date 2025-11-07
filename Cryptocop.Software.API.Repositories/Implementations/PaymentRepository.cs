using Cryptocop.Software.API.Models.Dtos;
using Cryptocop.Software.API.Models.InputModels;
using Cryptocop.Software.API.Repositories.Interfaces;
using Cryptocop.Software.API.Repositories.Data;
using Cryptocop.Software.API.Repositories.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Cryptocop.Software.API.Repositories.Implementations;

public class PaymentRepository : IPaymentRepository
{
    private readonly CryptocopDbContext _db;
    public PaymentRepository(CryptocopDbContext db)
    {
        _db = db;
    }

    public async Task AddPaymentCardAsync(string email, PaymentCardInputModel paymentCard)
    {
        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email)
                   ?? throw new InvalidOperationException("User not found");

        // Basic validation for payment card
        if (string.IsNullOrWhiteSpace(paymentCard.CardholderName))
        {
            throw new ArgumentException("Cardholder name is required");
        }
        if (!PaymentCardHelper.IsValidLuhn(paymentCard.CardNumber))
        {
            throw new ArgumentException("Invalid card number");
        }
        if (paymentCard.Month < 1 || paymentCard.Month > 12)
        {
            throw new ArgumentException("Invalid expiration month");
        }
        if (paymentCard.Year < 0 || paymentCard.Year > 99)
        {
            throw new ArgumentException("Invalid expiration year");
        }

        _db.PaymentCards.Add(new Entities.PaymentCard
        {
            UserId = user.Id,
            CardholderName = paymentCard.CardholderName.Trim(),
            CardNumber = paymentCard.CardNumber.Trim(), // stored unmasked
            Month = paymentCard.Month,
            Year = paymentCard.Year
        });
        await _db.SaveChangesAsync();
    }

    public async Task<IEnumerable<PaymentCardDto>> GetStoredPaymentCardsAsync(string email)
    {
        var userId = await _db.Users.Where(u => u.Email == email).Select(u => u.Id).FirstOrDefaultAsync();
        if (userId == 0) return Enumerable.Empty<PaymentCardDto>();

        return await _db.PaymentCards
            .Where(c => c.UserId == userId)
            .Select(c => new PaymentCardDto
            {
                Id = c.Id,
                CardholderName = c.CardholderName,
                CardNumber = PaymentCardHelper.MaskPaymentCard(c.CardNumber),
                Month = c.Month,
                Year = c.Year
            })
            .ToListAsync();
    }

    public async Task<PaymentCardDto?> GetPaymentCardByIdAsync(string email, int id)
    {
        var userId = await _db.Users.Where(u => u.Email == email).Select(u => u.Id).FirstOrDefaultAsync();
        if (userId == 0) return null;

        return await _db.PaymentCards
            .Where(c => c.UserId == userId && c.Id == id)
            .Select(c => new PaymentCardDto
            {
                Id = c.Id,
                CardholderName = c.CardholderName,
                CardNumber = c.CardNumber, // unmasked for internal use
                Month = c.Month,
                Year = c.Year
            })
            .FirstOrDefaultAsync();
    }
}