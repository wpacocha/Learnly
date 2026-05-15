using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Learnly.Infrastructure.Persistence.Migrations;

[DbContext(typeof(LearnlyDbContext))]
[Migration("20260515114500_AddMissingSubjectAliases")]
public partial class AddMissingSubjectAliases : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            INSERT INTO subjects ("Name", "Slug", "IsLanguage")
            VALUES
                ('Język japoński', 'jezyk-japonski', TRUE),
                ('Język włoski', 'jezyk-wloski', TRUE),
                ('Bazy Danych', 'bazy-danych', FALSE),
                ('Rachunek różniczkowy', 'rachunek-rozniczkowy', FALSE),
                ('Geometria', 'geometria', FALSE),
                ('Przyroda', 'przyroda', FALSE)
            ON CONFLICT ("Slug") DO UPDATE
            SET "Name" = EXCLUDED."Name",
                "IsLanguage" = EXCLUDED."IsLanguage";
            """);

        migrationBuilder.Sql("""
            WITH aliases("CanonicalSlug", "AliasName") AS (
                VALUES
                    ('jezyk-japonski', 'j. japonski'),
                    ('jezyk-japonski', 'j. japoński'),
                    ('jezyk-japonski', 'japanese'),
                    ('jezyk-japonski', 'japanese language'),
                    ('jezyk-wloski', 'j. wloski'),
                    ('jezyk-wloski', 'j. włoski'),
                    ('jezyk-wloski', 'italian'),
                    ('jezyk-wloski', 'italian language'),
                    ('bazy-danych', 'database'),
                    ('bazy-danych', 'databases'),
                    ('rachunek-rozniczkowy', 'rachunek różniczkowy'),
                    ('rachunek-rozniczkowy', 'differential calculus'),
                    ('geometria', 'geometry'),
                    ('przyroda', 'nature')
            )
            UPDATE tutor_teaching_offerings o
            SET "SubjectId" = canonical."Id"
            FROM subjects alias
            JOIN aliases map
              ON lower(alias."Name") = map."AliasName"
              OR lower(alias."Slug") = replace(map."AliasName", ' ', '-')
              OR lower(alias."Slug") = replace(replace(map."AliasName", 'j. ', 'j-'), ' ', '-')
            JOIN subjects canonical ON lower(canonical."Slug") = map."CanonicalSlug"
            WHERE o."SubjectId" = alias."Id"
              AND alias."Id" <> canonical."Id";
            """);

        migrationBuilder.Sql("""
            WITH aliases("CanonicalSlug", "AliasName") AS (
                VALUES
                    ('jezyk-japonski', 'j. japonski'),
                    ('jezyk-japonski', 'j. japoński'),
                    ('jezyk-japonski', 'japanese'),
                    ('jezyk-japonski', 'japanese language'),
                    ('jezyk-wloski', 'j. wloski'),
                    ('jezyk-wloski', 'j. włoski'),
                    ('jezyk-wloski', 'italian'),
                    ('jezyk-wloski', 'italian language'),
                    ('bazy-danych', 'database'),
                    ('bazy-danych', 'databases'),
                    ('rachunek-rozniczkowy', 'rachunek różniczkowy'),
                    ('rachunek-rozniczkowy', 'differential calculus'),
                    ('geometria', 'geometry'),
                    ('przyroda', 'nature')
            )
            DELETE FROM subjects s
            USING aliases map
            WHERE (lower(s."Name") = map."AliasName"
                   OR lower(s."Slug") = replace(map."AliasName", ' ', '-')
                   OR lower(s."Slug") = replace(replace(map."AliasName", 'j. ', 'j-'), ' ', '-'))
              AND lower(s."Slug") <> map."CanonicalSlug"
              AND EXISTS (SELECT 1 FROM subjects canonical WHERE lower(canonical."Slug") = map."CanonicalSlug");
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
    }
}
