using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookStore.Migrations
{
    public partial class FixSpelling : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PhoneNmber",
                table: "OrderHeaders",
                newName: "PhoneNumber");

            migrationBuilder.RenameColumn(
                name: "StreetAdress",
                table: "Companies",
                newName: "StreetAddress");

            migrationBuilder.RenameColumn(
                name: "StreetAdress",
                table: "AspNetUsers",
                newName: "StreetAddress");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PhoneNumber",
                table: "OrderHeaders",
                newName: "PhoneNmber");

            migrationBuilder.RenameColumn(
                name: "StreetAddress",
                table: "Companies",
                newName: "StreetAdress");

            migrationBuilder.RenameColumn(
                name: "StreetAddress",
                table: "AspNetUsers",
                newName: "StreetAdress");
        }
    }
}
