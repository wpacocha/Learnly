using Learnly.Domain.Entities;
using Learnly.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Learnly.Infrastructure.Persistence;

public sealed class LearnlyDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
{
    public LearnlyDbContext(DbContextOptions<LearnlyDbContext> options)
        : base(options)
    {
    }

    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<TeachingLevel> TeachingLevels => Set<TeachingLevel>();
    public DbSet<TutorProfile> TutorProfiles => Set<TutorProfile>();
    public DbSet<TutorSubject> TutorSubjects => Set<TutorSubject>();
    public DbSet<TutorTeachingLevel> TutorTeachingLevels => Set<TutorTeachingLevel>();
    public DbSet<TutorAvailabilitySlot> TutorAvailabilitySlots => Set<TutorAvailabilitySlot>();
    public DbSet<Lesson> Lessons => Set<Lesson>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<LessonMessage> LessonMessages => Set<LessonMessage>();
    public DbSet<WhiteboardEvent> WhiteboardEvents => Set<WhiteboardEvent>();

    // SQL schema 1:1 model (Schema-backup.txt)
    public DbSet<DbRole> DbRoles => Set<DbRole>();
    public DbSet<DbUser> DbUsers => Set<DbUser>();
    public DbSet<DbSubject> DbSubjects => Set<DbSubject>();
    public DbSet<DbLevel> DbLevels => Set<DbLevel>();
    public DbSet<DbTutorOffer> DbTutorOffers => Set<DbTutorOffer>();
    public DbSet<DbTutorAvailability> DbTutorAvailabilities => Set<DbTutorAvailability>();
    public DbSet<DbLessonRequest> DbLessonRequests => Set<DbLessonRequest>();
    public DbSet<DbLesson> DbLessons => Set<DbLesson>();
    public DbSet<DbReview> DbReviews => Set<DbReview>();
    public DbSet<DbWhiteboard> DbWhiteboards => Set<DbWhiteboard>();
    public DbSet<DbConversation> DbConversations => Set<DbConversation>();
    public DbSet<DbMessage> DbMessages => Set<DbMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LearnlyDbContext).Assembly);
    }
}
