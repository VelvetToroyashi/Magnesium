using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Magnesium.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Preferences",
                columns: table => new
                {
                    ID = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Notify = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Preferences", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Channels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Global = table.Column<bool>(type: "INTEGER", nullable: false),
                    GuildID = table.Column<ulong>(type: "INTEGER", nullable: false),
                    ChannelID = table.Column<ulong>(type: "INTEGER", nullable: false),
                    TrackingUserID = table.Column<ulong>(type: "INTEGER", nullable: true),
                    TrackingUserPreferenceID = table.Column<ulong>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Channels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Channels_Preferences_TrackingUserPreferenceID",
                        column: x => x.TrackingUserPreferenceID,
                        principalTable: "Preferences",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Channels_TrackingUserPreferenceID",
                table: "Channels",
                column: "TrackingUserPreferenceID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Channels");

            migrationBuilder.DropTable(
                name: "Preferences");
        }
    }
}
