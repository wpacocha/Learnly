using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Learnly.Infrastructure.Persistence.Migrations;

public partial class AddTutorAssignmentsAndAvailability : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "tutor_availability_slots",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                TutorProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                StartUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                EndUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_tutor_availability_slots", x => x.Id);
                table.ForeignKey(
                    name: "FK_tutor_availability_slots_tutor_profiles_TutorProfileId",
                    column: x => x.TutorProfileId,
                    principalTable: "tutor_profiles",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "tutor_subjects",
            columns: table => new
            {
                TutorProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                SubjectId = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_tutor_subjects", x => new { x.TutorProfileId, x.SubjectId });
                table.ForeignKey(
                    name: "FK_tutor_subjects_subjects_SubjectId",
                    column: x => x.SubjectId,
                    principalTable: "subjects",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_tutor_subjects_tutor_profiles_TutorProfileId",
                    column: x => x.TutorProfileId,
                    principalTable: "tutor_profiles",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "tutor_teaching_levels",
            columns: table => new
            {
                TutorProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                TeachingLevelId = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_tutor_teaching_levels", x => new { x.TutorProfileId, x.TeachingLevelId });
                table.ForeignKey(
                    name: "FK_tutor_teaching_levels_teaching_levels_TeachingLevelId",
                    column: x => x.TeachingLevelId,
                    principalTable: "teaching_levels",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_tutor_teaching_levels_tutor_profiles_TutorProfileId",
                    column: x => x.TutorProfileId,
                    principalTable: "tutor_profiles",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_tutor_availability_slots_TutorProfileId_StartUtc_EndUtc",
            table: "tutor_availability_slots",
            columns: new[] { "TutorProfileId", "StartUtc", "EndUtc" });

        migrationBuilder.CreateIndex(
            name: "IX_tutor_subjects_SubjectId",
            table: "tutor_subjects",
            column: "SubjectId");

        migrationBuilder.CreateIndex(
            name: "IX_tutor_teaching_levels_TeachingLevelId",
            table: "tutor_teaching_levels",
            column: "TeachingLevelId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "tutor_availability_slots");
        migrationBuilder.DropTable(name: "tutor_subjects");
        migrationBuilder.DropTable(name: "tutor_teaching_levels");
    }
}
