namespace Learnly.Application.Auth.Dtos;

public sealed record AuthResponse(
    string AccessToken,
    DateTimeOffset ExpiresAtUtc,
    string UserId,
    string Email,
    IReadOnlyList<string> Roles);
