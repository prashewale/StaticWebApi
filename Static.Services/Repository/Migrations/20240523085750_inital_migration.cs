using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Static.Services.Repository.Migrations
{
    /// <inheritdoc />
    public partial class inital_migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Instruments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InstrumentName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Exchange = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Instruments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OptionExpiries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InstrumentId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OptionExpiries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OptionExpiries_Instruments_InstrumentId",
                        column: x => x.InstrumentId,
                        principalTable: "Instruments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Strikes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StrikePrice = table.Column<long>(type: "bigint", nullable: false),
                    OptionExpiryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Strikes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Strikes_OptionExpiries_OptionExpiryId",
                        column: x => x.OptionExpiryId,
                        principalTable: "OptionExpiries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OptionCandles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Duration = table.Column<int>(type: "int", nullable: false),
                    StrikeId = table.Column<int>(type: "int", nullable: false),
                    OptionType = table.Column<int>(type: "int", nullable: false),
                    ClosePrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Delta = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Gamma = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ImpliedFuture = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ImpliedValume = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Rho = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Theta = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TimeStamp = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OptionCandles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OptionCandles_Strikes_StrikeId",
                        column: x => x.StrikeId,
                        principalTable: "Strikes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OptionCandles_StrikeId",
                table: "OptionCandles",
                column: "StrikeId");

            migrationBuilder.CreateIndex(
                name: "IX_OptionExpiries_InstrumentId",
                table: "OptionExpiries",
                column: "InstrumentId");

            migrationBuilder.CreateIndex(
                name: "IX_Strikes_OptionExpiryId",
                table: "Strikes",
                column: "OptionExpiryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OptionCandles");

            migrationBuilder.DropTable(
                name: "Strikes");

            migrationBuilder.DropTable(
                name: "OptionExpiries");

            migrationBuilder.DropTable(
                name: "Instruments");
        }
    }
}
