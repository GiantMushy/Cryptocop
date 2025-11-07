using Cryptocop.Software.API.Models.Dtos;

namespace Cryptocop.Software.API.Repositories.Interfaces;

public interface ITokenRepository
{
    Task<JwtTokenDto> CreateNewToken();
    Task<bool> IsTokenBlacklisted(int tokenId);
    Task VoidToken(int tokenId);
}