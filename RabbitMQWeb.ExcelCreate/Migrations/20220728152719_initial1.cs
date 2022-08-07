using Microsoft.EntityFrameworkCore.Migrations;

namespace RabbitMQWeb.ExcelCreate.Migrations
{
    public partial class initial1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Userfiles",
                table: "Userfiles");

            migrationBuilder.RenameTable(
                name: "Userfiles",
                newName: "UserFiles");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserFiles",
                table: "UserFiles",
                column: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserFiles",
                table: "UserFiles");

            migrationBuilder.RenameTable(
                name: "UserFiles",
                newName: "Userfiles");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Userfiles",
                table: "Userfiles",
                column: "Id");
        }
    }
}
