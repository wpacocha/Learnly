using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Learnly.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class UpdateTutorProfileFields : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Bio",
            table: "tutor_profiles");

        migrationBuilder.DropColumn(
            name: "Headline",
            table: "tutor_profiles");

        migrationBuilder.AddColumn<string>(
            name: "Description",
            table: "tutor_profiles",
            type: "character varying(4000)",
            maxLength: 4000,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "FirstName",
            table: "tutor_profiles",
            type: "character varying(100)",
            maxLength: 100,
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<string>(
            name: "LastName",
            table: "tutor_profiles",
            type: "character varying(100)",
            maxLength: 100,
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<string>(
            name: "Subject",
            table: "tutor_profiles",
            type: "character varying(200)",
            maxLength: 200,
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<string>(
            name: "TeachingLevel",
            table: "tutor_profiles",
            type: "character varying(200)",
            maxLength: 200,
            nullable: false,
            defaultValue: "");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Description",
            table: "tutor_profiles");

        migrationBuilder.DropColumn(
            name: "FirstName",
            table: "tutor_profiles");

        migrationBuilder.DropColumn(
            name: "LastName",
            table: "tutor_profiles");

        migrationBuilder.DropColumn(
            name: "Subject",
            table: "tutor_profiles");

        migrationBuilder.DropColumn(
            name: "TeachingLevel",
            table: "tutor_profiles");

        migrationBuilder.AddColumn<string>(
            name: "Bio",
            table: "tutor_profiles",
            type: "character varying(4000)",
            maxLength: 4000,
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<string>(
            name: "Headline",
            table: "tutor_profiles",
            type: "character varying(200)",
            maxLength: 200,
            nullable: false,
            defaultValue: "");
    }
}
