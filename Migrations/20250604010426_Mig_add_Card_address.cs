using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace KitapProject.Migrations
{
    /// <inheritdoc />
    public partial class Mig_add_Card_address : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserPaymentInfos",
                columns: table => new
                {
                    PaymentInfoId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AppUserId = table.Column<string>(type: "text", nullable: false),
                    CardHolderName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CardNumberLastFour = table.Column<string>(type: "character varying(19)", maxLength: 19, nullable: false),
                    ExpirationDate = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    CVV = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPaymentInfos", x => x.PaymentInfoId);
                    table.ForeignKey(
                        name: "FK_UserPaymentInfos_AspNetUsers_AppUserId",
                        column: x => x.AppUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserPaymentInfos_AppUserId",
                table: "UserPaymentInfos",
                column: "AppUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserPaymentInfos");
        }
    }
}
