using Learnly.Application.Auth.Dtos;

namespace Learnly.Application.Auth;

public sealed record AuthResult(bool Succeeded, AuthResponse? Response, IReadOnlyList<string> Errors)
{
    public static AuthResult Success(AuthResponse response) =>
        new(true, response, Array.Empty<string>());

    public static AuthResult Failure(params string[] errors) =>
        new(false, null, errors);

    public static AuthResult Failure(IEnumerable<string> errors) =>
        new(false, null, errors.ToArray());
}
