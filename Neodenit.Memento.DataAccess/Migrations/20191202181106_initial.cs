using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Neodenit.Memento.DataAccess.Migrations
{
    public partial class initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Answers",
                columns: table => new
                {
                    ID = table.Column<Guid>(nullable: false),
                    Time = table.Column<DateTime>(nullable: false),
                    Owner = table.Column<string>(nullable: true),
                    IsCorrect = table.Column<bool>(nullable: false),
                    ClozeID = table.Column<Guid>(nullable: false),
                    CardID = table.Column<Guid>(nullable: false),
                    DeckID = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Answers", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Decks",
                columns: table => new
                {
                    ID = table.Column<Guid>(nullable: false),
                    IsShared = table.Column<bool>(nullable: false),
                    Title = table.Column<string>(nullable: false),
                    Owner = table.Column<string>(nullable: true),
                    ControlMode = table.Column<int>(nullable: false),
                    DelayMode = table.Column<int>(nullable: false),
                    AllowSmallDelays = table.Column<bool>(nullable: false),
                    StartDelay = table.Column<int>(nullable: false),
                    Coeff = table.Column<double>(nullable: false),
                    PreviewAnswer = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Decks", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Cards",
                columns: table => new
                {
                    ID = table.Column<Guid>(nullable: false),
                    ReadingCardId = table.Column<Guid>(nullable: false),
                    DeckID = table.Column<Guid>(nullable: false),
                    Text = table.Column<string>(nullable: false),
                    Comment = table.Column<string>(nullable: true),
                    IsValid = table.Column<bool>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cards", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Cards_Decks_DeckID",
                        column: x => x.DeckID,
                        principalTable: "Decks",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Clozes",
                columns: table => new
                {
                    ID = table.Column<Guid>(nullable: false),
                    CardID = table.Column<Guid>(nullable: false),
                    Label = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clozes", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Clozes_Cards_CardID",
                        column: x => x.CardID,
                        principalTable: "Cards",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Repetitions",
                columns: table => new
                {
                    ID = table.Column<Guid>(nullable: false),
                    UserName = table.Column<string>(nullable: true),
                    ClozeID = table.Column<Guid>(nullable: false),
                    Position = table.Column<int>(nullable: false),
                    IsNew = table.Column<bool>(nullable: false),
                    LastDelay = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Repetitions", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Repetitions_Clozes_ClozeID",
                        column: x => x.ClozeID,
                        principalTable: "Clozes",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cards_DeckID",
                table: "Cards",
                column: "DeckID");

            migrationBuilder.CreateIndex(
                name: "IX_Clozes_CardID",
                table: "Clozes",
                column: "CardID");

            migrationBuilder.CreateIndex(
                name: "IX_Repetitions_ClozeID",
                table: "Repetitions",
                column: "ClozeID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Answers");

            migrationBuilder.DropTable(
                name: "Repetitions");

            migrationBuilder.DropTable(
                name: "Clozes");

            migrationBuilder.DropTable(
                name: "Cards");

            migrationBuilder.DropTable(
                name: "Decks");
        }
    }
}
