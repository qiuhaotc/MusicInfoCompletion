using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MusicInfoCompletion.Data.Migrations
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Albums",
                columns: table => new
                {
                    Pk = table.Column<Guid>(nullable: false),
                    AddedBy = table.Column<string>(nullable: true),
                    AddDate = table.Column<DateTime>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    LastModifyBy = table.Column<string>(nullable: true),
                    LastModifyDate = table.Column<DateTime>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn),
                    Title = table.Column<string>(maxLength: 200, nullable: true),
                    ReleaseDate = table.Column<DateTime>(nullable: true),
                    Description = table.Column<string>(maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Albums", x => x.Pk);
                });

            migrationBuilder.CreateTable(
                name: "Genre",
                columns: table => new
                {
                    Pk = table.Column<Guid>(nullable: false),
                    AddedBy = table.Column<string>(nullable: true),
                    AddDate = table.Column<DateTime>(nullable: false),
                    LastModifyBy = table.Column<string>(nullable: true),
                    LastModifyDate = table.Column<DateTime>(nullable: false),
                    Title = table.Column<string>(maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Genre", x => x.Pk);
                });

            migrationBuilder.CreateTable(
                name: "Singers",
                columns: table => new
                {
                    Pk = table.Column<Guid>(nullable: false),
                    AddedBy = table.Column<string>(nullable: true),
                    AddDate = table.Column<DateTime>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    LastModifyBy = table.Column<string>(nullable: true),
                    LastModifyDate = table.Column<DateTime>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn),
                    Name = table.Column<string>(maxLength: 100, nullable: false),
                    AKANames = table.Column<string>(maxLength: 300, nullable: true),
                    Description = table.Column<string>(maxLength: 10000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Singers", x => x.Pk);
                });

            migrationBuilder.CreateTable(
                name: "Songs",
                columns: table => new
                {
                    Pk = table.Column<Guid>(nullable: false),
                    AddedBy = table.Column<string>(nullable: true),
                    AddDate = table.Column<DateTime>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    LastModifyBy = table.Column<string>(nullable: true),
                    LastModifyDate = table.Column<DateTime>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn),
                    Title = table.Column<string>(maxLength: 200, nullable: false),
                    AKATitles = table.Column<string>(maxLength: 300, nullable: true),
                    AlbumPk = table.Column<Guid>(nullable: false),
                    Picture = table.Column<byte[]>(nullable: true),
                    Seconds = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Songs", x => x.Pk);
                    table.ForeignKey(
                        name: "FK_Songs_Albums_AlbumPk",
                        column: x => x.AlbumPk,
                        principalTable: "Albums",
                        principalColumn: "Pk",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GenreSong",
                columns: table => new
                {
                    Pk = table.Column<Guid>(nullable: false),
                    AddedBy = table.Column<string>(nullable: true),
                    AddDate = table.Column<DateTime>(nullable: false),
                    LastModifyBy = table.Column<string>(nullable: true),
                    LastModifyDate = table.Column<DateTime>(nullable: false),
                    GenrePk = table.Column<Guid>(nullable: false),
                    SongPk = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GenreSong", x => x.Pk);
                    table.ForeignKey(
                        name: "FK_GenreSong_Genre_GenrePk",
                        column: x => x.GenrePk,
                        principalTable: "Genre",
                        principalColumn: "Pk",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GenreSong_Songs_SongPk",
                        column: x => x.SongPk,
                        principalTable: "Songs",
                        principalColumn: "Pk",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SingerSongs",
                columns: table => new
                {
                    Pk = table.Column<Guid>(nullable: false),
                    AddedBy = table.Column<string>(nullable: true),
                    AddDate = table.Column<DateTime>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    LastModifyBy = table.Column<string>(nullable: true),
                    LastModifyDate = table.Column<DateTime>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn),
                    SingerPk = table.Column<Guid>(nullable: false),
                    SongPk = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SingerSongs", x => x.Pk);
                    table.ForeignKey(
                        name: "FK_SingerSongs_Singers_SingerPk",
                        column: x => x.SingerPk,
                        principalTable: "Singers",
                        principalColumn: "Pk",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SingerSongs_Songs_SongPk",
                        column: x => x.SongPk,
                        principalTable: "Songs",
                        principalColumn: "Pk",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Albums_Title",
                table: "Albums",
                column: "Title");

            migrationBuilder.CreateIndex(
                name: "IX_Genre_Title",
                table: "Genre",
                column: "Title",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GenreSong_SongPk",
                table: "GenreSong",
                column: "SongPk");

            migrationBuilder.CreateIndex(
                name: "IX_GenreSong_GenrePk_SongPk",
                table: "GenreSong",
                columns: new[] { "GenrePk", "SongPk" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Singers_Name_AKANames",
                table: "Singers",
                columns: new[] { "Name", "AKANames" });

            migrationBuilder.CreateIndex(
                name: "IX_SingerSongs_SongPk",
                table: "SingerSongs",
                column: "SongPk");

            migrationBuilder.CreateIndex(
                name: "IX_SingerSongs_SingerPk_SongPk",
                table: "SingerSongs",
                columns: new[] { "SingerPk", "SongPk" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Songs_AlbumPk",
                table: "Songs",
                column: "AlbumPk");

            migrationBuilder.CreateIndex(
                name: "IX_Songs_Title_AKATitles",
                table: "Songs",
                columns: new[] { "Title", "AKATitles" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GenreSong");

            migrationBuilder.DropTable(
                name: "SingerSongs");

            migrationBuilder.DropTable(
                name: "Genre");

            migrationBuilder.DropTable(
                name: "Singers");

            migrationBuilder.DropTable(
                name: "Songs");

            migrationBuilder.DropTable(
                name: "Albums");
        }
    }
}
