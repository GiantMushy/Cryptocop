using Cryptocop.Software.API.Models.Dtos;
using Cryptocop.Software.API.Repositories.Interfaces;
using Cryptocop.Software.API.Repositories.Data;
using Microsoft.EntityFrameworkCore;

namespace Cryptocop.Software.API.Repositories.Implementations;

public class TokenRepository : ITokenRepository
{
    private readonly CryptocopDbContext _db;

    public TokenRepository(CryptocopDbContext db)
    {
        _db = db;
    }

    public Task<JwtTokenDto> CreateNewToken()
    {
        var entity = new Entities.Token { IsBlacklisted = false };
        _db.Tokens.Add(entity);
        _db.SaveChanges();
        return Task.FromResult(new JwtTokenDto { Id = entity.Id });
    }

    public Task<bool> IsTokenBlacklisted(int tokenId)
    {
        var isBlacklisted = _db.Tokens.AsNoTracking().Any(t => t.Id == tokenId && t.IsBlacklisted);
        return Task.FromResult(isBlacklisted);
    }

    public Task VoidToken(int tokenId)
    {
        var token = _db.Tokens.FirstOrDefault(t => t.Id == tokenId);
        if (token != null && !token.IsBlacklisted)
        {
            token.IsBlacklisted = true;
            _db.SaveChanges();
        }
        return Task.CompletedTask;
    }
}