using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hao_Hao.Migrations
{
    /// <inheritdoc />
    public partial class FixChiTietMuaHang : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChiTietMuaHang",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdUser = table.Column<int>(type: "int", nullable: false),
                    IdProduct = table.Column<int>(type: "int", nullable: false),
                    TimeBuy = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SoTien = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTietMuaHang", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChiTietMuaHang_SanPham_IdProduct",
                        column: x => x.IdProduct,
                        principalTable: "SanPham",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChiTietMuaHang_Users_IdUser",
                        column: x => x.IdUser,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietMuaHang_IdProduct",
                table: "ChiTietMuaHang",
                column: "IdProduct");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietMuaHang_IdUser",
                table: "ChiTietMuaHang",
                column: "IdUser");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChiTietMuaHang");
        }
    }
}
