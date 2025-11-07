using Cryptocop.Software.API.Repositories.Interfaces;
using Cryptocop.Software.API.Services.Interfaces;

namespace Cryptocop.Software.API.Services.Implementations;

public class JwtTokenService : IJwtTokenService
{
    private readonly ITokenRepository _tokenRepository;

    public JwtTokenService(ITokenRepository tokenRepository)
    {
        _tokenRepository = tokenRepository;
    }

    public Task<bool> IsTokenBlacklisted(int tokenId)
        => _tokenRepository.IsTokenBlacklisted(tokenId);
}