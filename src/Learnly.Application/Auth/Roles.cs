namespace Learnly.Application.Auth;

public static class Roles
{
    public const string Student = "Student";
    public const string Tutor = "Tutor";

    public static bool IsDefined(string? role) =>
        string.Equals(role, Student, StringComparison.OrdinalIgnoreCase)
        || string.Equals(role, Tutor, StringComparison.OrdinalIgnoreCase);

    public static bool TryNormalize(string? role, out string normalized)
    {
        if (string.Equals(role, Student, StringComparison.OrdinalIgnoreCase))
        {
            normalized = Student;
            return true;
        }

        if (string.Equals(role, Tutor, StringComparison.OrdinalIgnoreCase))
        {
            normalized = Tutor;
            return true;
        }

        normalized = string.Empty;
        return false;
    }
}
