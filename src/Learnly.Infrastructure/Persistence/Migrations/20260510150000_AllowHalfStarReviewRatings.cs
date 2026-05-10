using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Learnly.Infrastructure.Persistence.Migrations;

public partial class AllowHalfStarReviewRatings : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<double>(
            name: "Rating",
            table: "reviews_v2",
            type: "double precision",
            nullable: false,
            oldClrType: typeof(int),
            oldType: "integer");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<int>(
            name: "Rating",
            table: "reviews_v2",
            type: "integer",
            nullable: false,
            oldClrType: typeof(double),
            oldType: "double precision");
    }
}
