using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TetaBackend.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Genders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Genders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Languages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Languages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Locations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Country = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FriendsCategoryInfos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Info = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FriendsCategoryInfos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LoveCategoryInfos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Info = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    MinAge = table.Column<int>(type: "int", nullable: false),
                    MaxAge = table.Column<int>(type: "int", nullable: false),
                    GenderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoveCategoryInfos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoveCategoryInfos_Genders_GenderId",
                        column: x => x.GenderId,
                        principalTable: "Genders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserInfoLanguages",
                columns: table => new
                {
                    UserInfoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LanguageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserInfoLanguages", x => new { x.UserInfoId, x.LanguageId });
                    table.ForeignKey(
                        name: "FK_UserInfoLanguages_Languages_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "Languages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserInfos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Age = table.Column<short>(type: "smallint", nullable: false),
                    About = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GenderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlaceOfBirthId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LocationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserInfos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserInfos_Genders_GenderId",
                        column: x => x.GenderId,
                        principalTable: "Genders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserInfos_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserInfos_Locations_PlaceOfBirthId",
                        column: x => x.PlaceOfBirthId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserInfoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LoveCategoryInfoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FriendsCategoryInfoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    WorkCategoryInfoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_FriendsCategoryInfos_FriendsCategoryInfoId",
                        column: x => x.FriendsCategoryInfoId,
                        principalTable: "FriendsCategoryInfos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Users_LoveCategoryInfos_LoveCategoryInfoId",
                        column: x => x.LoveCategoryInfoId,
                        principalTable: "LoveCategoryInfos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Users_UserInfos_UserInfoId",
                        column: x => x.UserInfoId,
                        principalTable: "UserInfos",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "WorkCategoryInfos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Info = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Income = table.Column<int>(type: "int", nullable: false),
                    LookingFor = table.Column<int>(type: "int", nullable: false),
                    Skills = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkCategoryInfos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkCategoryInfos_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FriendsCategoryInfos_UserId",
                table: "FriendsCategoryInfos",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LoveCategoryInfos_GenderId",
                table: "LoveCategoryInfos",
                column: "GenderId");

            migrationBuilder.CreateIndex(
                name: "IX_LoveCategoryInfos_UserId",
                table: "LoveCategoryInfos",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserInfoLanguages_LanguageId",
                table: "UserInfoLanguages",
                column: "LanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_UserInfos_GenderId",
                table: "UserInfos",
                column: "GenderId");

            migrationBuilder.CreateIndex(
                name: "IX_UserInfos_LocationId",
                table: "UserInfos",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_UserInfos_PlaceOfBirthId",
                table: "UserInfos",
                column: "PlaceOfBirthId");

            migrationBuilder.CreateIndex(
                name: "IX_UserInfos_UserId",
                table: "UserInfos",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_FriendsCategoryInfoId",
                table: "Users",
                column: "FriendsCategoryInfoId",
                unique: true,
                filter: "[FriendsCategoryInfoId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Users_LoveCategoryInfoId",
                table: "Users",
                column: "LoveCategoryInfoId",
                unique: true,
                filter: "[LoveCategoryInfoId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserInfoId",
                table: "Users",
                column: "UserInfoId",
                unique: true,
                filter: "[UserInfoId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Users_WorkCategoryInfoId",
                table: "Users",
                column: "WorkCategoryInfoId",
                unique: true,
                filter: "[WorkCategoryInfoId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_WorkCategoryInfos_UserId",
                table: "WorkCategoryInfos",
                column: "UserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_FriendsCategoryInfos_Users_UserId",
                table: "FriendsCategoryInfos",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LoveCategoryInfos_Users_UserId",
                table: "LoveCategoryInfos",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserInfoLanguages_UserInfos_UserInfoId",
                table: "UserInfoLanguages",
                column: "UserInfoId",
                principalTable: "UserInfos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserInfos_Users_UserId",
                table: "UserInfos",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_WorkCategoryInfos_WorkCategoryInfoId",
                table: "Users",
                column: "WorkCategoryInfoId",
                principalTable: "WorkCategoryInfos",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FriendsCategoryInfos_Users_UserId",
                table: "FriendsCategoryInfos");

            migrationBuilder.DropForeignKey(
                name: "FK_LoveCategoryInfos_Users_UserId",
                table: "LoveCategoryInfos");

            migrationBuilder.DropForeignKey(
                name: "FK_UserInfos_Users_UserId",
                table: "UserInfos");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkCategoryInfos_Users_UserId",
                table: "WorkCategoryInfos");

            migrationBuilder.DropTable(
                name: "UserInfoLanguages");

            migrationBuilder.DropTable(
                name: "Languages");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "FriendsCategoryInfos");

            migrationBuilder.DropTable(
                name: "LoveCategoryInfos");

            migrationBuilder.DropTable(
                name: "UserInfos");

            migrationBuilder.DropTable(
                name: "WorkCategoryInfos");

            migrationBuilder.DropTable(
                name: "Genders");

            migrationBuilder.DropTable(
                name: "Locations");
        }
    }
}
