using Caesura.Api.Services;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Caesura.Api.Controllers;

/// <summary>Registration, login, and OAuth endpoints.</summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
[Tags("Auth")]
public class AuthController(
    IAuthService authService,
    IValidator<RegisterRequest> registerValidator,
    IValidator<LoginRequest> loginValidator,
    IValidator<GoogleAuthRequest> googleValidator) : ControllerBase
{
    private readonly IAuthService _authService = authService;
    private readonly IValidator<RegisterRequest> _registerValidator = registerValidator;
    private readonly IValidator<LoginRequest> _loginValidator = loginValidator;
    private readonly IValidator<GoogleAuthRequest> _googleValidator = googleValidator;

    /// <summary>Create a new user account.</summary>
    /// <param name="request">Registration payload (email, username, password).</param>
    /// <response code="201">Account created. Returns the new user and JWT token.</response>
    /// <response code="400">Request body is missing or malformed.</response>
    /// <response code="409">Email or username is already taken.</response>
    /// <response code="422">One or more validation errors.</response>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(object), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var check = await this.ValidateAsync(_registerValidator, request);
        if (check is not null) return check;

        try
        {
            var response = await _authService.RegisterAsync(request);
            return CreatedAtAction(nameof(Register), response);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    /// <summary>Authenticate with email and password.</summary>
    /// <param name="request">Login credentials.</param>
    /// <response code="200">Authentication successful. Returns the user and JWT token.</response>
    /// <response code="400">Request body is missing or malformed.</response>
    /// <response code="401">Invalid credentials.</response>
    /// <response code="422">One or more validation errors.</response>
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (request is null)
            return BadRequest(new { error = "Request body is required." });

        var check = await this.ValidateAsync(_loginValidator, request);
        if (check is not null) return check;

        try
        {
            var result = await _authService.LoginAsync(request);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
    }

    /// <summary>Authenticate or create an account via Google OAuth.</summary>
    /// <param name="request">Google ID token payload.</param>
    /// <response code="200">Authentication successful. Returns the user and JWT token.</response>
    /// <response code="400">Request body is missing or malformed.</response>
    /// <response code="401">Google token is invalid or expired.</response>
    [HttpPost("google")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleAuthRequest request)
    {
        if (request is null)
            return BadRequest(new { error = "Request body is required." });

        var check = await this.ValidateAsync(_googleValidator, request);
        if (check is not null) return check;

        try
        {
            var result = await _authService.GoogleLoginAsync(request);
            return Ok(result);
        }
        catch (Exception e)
        {
            return Unauthorized(new { error = e.Message });
        }
    }
}
