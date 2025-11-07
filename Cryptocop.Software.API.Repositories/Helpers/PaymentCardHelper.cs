namespace Cryptocop.Software.API.Repositories.Helpers;

public class PaymentCardHelper
{
    public static string MaskPaymentCard(string paymentCardNumber)
    {
        if (string.IsNullOrWhiteSpace(paymentCardNumber)) return string.Empty;
        var digits = new string(paymentCardNumber.Where(char.IsDigit).ToArray());
        if (digits.Length <= 4) return new string('*', digits.Length);
        var visible = digits[^4..];
        return new string('*', digits.Length - 4) + visible;
    }

    public static bool IsValidLuhn(string? number)
    {
        if (string.IsNullOrWhiteSpace(number)) return false;
        var digits = new string(number.Where(char.IsDigit).ToArray());
        if (digits.Length < 12) return false;
        int sum = 0; bool alt = false;
        for (int i = digits.Length - 1; i >= 0; i--)
        {
            int n = digits[i] - '0';
            if (alt)
            {
                n *= 2;
                if (n > 9) n -= 9;
            }
            sum += n;
            alt = !alt;
        }
        return sum % 10 == 0;
    }
}