using Learnly.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Learnly.Infrastructure.Persistence.Seeding;

/// <summary>
/// Idempotentny katalog przedmiotów i poziomów — wymagany przez wyszukiwarkę i przypisania tutora.
/// </summary>
public static class CatalogSeedData
{
    public static async Task EnsureSubjectsAndLevelsAsync(LearnlyDbContext db, CancellationToken cancellationToken = default)
    {
        if (!await db.Subjects.AnyAsync(cancellationToken))
        {
            var subjects = new[]
            {
                ("Matematyka", "matematyka"),
                ("Język angielski", "jezyk-angielski"),
                ("Język polski", "jezyk-polski"),
                ("Fizyka", "fizyka"),
                ("Chemia", "chemia"),
                ("Biologia", "biologia"),
                ("Informatyka", "informatyka"),
                ("Historia", "historia"),
                ("Geografia", "geografia"),
                ("WOS", "wos"),
            };

            foreach (var (name, slug) in subjects)
            {
                db.Subjects.Add(new Subject { Name = name, Slug = slug });
            }

            await db.SaveChangesAsync(cancellationToken);
        }

        if (!await db.TeachingLevels.AnyAsync(cancellationToken))
        {
            var levels = new[]
            {
                "Szkoła podstawowa (klasy 4–8)",
                "Egzamin ósmoklasisty",
                "Liceum / technikum",
                "Matura — poziom podstawowy",
                "Matura — poziom rozszerzony",
                "Studia",
                "Kurs dla dorosłych",
            };

            foreach (var name in levels)
            {
                db.TeachingLevels.Add(new TeachingLevel { Name = name });
            }

            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
