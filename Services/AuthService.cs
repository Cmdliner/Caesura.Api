using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Caesura.Api.Services;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);

    Task<AuthResponse> GoogleLoginAsync(GoogleAuthRequest request);
}

public class AuthService(AppDbContext db, IConfiguration config) : IAuthService
{
    private readonly PasswordHasher<User> _hasher = new();

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var emailInUse = await db.Users.AnyAsync(u => u.Email == request.Email);
        if (emailInUse) throw new InvalidOperationException("Email already registered");

        var usernameInUse = await db.Users.AnyAsync(u => u.Username == request.Username);
        if (usernameInUse) throw new InvalidOperationException("Username already taken");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            Username = request.Username,
            DisplayName = request.DisplayName,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var account = new Account
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Provider = "email",
            CreatedAt = DateTime.UtcNow
        };

        account.PasswordHash = _hasher.HashPassword(user, request.Password);

        await using var tx = await db.Database.BeginTransactionAsync();
        db.Users.Add(user);
        db.Accounts.Add(account);

        await db.SaveChangesAsync();
        await tx.CommitAsync();

        return BuildResponse(user);

        throw new NotImplementedException();
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        const string badCredentials = "Invalid email or password";
        var user = await db.Users
            .Include(u => u.Accounts)
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user is null) throw new UnauthorizedAccessException(badCredentials);

        var account = user.Accounts.FirstOrDefault(a => a.Provider == "email");
        if (account?.PasswordHash is null)
            throw new UnauthorizedAccessException("This account uses Google sign-in. Please sign in with google");

        var result = _hasher.VerifyHashedPassword(user, account.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed) throw new UnauthorizedAccessException(badCredentials);

        return BuildResponse(user);
    }

    public async Task<AuthResponse> GoogleLoginAsync(GoogleAuthRequest request)
    {
        GoogleJsonWebSignature.Payload payload;

        try
        {
            payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken,
                new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = [config["Google:ClientId"]],
                });
        }
        catch (Exception _)
        {
            throw new UnauthorizedAccessException("Invalid Google token");
        }

        // If returning google user
        var existingAccount = await db.Accounts
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.Provider == "google" && a.ProviderUserId == payload.Subject);

        if (existingAccount is not null) return BuildResponse(existingAccount.User);

        var existingUser = await db.Users.FirstOrDefaultAsync(u => u.Email == payload.Email);

        // User previously registered with email && password; now you can link google as a sign-in method
        if (existingUser is not null)
        {
            db.Accounts.Add(new Account
            {
                Id = Guid.NewGuid(),
                UserId = existingUser.Id,
                Provider = "google",
                ProviderUserId = payload.Subject,
                CreatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
            return BuildResponse(existingUser);
        }


        // Totally new user
        var username = await GenerateUniqueUsernameAsync(payload.Name);

        var newUser = new User
        {
            Id = Guid.NewGuid(),
            Username = username,
            Email = payload.Email,
            DisplayName = payload.Name,
            AvatarUrl = payload.Picture,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await using var tx = await db.Database.BeginTransactionAsync();
        db.Users.Add(newUser);
        db.Accounts.Add(new Account
        {
            Id = Guid.NewGuid(),
            UserId = newUser.Id,
            Provider = "google",
            ProviderUserId = payload.Subject,
            CreatedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync();

        await tx.CommitAsync();
        return BuildResponse(newUser);
    }

    
    // Helper fns
    private AuthResponse BuildResponse(User user) => new()
    {
        Token = BuildJwt(user),
        User = new UserResponse
        {
            Id = user.Id,
            Email = user.Email,
            Username = user.Username,
            DisplayName = user.DisplayName,
            AvatarUrl = user.AvatarUrl,
            Bio = user.Bio,
            CreatedAt = user.CreatedAt
        }
    };

    private string BuildJwt(User user)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(config["Jwt:Secret"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("username", user.Username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(30),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task<string> GenerateUniqueUsernameAsync(string name)
    {
        var base_ = new string(name.ToLower()
            .Where(char.IsLetterOrDigit)
            .Take(20)
            .ToArray());

        if (string.IsNullOrEmpty(base_)) base_ = "user";

        var username = base_;
        var suffix = 0;

        while (await db.Users.AnyAsync(u => u.Username == username))
        {
            suffix++;
            username = $"{base_}{suffix}";
        }

        return username;
    }
}