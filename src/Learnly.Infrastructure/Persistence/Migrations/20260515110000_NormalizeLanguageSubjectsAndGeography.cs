using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Learnly.Infrastructure.Persistence.Migrations;

[DbContext(typeof(LearnlyDbContext))]
[Migration("20260515110000_NormalizeLanguageSubjectsAndGeography")]
public partial class NormalizeLanguageSubjectsAndGeography : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            UPDATE subjects
            SET "IsLanguage" = TRUE
            WHERE lower("Name") LIKE 'j.%'
               OR lower("Name") LIKE 'jezyk %'
               OR lower("Name") LIKE 'język %';
            """);

        migrationBuilder.Sql("""
            UPDATE tutor_teaching_offerings
            SET "SubjectId" = (SELECT "Id" FROM subjects WHERE "Slug" = 'geografia' ORDER BY "Id" LIMIT 1)
            WHERE "SubjectId" IN (SELECT "Id" FROM subjects WHERE lower("Name") = 'geography' OR "Slug" = 'geography')
              AND EXISTS (SELECT 1 FROM subjects WHERE "Slug" = 'geografia');

            DELETE FROM subjects
            WHERE (lower("Name") = 'geography' OR "Slug" = 'geography')
              AND EXISTS (SELECT 1 FROM subjects WHERE "Slug" = 'geografia');
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
    }
}
