using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Cryptocop.Software.API.Models.InputModels;
using Cryptocop.Software.API.Services.Interfaces;
using Cryptocop.Software.API.Models.Dtos;
using System.Security.Claims;

namespace Cryptocop.Software.API.Controllers;

[Route("api/account")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly IAccountService _accountService;
    private readonly ITokenService _tokenService;

    public AccountController(IAccountService accountService, ITokenService tokenService)
    {
        _accountService = accountService;
        _tokenService = tokenService;
    }

    // POST /api/account/register
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterInputModel input)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        var user = await _accountService.CreateUserAsync(input);
        return Created($"/api/account/{user.Id}", new { user.Id, user.FullName, user.Email });
    }

    // POST /api/account/signin
    [HttpPost("signin")]
    [AllowAnonymous]
    public async Task<IActionResult> SignIn([FromBody] LoginInputModel input)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        try
        {
            var user = await _accountService.AuthenticateUserAsync(input);
            var token = await _tokenService.GenerateJwtTokenAsync(user);
            return Ok(new JwtTokenDto { Token = token });
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
    }

    // GET /api/account/signout
    [HttpGet("signout")]
    [Authorize]
    public new IActionResult SignOut()
    {
        var tokenIdValue = User.FindFirst("tokenId")?.Value;
        if (string.IsNullOrWhiteSpace(tokenIdValue) || !int.TryParse(tokenIdValue, out var tokenId))
        {
            return Unauthorized();
        }

        _accountService.LogoutAsync(tokenId).Wait();
        return NoContent();
    }

    private string? GetEmail()
    {
        return User.FindFirst("email")?.Value
            ?? User.FindFirst(ClaimTypes.Email)?.Value
            ?? User.FindFirst("emails")?.Value;
    }

    // GET /api/account/me
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me()
    {
        var email = GetEmail();
        if (string.IsNullOrWhiteSpace(email)) return Unauthorized();
        var user = await _accountService.GetUserByEmailAsync(email);
        if (user == null) return NotFound();
        return Ok(new { email = user.Email, fullName = user.FullName });
    }

    public class UpdateProfileInputModel
    {
        [System.ComponentModel.DataAnnotations.Required]
        public string FullName { get; set; } = string.Empty;
    }

    // PATCH /api/account/profile
    [HttpPatch("profile")]
    [Authorize]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileInputModel input)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        var email = GetEmail();
        if (string.IsNullOrWhiteSpace(email)) return Unauthorized();
        await _accountService.UpdateFullNameAsync(email, input.FullName);
        return NoContent();
    }
}