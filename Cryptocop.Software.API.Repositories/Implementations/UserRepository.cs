using Cryptocop.Software.API.Models.Dtos;
using Cryptocop.Software.API.Models.InputModels;
using Cryptocop.Software.API.Repositories.Interfaces;
using Cryptocop.Software.API.Repositories.Helpers;
using Cryptocop.Software.API.Repositories.Data;
using Microsoft.EntityFrameworkCore;

namespace Cryptocop.Software.API.Repositories.Implementations;

public class UserRepository : IUserRepository
{
    private readonly CryptocopDbContext _db;
    public UserRepository(CryptocopDbContext db)
    {
        _db = db;
    }

    public async Task<UserDto> CreateUserAsync(RegisterInputModel inputModel)
    {
        var exists = await _db.Users.AnyAsync(u => u.Email == inputModel.Email);
        if (exists)
        {
            throw new InvalidOperationException("A user with this email already exists.");
        }

        var user = new Entities.User
        {
            Email = inputModel.Email.Trim(),
            FullName = inputModel.FullName.Trim(),
            PasswordHash = HashingHelper.HashPassword(inputModel.Password)
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        // Create a new token record per assignment spec and return its id
        var token = new Entities.Token { IsBlacklisted = false };
        _db.Tokens.Add(token);
        await _db.SaveChangesAsync();

        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            TokenId = token.Id
        };
    }

    public async Task<UserDto> AuthenticateUserAsync(LoginInputModel loginInputModel)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == loginInputModel.Email)
                   ?? throw new UnauthorizedAccessException("Invalid credentials.");

        var hash = HashingHelper.HashPassword(loginInputModel.Password);
        if (!string.Equals(user.PasswordHash, hash, StringComparison.Ordinal))
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        // Create a new token for this authentication event
        var token = new Entities.Token { IsBlacklisted = false };
        _db.Tokens.Add(token);
        await _db.SaveChangesAsync();

        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            TokenId = token.Id
        };
    }

    public async Task<UserDto?> GetByEmailAsync(string email)
    {
        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email);
        if (user is null) return null;
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            TokenId = 0
        };
    }

    public async Task UpdateFullNameAsync(string email, string fullName)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email) ?? throw new InvalidOperationException("User not found");
        user.FullName = fullName.Trim();
        await _db.SaveChangesAsync();
    }
}