using Microsoft.EntityFrameworkCore.Migrations;

namespace WizLib_DataAccess.Migrations
{
    public partial class changeBookDetailNumberOfChapters : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "NameOfChapters",
                table: "BookDetails",
                newName: "NumberOfChapters");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "NumberOfChapters",
                table: "BookDetails",
                newName: "NameOfChapters");
        }
    }
}
