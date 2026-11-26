using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MovieVault.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddReviewsAndMediaOutlets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MediaOutlets",
                columns: table => new
                {
                    MediaOutletId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    OutletType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Website = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaOutlets", x => x.MediaOutletId);
                });

            migrationBuilder.CreateTable(
                name: "Reviewers",
                columns: table => new
                {
                    ReviewerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MediaOutletId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reviewers", x => x.ReviewerId);
                    table.ForeignKey(
                        name: "FK_Reviewers_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Reviewers_MediaOutlets_MediaOutletId",
                        column: x => x.MediaOutletId,
                        principalTable: "MediaOutlets",
                        principalColumn: "MediaOutletId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Reviews",
                columns: table => new
                {
                    ReviewId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Headline = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Body = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MovieId = table.Column<int>(type: "int", nullable: false),
                    ReviewerId = table.Column<int>(type: "int", nullable: false),
                    MediaOutletId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reviews", x => x.ReviewId);
                    table.CheckConstraint("CK_Review_Rating", "[Rating] BETWEEN 1 AND 5");
                    table.ForeignKey(
                        name: "FK_Reviews_MediaOutlets_MediaOutletId",
                        column: x => x.MediaOutletId,
                        principalTable: "MediaOutlets",
                        principalColumn: "MediaOutletId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reviews_Movies_MovieId",
                        column: x => x.MovieId,
                        principalTable: "Movies",
                        principalColumn: "MovieId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reviews_Reviewers_ReviewerId",
                        column: x => x.ReviewerId,
                        principalTable: "Reviewers",
                        principalColumn: "ReviewerId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "MediaOutlets",
                columns: new[] { "MediaOutletId", "Name", "OutletType", "Website" },
                values: new object[,]
                {
                    { 1, "The Morning Ledger", "Newspaper", "https://morningledger.example" },
                    { 2, "Channel Nord", "TvChannel", "https://channelnord.example" },
                    { 3, "Screenwise Magazine", "Magazine", "https://screenwise.example" }
                });

            migrationBuilder.InsertData(
                table: "Reviewers",
                columns: new[] { "ReviewerId", "MediaOutletId", "Name", "UserId" },
                values: new object[,]
                {
                    { 1, 1, "Rita Reel", null },
                    { 2, 2, "Jonas Frame", null },
                    { 3, 1, "Maja Lens", null },
                    { 4, 3, "Peter Cutts", null }
                });

            migrationBuilder.InsertData(
                table: "Reviews",
                columns: new[] { "ReviewId", "Body", "Headline", "MediaOutletId", "MovieId", "PublishedAt", "Rating", "ReviewerId" },
                values: new object[,]
                {
                    { 1, "The Last Lighthouse builds its tension slowly and never lets go. Mikkel Strand carries the film with a restrained, powerful lead performance.", "A quiet storm of a film", 1, 1, new DateTime(2026, 3, 13, 8, 0, 0, 0, DateTimeKind.Utc), 5, 1 },
                    { 2, "Stunning coastal photography and a strong cast. The middle act drags, but the ending is worth the wait.", "Beautiful, if a little long", 2, 1, new DateTime(2026, 3, 14, 18, 30, 0, 0, DateTimeKind.Utc), 4, 2 },
                    { 3, "Few films look this good, but the plot is thin. Worth seeing on a big screen for the visuals alone.", "Atmosphere over story", 3, 1, new DateTime(2026, 3, 20, 10, 0, 0, 0, DateTimeKind.Utc), 3, 4 },
                    { 4, "Echoes of Neon is a feast for the eyes, but the mystery is easy to solve long before the detectives do.", "Style without much substance", 1, 2, new DateTime(2026, 5, 23, 7, 45, 0, 0, DateTimeKind.Utc), 3, 1 },
                    { 5, "Glass Harbor is a confident small-town drama with an excellent ensemble and a script that trusts its audience.", "A harbour full of secrets", 1, 3, new DateTime(2026, 8, 8, 9, 15, 0, 0, DateTimeKind.Utc), 4, 3 },
                    { 6, "Big ideas and a big budget, but Paper Satellites never decides what kind of film it wants to be.", "Lost in orbit", 2, 5, new DateTime(2026, 10, 3, 19, 0, 0, 0, DateTimeKind.Utc), 2, 2 },
                    { 7, "Northbound is funny, touching and refreshingly short. A good choice for the whole family.", "A warm road movie", 1, 4, new DateTime(2026, 9, 19, 8, 0, 0, 0, DateTimeKind.Utc), 4, 1 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_MediaOutlets_Name",
                table: "MediaOutlets",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reviewers_MediaOutletId",
                table: "Reviewers",
                column: "MediaOutletId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviewers_UserId",
                table: "Reviewers",
                column: "UserId",
                unique: true,
                filter: "[UserId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_MediaOutletId",
                table: "Reviews",
                column: "MediaOutletId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_MovieId",
                table: "Reviews",
                column: "MovieId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_ReviewerId",
                table: "Reviews",
                column: "ReviewerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Reviews");

            migrationBuilder.DropTable(
                name: "Reviewers");

            migrationBuilder.DropTable(
                name: "MediaOutlets");
        }
    }
}
