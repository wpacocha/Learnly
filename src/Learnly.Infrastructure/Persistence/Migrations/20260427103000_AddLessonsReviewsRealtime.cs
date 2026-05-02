using Learnly.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Learnly.Infrastructure.Persistence.Migrations;

[DbContext(typeof(LearnlyDbContext))]
[Migration("20260427103000_AddLessonsReviewsRealtime")]
public partial class AddLessonsReviewsRealtime : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "lessons",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                StudentUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                TutorProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                TutorAvailabilitySlotId = table.Column<Guid>(type: "uuid", nullable: false),
                StartUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                EndUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                Status = table.Column<int>(type: "integer", nullable: false),
                CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_lessons", x => x.Id);
                table.ForeignKey(
                    name: "FK_lessons_AspNetUsers_StudentUserId",
                    column: x => x.StudentUserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_lessons_tutor_availability_slots_TutorAvailabilitySlotId",
                    column: x => x.TutorAvailabilitySlotId,
                    principalTable: "tutor_availability_slots",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_lessons_tutor_profiles_TutorProfileId",
                    column: x => x.TutorProfileId,
                    principalTable: "tutor_profiles",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "lesson_messages",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                LessonId = table.Column<Guid>(type: "uuid", nullable: false),
                SenderUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                Message = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                SentAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_lesson_messages", x => x.Id);
                table.ForeignKey(
                    name: "FK_lesson_messages_AspNetUsers_SenderUserId",
                    column: x => x.SenderUserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_lesson_messages_lessons_LessonId",
                    column: x => x.LessonId,
                    principalTable: "lessons",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "reviews_v2",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                LessonId = table.Column<Guid>(type: "uuid", nullable: false),
                TutorProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                StudentUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                Rating = table.Column<int>(type: "integer", nullable: false),
                Comment = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_reviews_v2", x => x.Id);
                table.ForeignKey(
                    name: "FK_reviews_v2_AspNetUsers_StudentUserId",
                    column: x => x.StudentUserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_reviews_v2_lessons_LessonId",
                    column: x => x.LessonId,
                    principalTable: "lessons",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_reviews_v2_tutor_profiles_TutorProfileId",
                    column: x => x.TutorProfileId,
                    principalTable: "tutor_profiles",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "whiteboard_events",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                LessonId = table.Column<Guid>(type: "uuid", nullable: false),
                SenderUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                EventType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                PayloadJson = table.Column<string>(type: "character varying(20000)", maxLength: 20000, nullable: false),
                CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_whiteboard_events", x => x.Id);
                table.ForeignKey(
                    name: "FK_whiteboard_events_AspNetUsers_SenderUserId",
                    column: x => x.SenderUserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_whiteboard_events_lessons_LessonId",
                    column: x => x.LessonId,
                    principalTable: "lessons",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_lesson_messages_LessonId_SentAtUtc",
            table: "lesson_messages",
            columns: new[] { "LessonId", "SentAtUtc" });

        migrationBuilder.CreateIndex(
            name: "IX_lesson_messages_SenderUserId",
            table: "lesson_messages",
            column: "SenderUserId");

        migrationBuilder.CreateIndex(
            name: "IX_lessons_StudentUserId",
            table: "lessons",
            column: "StudentUserId");

        migrationBuilder.CreateIndex(
            name: "IX_lessons_TutorAvailabilitySlotId",
            table: "lessons",
            column: "TutorAvailabilitySlotId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_lessons_TutorProfileId",
            table: "lessons",
            column: "TutorProfileId");

        migrationBuilder.CreateIndex(
            name: "IX_reviews_v2_LessonId",
            table: "reviews_v2",
            column: "LessonId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_reviews_v2_StudentUserId",
            table: "reviews_v2",
            column: "StudentUserId");

        migrationBuilder.CreateIndex(
            name: "IX_reviews_v2_TutorProfileId",
            table: "reviews_v2",
            column: "TutorProfileId");

        migrationBuilder.CreateIndex(
            name: "IX_whiteboard_events_LessonId_CreatedAtUtc",
            table: "whiteboard_events",
            columns: new[] { "LessonId", "CreatedAtUtc" });

        migrationBuilder.CreateIndex(
            name: "IX_whiteboard_events_SenderUserId",
            table: "whiteboard_events",
            column: "SenderUserId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "lesson_messages");
        migrationBuilder.DropTable(name: "reviews_v2");
        migrationBuilder.DropTable(name: "whiteboard_events");
        migrationBuilder.DropTable(name: "lessons");
    }
}
