using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Birthday.DAL.Migrations
{
    /// <inheritdoc />
    public partial class RenamedPhotoPath : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PhotoPuth",
                table: "BirthdayPersons",
                newName: "PhotoPath");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PhotoPath",
                table: "BirthdayPersons",
                newName: "PhotoPuth");
        }
    }
}
