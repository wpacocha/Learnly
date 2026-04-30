using System.ComponentModel.DataAnnotations;

namespace Learnly.Application.SqlV2.Dtos;

public sealed record SqlUserDto(
    int UserId,
    int RoleId,
    string Email,
    string Name,
    string Surname);

public sealed class CreateSqlUserRequest
{
    [Required]
    public int RoleId { get; set; }

    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Surname { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    [MaxLength(255)]
    public string Password { get; set; } = string.Empty;
}
