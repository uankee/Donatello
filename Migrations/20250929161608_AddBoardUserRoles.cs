using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Donatello.Migrations
{
    /// <inheritdoc />
    public partial class AddBoardUserRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BoardUser_Board_BoardId",
                table: "BoardUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_BoardUser_User_UserId",
                table: "BoardUsers");

            migrationBuilder.DeleteData(
                table: "Cards",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Cards",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Cards",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Columns",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Columns",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Columns",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Boards",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "BoardUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Organization",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_BoardUsers_AspNetUsers_UserId",
                table: "BoardUsers",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BoardUsers_Boards_BoardId",
                table: "BoardUsers",
                column: "BoardId",
                principalTable: "Boards",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BoardUsers_AspNetUsers_UserId",
                table: "BoardUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_BoardUsers_Boards_BoardId",
                table: "BoardUsers");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "BoardUsers");

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Organization",
                table: "AspNetUsers");

            migrationBuilder.InsertData(
                table: "Boards",
                columns: new[] { "Id", "Description", "Order", "Title" },
                values: new object[] { 1, "Example board (Donatello)", 0, "Personal Board" });

            migrationBuilder.InsertData(
                table: "Columns",
                columns: new[] { "Id", "AttachmentPath", "BoardId", "Color", "Description", "Order", "Title" },
                values: new object[,]
                {
                    { 1, null, 1, null, null, 0, "To do" },
                    { 2, null, 1, null, null, 1, "In progress" },
                    { 3, null, 1, null, null, 2, "Done" }
                });

            migrationBuilder.InsertData(
                table: "Cards",
                columns: new[] { "Id", "AttachmentPath", "ColumnId", "Description", "DueDate", "IsDone", "Order", "Title" },
                values: new object[,]
                {
                    { 1, null, 1, "Outline tasks and milestones", null, false, 0, "Make project plan" },
                    { 2, null, 1, null, null, false, 1, "Buy milk" },
                    { 3, null, 2, null, null, false, 0, "Implement BoardsController" }
                });

            migrationBuilder.AddForeignKey(
                name: "FK_BoardUser_Board_BoardId",
                table: "BoardUsers",
                column: "BoardId",
                principalTable: "Boards",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BoardUser_User_UserId",
                table: "BoardUsers",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
