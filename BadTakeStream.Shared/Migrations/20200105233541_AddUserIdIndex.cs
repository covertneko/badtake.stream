using Microsoft.EntityFrameworkCore.Migrations;

namespace BadTakeStream.Shared.Migrations
{
    public partial class AddUserIdIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Tweets_UserId",
                table: "Tweets",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tweets_UserId",
                table: "Tweets");
        }
    }
}
