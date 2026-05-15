using Learnly.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Learnly.Infrastructure.Persistence.Seeding;

/// <summary>
/// Idempotent catalog of subjects and teaching levels used by tutor search and tutor assignments.
/// </summary>
public static class CatalogSeedData
{
    public static async Task EnsureSubjectsAndLevelsAsync(LearnlyDbContext db, CancellationToken cancellationToken = default)
    {
        var subjects = new[]
        {
            ("Matematyka", "matematyka", false),
            ("Jezyk angielski", "jezyk-angielski", true),
            ("Jezyk polski", "jezyk-polski", true),
            ("Jezyk niemiecki", "jezyk-niemiecki", true),
            ("Jezyk hiszpanski", "jezyk-hiszpanski", true),
            ("Język japoński", "jezyk-japonski", true),
            ("Język włoski", "jezyk-wloski", true),
            ("Fizyka", "fizyka", false),
            ("Chemia", "chemia", false),
            ("Biologia", "biologia", false),
            ("Informatyka", "informatyka", false),
            ("Historia", "historia", false),
            ("Geografia", "geografia", false),
            ("WOS", "wos", false),
            ("Bazy Danych", "bazy-danych", false),
            ("Rachunek różniczkowy", "rachunek-rozniczkowy", false),
            ("Geometria", "geometria", false),
            ("Przyroda", "przyroda", false),
        };

        foreach (var (name, slug, isLanguage) in subjects)
        {
            var aliases = GetSubjectAliases(slug);
            var subject = await db.Subjects.FirstOrDefaultAsync(
                s => s.Slug == slug || s.Name == name || aliases.Contains(s.Slug) || aliases.Contains(s.Name.ToLower()),
                cancellationToken);
            if (subject is null)
            {
                db.Subjects.Add(new Subject { Name = name, Slug = slug, IsLanguage = isLanguage });
                continue;
            }

            subject.Name = name;
            subject.Slug = slug;
            subject.IsLanguage = isLanguage;
        }

        var languageSubjects = await db.Subjects
            .Where(s => s.Name.ToLower().StartsWith("j.") || s.Name.ToLower().StartsWith("jezyk "))
            .ToListAsync(cancellationToken);
        foreach (var subject in languageSubjects)
        {
            subject.IsLanguage = true;
        }

        var levelRenames = new Dictionary<string, string>
        {
            ["Primary School (1-3)"] = "Szkola podstawowa (1-3)",
            ["Primary School (4-6)"] = "Szkola podstawowa (4-8)",
            ["Primary School (7-8)"] = "Egzamin osmoklasisty",
            ["High School"] = "Szkola srednia",
            ["University"] = "Studia",
            ["Matura basic"] = "Poziom maturalny",
            ["Matura extended"] = "Poziom maturalny rozszerzony",
            ["Szkoła podstawowa (klasy 4-8)"] = "Szkola podstawowa (4-8)",
            ["Szkoła podstawowa (klasy 4–8)"] = "Szkola podstawowa (4-8)",
            ["Egzamin ósmoklasisty"] = "Egzamin osmoklasisty",
            ["Liceum / technikum"] = "Szkola srednia",
            ["Matura — poziom podstawowy"] = "Poziom maturalny",
            ["Matura — poziom rozszerzony"] = "Poziom maturalny",
            ["Kurs dla dorosłych"] = "Kurs dla doroslych",
        };

        var existingLevels = await db.TeachingLevels.ToListAsync(cancellationToken);
        foreach (var level in existingLevels)
        {
            if (levelRenames.TryGetValue(level.Name, out var replacement))
            {
                level.Name = replacement;
            }
        }

        var levels = new[]
        {
            "Szkola podstawowa (1-3)",
            "Szkola podstawowa (4-8)",
            "Egzamin osmoklasisty",
            "Szkola srednia",
            "Poziom maturalny",
            "Poziom maturalny rozszerzony",
            "Studia",
            "Kurs dla doroslych",
            "A1",
            "A2",
            "B1",
            "B2",
            "C1",
            "C2",
        };

        foreach (var name in levels)
        {
            if (!await db.TeachingLevels.AnyAsync(l => l.Name == name, cancellationToken))
            {
                db.TeachingLevels.Add(new TeachingLevel { Name = name });
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private static string[] GetSubjectAliases(string slug) => slug switch
    {
        "matematyka" => ["math", "mathematics"],
        "jezyk-angielski" => ["english", "english language", "english-language", "j. angielski", "j-angielski"],
        "jezyk-polski" => ["polish", "polish language", "polish-language", "j. polski", "j-polski"],
        "jezyk-niemiecki" => ["german", "german language", "german-language", "j. niemiecki", "j-niemiecki"],
        "jezyk-hiszpanski" => ["spanish", "spanish language", "spanish-language", "j. hiszpanski", "j-hiszpanski"],
        "jezyk-japonski" => ["japanese", "japanese language", "j. japonski", "j. japoński", "j-japonski"],
        "jezyk-wloski" => ["italian", "italian language", "j. wloski", "j. włoski", "j-wloski"],
        "fizyka" => ["physics"],
        "chemia" => ["chemistry"],
        "biologia" => ["biology"],
        "informatyka" => ["computer science", "computer-science", "informatics"],
        "historia" => ["history"],
        "geografia" => ["geography"],
        "wos" => ["civics", "social studies", "social-studies", "wiedza o spoleczenstwie", "wiedza-o-spoleczenstwie"],
        "bazy-danych" => ["database", "databases"],
        "rachunek-rozniczkowy" => ["differential calculus", "rachunek różniczkowy"],
        "geometria" => ["geometry"],
        "przyroda" => ["nature"],
        _ => [],
    };
}
