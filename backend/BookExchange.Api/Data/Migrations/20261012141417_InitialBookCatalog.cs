using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookExchange.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialBookCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Authors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Authors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BookCatalogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ISBN13 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ISBN10 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Publisher = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PublishedYear = table.Column<int>(type: "int", nullable: true),
                    PageCount = table.Column<int>(type: "int", nullable: true),
                    CoverImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GoogleBooksId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookCatalogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BookCatalogAuthors",
                columns: table => new
                {
                    AuthorsId = table.Column<int>(type: "int", nullable: false),
                    BooksId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookCatalogAuthors", x => new { x.AuthorsId, x.BooksId });
                    table.ForeignKey(
                        name: "FK_BookCatalogAuthors_Authors_AuthorsId",
                        column: x => x.AuthorsId,
                        principalTable: "Authors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookCatalogAuthors_BookCatalogs_BooksId",
                        column: x => x.BooksId,
                        principalTable: "BookCatalogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BookCatalogGenres",
                columns: table => new
                {
                    BooksId = table.Column<int>(type: "int", nullable: false),
                    GenresId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookCatalogGenres", x => new { x.BooksId, x.GenresId });
                    table.ForeignKey(
                        name: "FK_BookCatalogGenres_BookCatalogs_BooksId",
                        column: x => x.BooksId,
                        principalTable: "BookCatalogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookCatalogGenres_Genres_GenresId",
                        column: x => x.GenresId,
                        principalTable: "Genres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Listings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookCatalogId = table.Column<int>(type: "int", nullable: false),
                    OwnerId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ConditionId = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Listings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Listings_AspNetUsers_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Listings_BookCatalogs_BookCatalogId",
                        column: x => x.BookCatalogId,
                        principalTable: "BookCatalogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Listings_Condition_ConditionId",
                        column: x => x.ConditionId,
                        principalTable: "Condition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BookCatalogAuthors_BooksId",
                table: "BookCatalogAuthors",
                column: "BooksId");

            migrationBuilder.CreateIndex(
                name: "IX_BookCatalogGenres_GenresId",
                table: "BookCatalogGenres",
                column: "GenresId");

            migrationBuilder.CreateIndex(
                name: "IX_BookCatalogs_GoogleBooksId",
                table: "BookCatalogs",
                column: "GoogleBooksId",
                unique: true,
                filter: "[GoogleBooksId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Listings_BookCatalogId",
                table: "Listings",
                column: "BookCatalogId");

            migrationBuilder.CreateIndex(
                name: "IX_Listings_ConditionId",
                table: "Listings",
                column: "ConditionId");

            migrationBuilder.CreateIndex(
                name: "IX_Listings_OwnerId",
                table: "Listings",
                column: "OwnerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookCatalogAuthors");

            migrationBuilder.DropTable(
                name: "BookCatalogGenres");

            migrationBuilder.DropTable(
                name: "Listings");

            migrationBuilder.DropTable(
                name: "Authors");

            migrationBuilder.DropTable(
                name: "BookCatalogs");
        }
    }
}
