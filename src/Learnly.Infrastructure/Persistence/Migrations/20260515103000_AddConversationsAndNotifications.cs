using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable disable

namespace Learnly.Infrastructure.Persistence.Migrations;

[DbContext(typeof(LearnlyDbContext))]
[Migration("20260515103000_AddConversationsAndNotifications")]
public partial class AddConversationsAndNotifications : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "tutor_conversations",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                TutorProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                StudentUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_tutor_conversations", x => x.Id);
                table.ForeignKey(
                    name: "FK_tutor_conversations_AspNetUsers_StudentUserId",
                    column: x => x.StudentUserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_tutor_conversations_tutor_profiles_TutorProfileId",
                    column: x => x.TutorProfileId,
                    principalTable: "tutor_profiles",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "user_notifications",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                Type = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                Message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                LinkUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                LessonId = table.Column<Guid>(type: "uuid", nullable: true),
                CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                ReadAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_user_notifications", x => x.Id);
                table.ForeignKey(
                    name: "FK_user_notifications_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "tutor_conversation_messages",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                ConversationId = table.Column<Guid>(type: "uuid", nullable: false),
                SenderUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                Message = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                SentAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_tutor_conversation_messages", x => x.Id);
                table.ForeignKey(
                    name: "FK_tutor_conversation_messages_AspNetUsers_SenderUserId",
                    column: x => x.SenderUserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_tutor_conversation_messages_tutor_conversations_Conversa~",
                    column: x => x.ConversationId,
                    principalTable: "tutor_conversations",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_tutor_conversation_messages_ConversationId_SentAtUtc",
            table: "tutor_conversation_messages",
            columns: new[] { "ConversationId", "SentAtUtc" });

        migrationBuilder.CreateIndex(
            name: "IX_tutor_conversation_messages_SenderUserId",
            table: "tutor_conversation_messages",
            column: "SenderUserId");

        migrationBuilder.CreateIndex(
            name: "IX_tutor_conversations_StudentUserId",
            table: "tutor_conversations",
            column: "StudentUserId");

        migrationBuilder.CreateIndex(
            name: "IX_tutor_conversations_TutorProfileId_StudentUserId",
            table: "tutor_conversations",
            columns: new[] { "TutorProfileId", "StudentUserId" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_user_notifications_Type_LessonId_UserId",
            table: "user_notifications",
            columns: new[] { "Type", "LessonId", "UserId" },
            unique: true,
            filter: "\"LessonId\" IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "IX_user_notifications_UserId_ReadAtUtc",
            table: "user_notifications",
            columns: new[] { "UserId", "ReadAtUtc" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "tutor_conversation_messages");
        migrationBuilder.DropTable(name: "user_notifications");
        migrationBuilder.DropTable(name: "tutor_conversations");
    }
}
