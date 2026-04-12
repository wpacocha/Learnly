namespace Learnly.Application.Abstractions;

public interface ICurrentUser
{
    string? UserId { get; }

    bool IsAuthenticated { get; }
}
