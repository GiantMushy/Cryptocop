using Cryptocop.Software.API.Models.Dtos;
using Cryptocop.Software.API.Models.InputModels;

namespace Cryptocop.Software.API.Repositories.Interfaces;

public interface IUserRepository
{
    Task<UserDto> CreateUserAsync(RegisterInputModel inputModel);
    Task<UserDto> AuthenticateUserAsync(LoginInputModel loginInputModel);
    Task<UserDto?> GetByEmailAsync(string email);
    Task UpdateFullNameAsync(string email, string fullName);
}