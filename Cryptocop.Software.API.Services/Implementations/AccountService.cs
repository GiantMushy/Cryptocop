using Cryptocop.Software.API.Models.Dtos;
using Cryptocop.Software.API.Models.InputModels;
using Cryptocop.Software.API.Services.Interfaces;
using Cryptocop.Software.API.Repositories.Interfaces;

namespace Cryptocop.Software.API.Services.Implementations;

public class AccountService : IAccountService
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenRepository _tokenRepository;

    public AccountService(IUserRepository userRepository, ITokenRepository tokenRepository)
    {
        _userRepository = userRepository;
        _tokenRepository = tokenRepository;
    }

    public Task<UserDto> CreateUserAsync(RegisterInputModel inputModel)
        => _userRepository.CreateUserAsync(inputModel);

    public Task<UserDto> AuthenticateUserAsync(LoginInputModel loginInputModel)
        => _userRepository.AuthenticateUserAsync(loginInputModel);

    public Task LogoutAsync(int tokenId)
    {
        return _tokenRepository.VoidTokenAsync(tokenId);
    }

    public Task<UserDto?> GetUserByEmailAsync(string email)
        => _userRepository.GetByEmailAsync(email);

    public Task UpdateFullNameAsync(string email, string fullName)
        => _userRepository.UpdateFullNameAsync(email, fullName);
}