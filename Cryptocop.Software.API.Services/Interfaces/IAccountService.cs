using Cryptocop.Software.API.Models.Dtos;
using Cryptocop.Software.API.Models.InputModels;

namespace Cryptocop.Software.API.Services.Interfaces;

public interface IAccountService
{
    Task<UserDto> CreateUser(RegisterInputModel inputModel);
    Task<UserDto> AuthenticateUser(LoginInputModel loginInputModel);
    Task Logout(int tokenId);
    Task<UserDto?> GetUserByEmail(string email);
    Task UpdateFullName(string email, string fullName);
}