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

    public Task<UserDto> CreateUser(RegisterInputModel inputModel)
        => _userRepository.CreateUser(inputModel);

    public Task<UserDto> AuthenticateUser(LoginInputModel loginInputModel)
        => _userRepository.AuthenticateUser(loginInputModel);

    public Task Logout(int tokenId)
        => _tokenRepository.VoidToken(tokenId);

    public Task<UserDto?> GetUserByEmail(string email)
        => _userRepository.GetByEmail(email);

    public Task UpdateFullName(string email, string fullName)
        => _userRepository.UpdateFullName(email, fullName);
}