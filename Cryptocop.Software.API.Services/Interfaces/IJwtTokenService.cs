namespace Cryptocop.Software.API.Services.Interfaces;

public interface IJwtTokenService
{
    Task<bool> IsTokenBlacklistedAsync(int tokenId);
}