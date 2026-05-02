using System;
using Learnly.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Learnly.Infrastructure.Persistence.Migrations;

/// <summary>
/// Replaces tutor_subjects / tutor_teaching_levels with tutor_teaching_offerings,
/// adds per-offering mode, location, rate, duration; profile becomes first/last/bio only;
/// availability slots reference an offering; migrates existing rows.
/// </summary>
[DbContext(typeof(LearnlyDbContext))]
[Migration("20260502120000_TutorTeachingOfferingsRebuild")]
public partial class TutorTeachingOfferingsRebuild : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            CREATE TABLE tutor_teaching_offerings (
                "Id" uuid NOT NULL,
                "TutorProfileId" uuid NOT NULL,
                "SubjectId" integer NOT NULL,
                "TeachingMode" integer NOT NULL,
                "Location" character varying(300),
                "HourlyRate" numeric(10,2) NOT NULL,
                "DurationMinutes" integer NOT NULL,
                CONSTRAINT "PK_tutor_teaching_offerings" PRIMARY KEY ("Id"),
                CONSTRAINT "FK_tutor_teaching_offerings_subjects_SubjectId" FOREIGN KEY ("SubjectId") REFERENCES subjects ("Id") ON DELETE RESTRICT,
                CONSTRAINT "FK_tutor_teaching_offerings_tutor_profiles_TutorProfileId" FOREIGN KEY ("TutorProfileId") REFERENCES tutor_profiles ("Id") ON DELETE CASCADE
            );

            CREATE INDEX "IX_tutor_teaching_offerings_TutorProfileId" ON tutor_teaching_offerings ("TutorProfileId");
            CREATE INDEX "IX_tutor_teaching_offerings_TutorProfileId_SubjectId" ON tutor_teaching_offerings ("TutorProfileId", "SubjectId");

            CREATE TABLE tutor_offering_levels (
                "TutorTeachingOfferingId" uuid NOT NULL,
                "TeachingLevelId" integer NOT NULL,
                CONSTRAINT "PK_tutor_offering_levels" PRIMARY KEY ("TutorTeachingOfferingId", "TeachingLevelId"),
                CONSTRAINT "FK_tutor_offering_levels_teaching_levels_TeachingLevelId" FOREIGN KEY ("TeachingLevelId") REFERENCES teaching_levels ("Id") ON DELETE RESTRICT,
                CONSTRAINT "FK_tutor_offering_levels_tutor_teaching_offerings_TutorTeachingOfferingId" FOREIGN KEY ("TutorTeachingOfferingId") REFERENCES tutor_teaching_offerings ("Id") ON DELETE CASCADE
            );

            CREATE INDEX "IX_tutor_offering_levels_TeachingLevelId" ON tutor_offering_levels ("TeachingLevelId");

            INSERT INTO tutor_teaching_offerings ("Id", "TutorProfileId", "SubjectId", "TeachingMode", "Location", "HourlyRate", "DurationMinutes")
            SELECT gen_random_uuid(), ts."TutorProfileId", ts."SubjectId", 2,
                NULLIF(trim(both from coalesce(p."Location", '')), ''),
                coalesce(p."HourlyRate", 50),
                60
            FROM tutor_subjects ts
            INNER JOIN tutor_profiles p ON p."Id" = ts."TutorProfileId";

            INSERT INTO tutor_offering_levels ("TutorTeachingOfferingId", "TeachingLevelId")
            SELECT o."Id", ttl."TeachingLevelId"
            FROM tutor_teaching_offerings o
            INNER JOIN tutor_teaching_levels ttl ON ttl."TutorProfileId" = o."TutorProfileId";

            INSERT INTO tutor_offering_levels ("TutorTeachingOfferingId", "TeachingLevelId")
            SELECT o."Id", tl."Id"
            FROM tutor_teaching_offerings o
            CROSS JOIN teaching_levels tl
            WHERE NOT EXISTS (
                SELECT 1 FROM tutor_offering_levels z WHERE z."TutorTeachingOfferingId" = o."Id"
            );

            INSERT INTO tutor_teaching_offerings ("Id", "TutorProfileId", "SubjectId", "TeachingMode", "Location", "HourlyRate", "DurationMinutes")
            SELECT gen_random_uuid(), x."TutorProfileId", (SELECT s."Id" FROM subjects s ORDER BY s."Id" LIMIT 1), 2,
                NULLIF(trim(both from coalesce(p."Location", '')), ''),
                coalesce(p."HourlyRate", 50),
                60
            FROM (
                SELECT DISTINCT tas."TutorProfileId" FROM tutor_availability_slots tas
            ) x
            INNER JOIN tutor_profiles p ON p."Id" = x."TutorProfileId"
            WHERE NOT EXISTS (SELECT 1 FROM tutor_teaching_offerings o2 WHERE o2."TutorProfileId" = x."TutorProfileId");

            INSERT INTO tutor_offering_levels ("TutorTeachingOfferingId", "TeachingLevelId")
            SELECT o."Id", tl."Id"
            FROM tutor_teaching_offerings o
            CROSS JOIN teaching_levels tl
            WHERE NOT EXISTS (
                SELECT 1 FROM tutor_offering_levels z WHERE z."TutorTeachingOfferingId" = o."Id"
            );

            ALTER TABLE tutor_availability_slots ADD COLUMN "TutorTeachingOfferingId" uuid;

            UPDATE tutor_availability_slots s
            SET "TutorTeachingOfferingId" = sub."Id"
            FROM (
                SELECT "Id", "TutorProfileId",
                    ROW_NUMBER() OVER (PARTITION BY "TutorProfileId" ORDER BY "SubjectId", "Id") AS rn
                FROM tutor_teaching_offerings
            ) sub
            WHERE s."TutorProfileId" = sub."TutorProfileId" AND sub.rn = 1;

            DELETE FROM tutor_availability_slots WHERE "TutorTeachingOfferingId" IS NULL;

            ALTER TABLE tutor_availability_slots ALTER COLUMN "TutorTeachingOfferingId" SET NOT NULL;

            ALTER TABLE tutor_availability_slots ADD CONSTRAINT "FK_tutor_availability_slots_tutor_teaching_offerings_TutorTeachingOfferingId"
                FOREIGN KEY ("TutorTeachingOfferingId") REFERENCES tutor_teaching_offerings ("Id") ON DELETE CASCADE;

            UPDATE tutor_availability_slots s
            SET "EndUtc" = s."StartUtc" + (o."DurationMinutes" * interval '1 minute')
            FROM tutor_teaching_offerings o
            WHERE o."Id" = s."TutorTeachingOfferingId";

            ALTER TABLE tutor_profiles ADD COLUMN IF NOT EXISTS "Bio" character varying(4000) NOT NULL DEFAULT '';

            DO $EF$
            BEGIN
                IF EXISTS (
                    SELECT 1 FROM information_schema.columns
                    WHERE table_schema = 'public' AND table_name = 'tutor_profiles' AND column_name = 'Description'
                ) THEN
                    EXECUTE $Q$ UPDATE tutor_profiles SET "Bio" = COALESCE(NULLIF(trim(both from "Bio"), ''), COALESCE("Description", '')) $Q$;
                END IF;
            END $EF$;

            ALTER TABLE tutor_profiles ADD COLUMN IF NOT EXISTS "FirstName" character varying(100) NOT NULL DEFAULT '';
            ALTER TABLE tutor_profiles ADD COLUMN IF NOT EXISTS "LastName" character varying(100) NOT NULL DEFAULT '';

            DO $EF$
            BEGIN
                IF EXISTS (
                    SELECT 1 FROM information_schema.columns
                    WHERE table_schema = 'public' AND table_name = 'tutor_profiles' AND column_name = 'Headline'
                ) THEN
                    EXECUTE $Q$
                    UPDATE tutor_profiles SET
                        "FirstName" = left(split_part(trim(both from coalesce("Headline", '')), ' ', 1), 100),
                        "LastName" = left(
                            nullif(
                                trim(both from substring(trim(both from coalesce("Headline", '')) from length(split_part(trim(both from coalesce("Headline", '')), ' ', 1)) + 2)),
                                ''
                            ),
                            100
                        )
                    WHERE trim(both from coalesce("FirstName", '')) = '' OR trim(both from coalesce("LastName", '')) = ''
                    $Q$;
                END IF;
            END $EF$;

            UPDATE tutor_profiles SET "FirstName" = 'Tutor' WHERE trim(both from "FirstName") = '';

            ALTER TABLE tutor_profiles DROP COLUMN IF EXISTS "Headline";
            ALTER TABLE tutor_profiles DROP COLUMN IF EXISTS "Description";
            ALTER TABLE tutor_profiles DROP COLUMN IF EXISTS "Subject";
            ALTER TABLE tutor_profiles DROP COLUMN IF EXISTS "TeachingLevel";
            ALTER TABLE tutor_profiles DROP COLUMN IF EXISTS "Location";
            ALTER TABLE tutor_profiles DROP COLUMN IF EXISTS "HourlyRate";

            DROP TABLE tutor_subjects;
            DROP TABLE tutor_teaching_levels;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        throw new NotSupportedException("Down is not supported for TutorTeachingOfferingsRebuild.");
    }
}
