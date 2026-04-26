using System.ComponentModel.DataAnnotations;

namespace Learnly.Application.Auth.Dtos;

public sealed class RegisterRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;

    /// <summary>Must be <see cref="Roles.Student"/> or <see cref="Roles.Tutor"/>.</summary>
    [Required]
    public string Role { get; set; } = string.Empty;
}
