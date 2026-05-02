using Learnly.Application.Auth;
using Learnly.Domain.Entities;
using Learnly.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Learnly.Infrastructure.Persistence.Seeding;

/// <summary>
/// Opcjonalne konta testowe (włączane w Development przez Seed:DemoAccounts).
/// Hasło: <see cref="DemoPassword"/>. Idempotentne — ponowny start nie duplikuje użytkowników.
/// </summary>
public static class DemoAccountsSeedData
{
    public const string TutorEmail = "tutor@learnly.demo";
    public const string StudentEmail = "student@learnly.demo";
    public const string DemoPassword = "LearnlyDemo2026!";

    public static async Task EnsureDemoAccountsAsync(
        IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var db = services.GetRequiredService<LearnlyDbContext>();
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(DemoAccountsSeedData));

        if (await userManager.FindByEmailAsync(TutorEmail) is not null)
        {
            return;
        }

        if (!await db.Subjects.AnyAsync(cancellationToken) || !await db.TeachingLevels.AnyAsync(cancellationToken))
        {
            logger.LogWarning("Demo accounts skipped: catalog (subjects/teaching levels) is empty. Run catalog seed first.");
            return;
        }

        var tutor = new ApplicationUser
        {
            UserName = TutorEmail,
            Email = TutorEmail,
            EmailConfirmed = true,
        };

        var tutorCreate = await userManager.CreateAsync(tutor, DemoPassword);
        if (!tutorCreate.Succeeded)
        {
            logger.LogError(
                "Demo tutor not created: {Errors}",
                string.Join("; ", tutorCreate.Errors.Select(e => $"{e.Code}:{e.Description}")));
            return;
        }

        await userManager.AddToRoleAsync(tutor, Roles.Tutor);

        var student = new ApplicationUser
        {
            UserName = StudentEmail,
            Email = StudentEmail,
            EmailConfirmed = true,
        };

        var studentCreate = await userManager.CreateAsync(student, DemoPassword);
        if (!studentCreate.Succeeded)
        {
            logger.LogError(
                "Demo student not created: {Errors}",
                string.Join("; ", studentCreate.Errors.Select(e => $"{e.Code}:{e.Description}")));
        }
        else
        {
            await userManager.AddToRoleAsync(student, Roles.Student);
        }

        var profileId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        db.TutorProfiles.Add(
            new TutorProfile
            {
                Id = profileId,
                UserId = tutor.Id,
                FirstName = "Anna",
                LastName = "Demo",
                Bio =
                    "Konto wygenerowane automatycznie do testów (seed). Uczę od 8 lat, przygotowuję do matury i egzaminów.",
                PhotoUrl = null,
                CreatedAtUtc = now,
                UpdatedAtUtc = now,
            });

        var subjectIds = await db.Subjects.OrderBy(s => s.Id).Take(4).Select(s => s.Id).ToListAsync(cancellationToken);
        var levelIds = await db.TeachingLevels.OrderBy(l => l.Id).Take(3).Select(l => l.Id).ToListAsync(cancellationToken);

        TutorTeachingOffering? firstOfferingForSlots = null;
        foreach (var sid in subjectIds)
        {
            var offering = new TutorTeachingOffering
            {
                Id = Guid.NewGuid(),
                TutorProfileId = profileId,
                SubjectId = sid,
                TeachingMode = TeachingMode.Both,
                Location = "Warszawa",
                HourlyRate = 95,
                DurationMinutes = 60,
            };
            foreach (var lid in levelIds)
            {
                offering.OfferingLevels.Add(new TutorOfferingLevel
                {
                    TutorTeachingOffering = offering,
                    TeachingLevelId = lid,
                });
            }

            db.TutorTeachingOfferings.Add(offering);
            firstOfferingForSlots ??= offering;
        }

        await db.SaveChangesAsync(cancellationToken);

        try
        {
            if (firstOfferingForSlots is not null)
            {
                for (var i = 1; i <= 5; i++)
                {
                    var startUtc = DateTimeOffset.UtcNow.AddDays(i + 2).AddHours(8 + i);
                    db.TutorAvailabilitySlots.Add(
                        new TutorAvailabilitySlot
                        {
                            Id = Guid.NewGuid(),
                            TutorProfileId = profileId,
                            TutorTeachingOfferingId = firstOfferingForSlots.Id,
                            StartUtc = startUtc,
                            EndUtc = startUtc.AddMinutes(firstOfferingForSlots.DurationMinutes),
                        });
                }

                await db.SaveChangesAsync(cancellationToken);
            }
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pg && pg.SqlState == "42P01")
        {
            logger.LogWarning(
                ex,
                "Brak tabeli tutor_availability_slots — uruchom migracje EF lub pełny database_learnly.sql. Demo tutor bez slotów.");
        }
        catch (PostgresException ex) when (ex.SqlState == "42P01")
        {
            logger.LogWarning(
                ex,
                "Brak tabeli tutor_availability_slots — uruchom migracje EF lub pełny database_learnly.sql. Demo tutor bez slotów.");
        }

        logger.LogInformation("Demo accounts seeded for local testing: {Tutor}, {Student}.", TutorEmail, StudentEmail);
    }
}
