using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoWalletApi.Migrations
{
    /// <inheritdoc />
    public partial class ImageNewPropertyName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UploadedByUid",
                table: "Images",
                newName: "UploadedBy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UploadedBy",
                table: "Images",
                newName: "UploadedByUid");
        }
    }
}
