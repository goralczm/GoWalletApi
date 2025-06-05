using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoWalletApi.Migrations
{
    /// <inheritdoc />
    public partial class ObjectKeyProperty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "S3Key",
                table: "Images",
                newName: "ObjectKey");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ObjectKey",
                table: "Images",
                newName: "S3Key");
        }
    }
}
