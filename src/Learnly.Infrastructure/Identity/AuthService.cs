using Learnly.Application.Auth;
using Learnly.Application.Auth.Dtos;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Learnly.Infrastructure.Identity;

public sealed class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly JwtOptions _jwtOptions;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IJwtTokenGenerator jwtTokenGenerator,
        IOptions<JwtOptions> jwtOptions,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtTokenGenerator = jwtTokenGenerator;
        _jwtOptions = jwtOptions.Value;
        _logger = logger;
    }

    public async Task<AuthResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;

        if (!Roles.TryNormalize(request.Role, out var normalizedRole))
        {
            return AuthResult.Failure("Role must be Student or Tutor.");
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            return AuthResult.Failure(createResult.Errors.Select(e => e.Description));
        }

        var addRole = await _userManager.AddToRoleAsync(user, normalizedRole);
        if (!addRole.Succeeded)
        {
            _logger.LogWarning("User {UserId} created but role assignment failed: {Errors}", user.Id, string.Join("; ", addRole.Errors.Select(e => e.Description)));
            await _userManager.DeleteAsync(user);
            return AuthResult.Failure(addRole.Errors.Select(e => e.Description));
        }

        return await BuildAuthResponseAsync(user);
    }

    public async Task<AuthResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return AuthResult.Failure("Invalid login attempt.");
        }

        var signIn = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (!signIn.Succeeded)
        {
            return AuthResult.Failure("Invalid login attempt.");
        }

        return await BuildAuthResponseAsync(user);
    }

    private async Task<AuthResult> BuildAuthResponseAsync(ApplicationUser user)
    {
        var email = user.Email ?? string.Empty;
        var roles = (await _userManager.GetRolesAsync(user)).ToArray();
        var token = _jwtTokenGenerator.CreateAccessToken(user.Id, email, roles);
        var expires = DateTimeOffset.UtcNow.AddMinutes(_jwtOptions.AccessTokenMinutes);

        return AuthResult.Success(new AuthResponse(
            token,
            expires,
            user.Id,
            email,
            roles));
    }
}

