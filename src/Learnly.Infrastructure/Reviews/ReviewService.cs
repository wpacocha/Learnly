using Learnly.Application.Abstractions;
using Learnly.Application.Reviews;
using Learnly.Application.Reviews.Dtos;
using Learnly.Domain.Entities;
using Learnly.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Learnly.Infrastructure.Reviews;

public sealed class ReviewService : IReviewService
{
    private readonly LearnlyDbContext _db;
    private readonly ICurrentUser _currentUser;

    public ReviewService(LearnlyDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<(ReviewDto? review, string? error)> CreateAsync(
        CreateReviewRequestDto dto,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.UserId;
        if (userId is null)
        {
            return (null, "User is not authenticated.");
        }

        var lesson = await _db.Lessons
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == dto.LessonId, cancellationToken);
        if (lesson is null)
        {
            return (null, "Lesson not found.");
        }

        if (lesson.StudentUserId != userId)
        {
            return (null, "Only the lesson student can add a review.");
        }

        if (lesson.Status != LessonStatus.Completed)
        {
            return (null, "Review can be added only for completed lessons.");
        }

        var exists = await _db.Reviews.AnyAsync(x => x.LessonId == dto.LessonId, cancellationToken);
        if (exists)
        {
            return (null, "Review for this lesson already exists.");
        }

        var review = new Review
        {
            Id = Guid.NewGuid(),
            LessonId = lesson.Id,
            TutorProfileId = lesson.TutorProfileId,
            StudentUserId = userId,
            Rating = dto.Rating,
            Comment = string.IsNullOrWhiteSpace(dto.Comment) ? null : dto.Comment.Trim(),
            CreatedAtUtc = DateTimeOffset.UtcNow
        };

        _db.Reviews.Add(review);
        await _db.SaveChangesAsync(cancellationToken);
        return (Map(review), null);
    }

    public async Task<IReadOnlyList<ReviewDto>> GetForTutorAsync(
        Guid tutorProfileId,
        CancellationToken cancellationToken = default)
    {
        return await _db.Reviews
            .AsNoTracking()
            .Where(x => x.TutorProfileId == tutorProfileId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => Map(x))
            .ToListAsync(cancellationToken);
    }

    private static ReviewDto Map(Review x) =>
        new(x.Id, x.LessonId, x.TutorProfileId, x.StudentUserId, x.Rating, x.Comment, x.CreatedAtUtc);
}
