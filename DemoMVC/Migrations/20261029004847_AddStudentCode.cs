
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DemoMVC.Migrations
{
    public partial class AddStudentCode : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Bước 1: Thêm cột tạm thời cho phép NULL
            migrationBuilder.AddColumn<string>(
                name: "StudentCode",
                table: "Students",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            // Bước 2: Gán mã riêng cho các sinh viên hiện có
            // Ví dụ: 2821050001, 2821050002, ...
            migrationBuilder.Sql(@"
                ;WITH NumberedStudents AS
                (
                    SELECT
                        Id,
                        ROW_NUMBER() OVER (ORDER BY Id) AS RowNum
                    FROM Students
                )
                UPDATE s
                SET StudentCode =
                    '282105' +
                    RIGHT('0000' + CAST(n.RowNum AS VARCHAR(10)), 4)
                FROM Students AS s
                INNER JOIN NumberedStudents AS n
                    ON s.Id = n.Id;
            ");

            // Bước 3: Chuyển cột thành NOT NULL
            migrationBuilder.AlterColumn<string>(
                name: "StudentCode",
                table: "Students",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10,
                oldNullable: true);

            // Bước 4: Tạo chỉ mục duy nhất
            migrationBuilder.CreateIndex(
                name: "IX_Students_StudentCode",
                table: "Students",
                column: "StudentCode",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Students_StudentCode",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "StudentCode",
                table: "Students");
        }
    }
}
