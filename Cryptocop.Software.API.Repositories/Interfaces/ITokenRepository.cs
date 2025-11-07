using Cryptocop.Software.API.Models.Dtos;

namespace Cryptocop.Software.API.Repositories.Interfaces;

public interface ITokenRepository
{
    Task<JwtTokenDto> CreateNewTokenAsync();
    Task<bool> IsTokenBlacklistedAsync(int tokenId);
    Task VoidTokenAsync(int tokenId);
}