using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable disable

namespace Learnly.Infrastructure.Persistence.Migrations;

[DbContext(typeof(LearnlyDbContext))]
[Migration("20260515100000_AddSubjectLanguageFlagAndPolishLevels")]
public partial class AddSubjectLanguageFlagAndPolishLevels : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "IsLanguage",
            table: "subjects",
            type: "boolean",
            nullable: false,
            defaultValue: false);

        migrationBuilder.Sql("""
            UPDATE subjects
            SET "IsLanguage" = TRUE
            WHERE "Slug" IN ('jezyk-angielski', 'jezyk-polski', 'jezyk-niemiecki', 'jezyk-hiszpanski')
               OR lower("Name") LIKE 'j.%'
               OR lower("Name") LIKE 'jezyk %'
               OR lower("Name") LIKE 'język %';
            """);

        migrationBuilder.Sql("""
            UPDATE subjects SET "Name" = 'Jezyk angielski' WHERE "Slug" = 'jezyk-angielski';
            UPDATE subjects SET "Name" = 'Jezyk polski' WHERE "Slug" = 'jezyk-polski';
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

        migrationBuilder.Sql("""
            SELECT setval(
                pg_get_serial_sequence('subjects', 'Id'),
                GREATEST(COALESCE((SELECT MAX("Id") FROM subjects), 1), 1),
                true
            );
            """);

        migrationBuilder.Sql("""
            INSERT INTO subjects ("Name", "Slug", "IsLanguage")
            VALUES
                ('Jezyk niemiecki', 'jezyk-niemiecki', TRUE),
                ('Jezyk hiszpanski', 'jezyk-hiszpanski', TRUE)
            ON CONFLICT ("Slug") DO UPDATE
            SET "Name" = EXCLUDED."Name",
                "IsLanguage" = EXCLUDED."IsLanguage";
            """);

        migrationBuilder.Sql("""
            UPDATE teaching_levels SET "Name" = 'Szkola podstawowa (1-3)' WHERE "Name" = 'Primary School (1-3)';
            UPDATE teaching_levels SET "Name" = 'Szkola podstawowa (4-8)' WHERE "Name" = 'Primary School (4-6)';
            UPDATE teaching_levels SET "Name" = 'Egzamin osmoklasisty' WHERE "Name" = 'Primary School (7-8)';
            UPDATE teaching_levels SET "Name" = 'Szkola srednia' WHERE "Name" = 'High School';
            UPDATE teaching_levels SET "Name" = 'Studia' WHERE "Name" = 'University';
            UPDATE teaching_levels SET "Name" = 'Poziom maturalny' WHERE "Name" = 'Matura basic';
            UPDATE teaching_levels SET "Name" = 'Poziom maturalny rozszerzony' WHERE "Name" = 'Matura extended';
            UPDATE teaching_levels SET "Name" = 'Szkola podstawowa (4-8)' WHERE "Name" IN ('Szkoła podstawowa (klasy 4-8)', 'Szkoła podstawowa (klasy 4–8)');
            UPDATE teaching_levels SET "Name" = 'Egzamin osmoklasisty' WHERE "Name" = 'Egzamin ósmoklasisty';
            UPDATE teaching_levels SET "Name" = 'Szkola srednia' WHERE "Name" = 'Liceum / technikum';
            UPDATE teaching_levels SET "Name" = 'Poziom maturalny' WHERE "Name" = 'Matura — poziom podstawowy';
            UPDATE teaching_levels SET "Name" = 'Poziom maturalny rozszerzony' WHERE "Name" = 'Matura — poziom rozszerzony';
            UPDATE teaching_levels SET "Name" = 'Kurs dla doroslych' WHERE "Name" = 'Kurs dla dorosłych';
            """);

        migrationBuilder.Sql("""
            SELECT setval(
                pg_get_serial_sequence('teaching_levels', 'Id'),
                GREATEST(COALESCE((SELECT MAX("Id") FROM teaching_levels), 1), 1),
                true
            );
            """);

        migrationBuilder.Sql("""
            INSERT INTO teaching_levels ("Name")
            VALUES
                ('Szkola podstawowa (1-3)'),
                ('Szkola podstawowa (4-8)'),
                ('Egzamin osmoklasisty'),
                ('Szkola srednia'),
                ('Poziom maturalny'),
                ('Poziom maturalny rozszerzony'),
                ('Studia'),
                ('Kurs dla doroslych'),
                ('A1'),
                ('A2'),
                ('B1'),
                ('B2'),
                ('C1'),
                ('C2')
            ON CONFLICT ("Name") DO NOTHING;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "IsLanguage",
            table: "subjects");
    }
}
