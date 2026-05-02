namespace Learnly.Domain.Entities;

public sealed class DbRole
{
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
}

public sealed class DbUser
{
    public int UserId { get; set; }
    public int RoleId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public sealed class DbSubject
{
    public int SubjectId { get; set; }
    public string Name { get; set; } = string.Empty;
}

public sealed class DbLevel
{
    public int LevelId { get; set; }
    public string Name { get; set; } = string.Empty;
}

public sealed class DbTutorOffer
{
    public int TutorOffersId { get; set; }
    public int TutorId { get; set; }
    public int SubjectId { get; set; }
    public int LevelId { get; set; }
    public string LessonType { get; set; } = string.Empty;
    public string? Localization { get; set; }
    public decimal HourlyRate { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public sealed class DbTutorAvailability
{
    public int TutorAvailabilityId { get; set; }
    public int TutorId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string? Localization { get; set; }
}

public sealed class DbLessonRequest
{
    public int RequestId { get; set; }
    public int StudentId { get; set; }
    public int TutorOffersId { get; set; }
    public DateTime RequestedTimeStart { get; set; }
    public DateTime RequestedTimeEnd { get; set; }
    public string Status { get; set; } = "pending";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public sealed class DbLesson
{
    public int LessonId { get; set; }
    public int RequestId { get; set; }
    public string Status { get; set; } = "scheduled";
    public string? MeetingUrl { get; set; }
    public bool IsPaid { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public sealed class DbReview
{
    public int ReviewId { get; set; }
    public int LessonId { get; set; }
    public int TutorId { get; set; }
    public int? StudentId { get; set; }
    public decimal Rate { get; set; }
    public string? Comment { get; set; }
}

public sealed class DbWhiteboard
{
    public int WhiteboardId { get; set; }
    public int LessonId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public sealed class DbConversation
{
    public int ConversationId { get; set; }
    public int TutorId { get; set; }
    public int StudentId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class DbMessage
{
    public int MessageId { get; set; }
    public int ConversationId { get; set; }
    public int SenderId { get; set; }
    public int? WhiteboardId { get; set; }
    public string MessageType { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public string? Content { get; set; }
    public string? FileUrl { get; set; }
    public DateTime SentAt { get; set; }
}
