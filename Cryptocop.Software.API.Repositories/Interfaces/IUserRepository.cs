using Cryptocop.Software.API.Models.Dtos;
using Cryptocop.Software.API.Models.InputModels;

namespace Cryptocop.Software.API.Repositories.Interfaces;

public interface IUserRepository
{
    Task<UserDto> CreateUser(RegisterInputModel inputModel);
    Task<UserDto> AuthenticateUser(LoginInputModel loginInputModel);
    Task<UserDto?> GetByEmail(string email);
    Task UpdateFullName(string email, string fullName);
}