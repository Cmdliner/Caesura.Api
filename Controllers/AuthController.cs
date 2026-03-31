using Caesura.Api.Services;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Caesura.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController(
    IAuthService authService, 
    IValidator<RegisterRequest> registerValidator,
    IValidator<LoginRequest> loginValidator,
    IValidator<GoogleAuthRequest> googleValidator): ControllerBase
{
    private readonly IAuthService _authService = authService;

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request) 
    {
        var check = await this.ValidateAsync(registerValidator, request);
        if (check is not null) return check;

        try
        {
            var response = await authService.RegisterAsync(request);
            return CreatedAtAction(nameof(Register), response);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request) 
    {
        var check = await this.ValidateAsync(loginValidator, request);
        if (check is not null) return check;

        try
        {
            var result = await this._authService.LoginAsync(request);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
    }

    [HttpPost("google")]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleAuthRequest request)
    {
        var check = await this.ValidateAsync(googleValidator, request);
        if(check is not null) return check;

        try
        {
            var result = this._authService.GoogleLoginAsync(request);
            return Ok(result);
        }
        catch (Exception e)
        {
            return Unauthorized(new { error = e.Message });
        }
    }
}