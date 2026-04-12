namespace Learnly.Domain.Entities;

public sealed class Subject
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Slug { get; set; }
}
