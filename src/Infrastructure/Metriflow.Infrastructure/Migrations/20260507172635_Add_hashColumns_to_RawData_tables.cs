using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Metriflow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_hashColumns_to_RawData_tables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "Hash",
                table: "PSIRecords",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000")
            );

            migrationBuilder.AddColumn<Guid>(
                name: "Hash",
                table: "GARecords",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000")
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "Hash", table: "PSIRecords");

            migrationBuilder.DropColumn(name: "Hash", table: "GARecords");
        }
    }
}
