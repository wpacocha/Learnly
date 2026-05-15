using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Learnly.Infrastructure.Persistence.Migrations;

[DbContext(typeof(LearnlyDbContext))]
[Migration("20260515113000_NormalizeAllSubjectAliases")]
public partial class NormalizeAllSubjectAliases : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            WITH aliases("CanonicalSlug", "AliasSlug") AS (
                VALUES
                    ('matematyka', 'math'),
                    ('matematyka', 'mathematics'),
                    ('jezyk-angielski', 'english'),
                    ('jezyk-angielski', 'english-language'),
                    ('jezyk-angielski', 'j-angielski'),
                    ('jezyk-polski', 'polish'),
                    ('jezyk-polski', 'polish-language'),
                    ('jezyk-polski', 'j-polski'),
                    ('jezyk-niemiecki', 'german'),
                    ('jezyk-niemiecki', 'german-language'),
                    ('jezyk-niemiecki', 'j-niemiecki'),
                    ('jezyk-hiszpanski', 'spanish'),
                    ('jezyk-hiszpanski', 'spanish-language'),
                    ('jezyk-hiszpanski', 'j-hiszpanski'),
                    ('fizyka', 'physics'),
                    ('chemia', 'chemistry'),
                    ('biologia', 'biology'),
                    ('informatyka', 'computer-science'),
                    ('informatyka', 'informatics'),
                    ('historia', 'history'),
                    ('geografia', 'geography'),
                    ('wos', 'civics'),
                    ('wos', 'social-studies'),
                    ('wos', 'wiedza-o-spoleczenstwie')
            )
            UPDATE tutor_teaching_offerings o
            SET "SubjectId" = canonical."Id"
            FROM subjects alias
            JOIN aliases map
              ON lower(alias."Slug") = map."AliasSlug"
              OR lower(alias."Name") = map."AliasSlug"
              OR lower(alias."Name") = replace(map."AliasSlug", '-', ' ')
            JOIN subjects canonical ON lower(canonical."Slug") = map."CanonicalSlug"
            WHERE o."SubjectId" = alias."Id"
              AND alias."Id" <> canonical."Id";
            """);

        migrationBuilder.Sql("""
            WITH aliases("CanonicalSlug", "AliasSlug") AS (
                VALUES
                    ('matematyka', 'math'),
                    ('matematyka', 'mathematics'),
                    ('jezyk-angielski', 'english'),
                    ('jezyk-angielski', 'english-language'),
                    ('jezyk-angielski', 'j-angielski'),
                    ('jezyk-polski', 'polish'),
                    ('jezyk-polski', 'polish-language'),
                    ('jezyk-polski', 'j-polski'),
                    ('jezyk-niemiecki', 'german'),
                    ('jezyk-niemiecki', 'german-language'),
                    ('jezyk-niemiecki', 'j-niemiecki'),
                    ('jezyk-hiszpanski', 'spanish'),
                    ('jezyk-hiszpanski', 'spanish-language'),
                    ('jezyk-hiszpanski', 'j-hiszpanski'),
                    ('fizyka', 'physics'),
                    ('chemia', 'chemistry'),
                    ('biologia', 'biology'),
                    ('informatyka', 'computer-science'),
                    ('informatyka', 'informatics'),
                    ('historia', 'history'),
                    ('geografia', 'geography'),
                    ('wos', 'civics'),
                    ('wos', 'social-studies'),
                    ('wos', 'wiedza-o-spoleczenstwie')
            )
            DELETE FROM subjects s
            USING aliases map
            WHERE (lower(s."Slug") = map."AliasSlug"
                   OR lower(s."Name") = map."AliasSlug"
                   OR lower(s."Name") = replace(map."AliasSlug", '-', ' '))
              AND EXISTS (SELECT 1 FROM subjects canonical WHERE lower(canonical."Slug") = map."CanonicalSlug");
            """);

        migrationBuilder.Sql("""
            UPDATE subjects
            SET "IsLanguage" = TRUE
            WHERE lower("Slug") IN ('jezyk-angielski', 'jezyk-polski', 'jezyk-niemiecki', 'jezyk-hiszpanski')
               OR lower("Name") LIKE 'j.%'
               OR lower("Name") LIKE 'jezyk %'
               OR lower("Name") LIKE 'język %';
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
    }
}
