using Cryptocop.Software.API.Models.Dtos;
using Cryptocop.Software.API.Models.InputModels;

namespace Cryptocop.Software.API.Services.Interfaces;

public interface IAccountService
{
    Task<UserDto> CreateUserAsync(RegisterInputModel inputModel);
    Task<UserDto> AuthenticateUserAsync(LoginInputModel loginInputModel);
    Task LogoutAsync(int tokenId);
    Task<UserDto?> GetUserByEmailAsync(string email);
    Task UpdateFullNameAsync(string email, string fullName);
}