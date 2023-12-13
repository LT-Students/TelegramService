using System;
using LT.DigitalOffice.TelegramService.DataLayer.Models;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace LT.DigitalOffice.TelegramService.DataLayer.Migrations;

[DbContext(typeof(TelegramServiceDbContext))]
[Migration("20231208170000_InitialMigration")]
public class CreateTelegramContacts : Migration
{
  private void CreateTelegramContactsTable(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.CreateTable(
      name: DbTelegramContact.TableName,
      columns: table => new
      {
        UserId = table.Column<Guid>(nullable: false),
        TelegramUserId = table.Column<long>(nullable: false),
        TelegramUserName = table.Column<string>(nullable: false),
        IsActive = table.Column<bool>(nullable: false),
        CreatedBy = table.Column<Guid>(nullable: false),
        CreatedAtUtc = table.Column<DateTime>(nullable: false),
        ModifiedBy = table.Column<Guid>(nullable: true),
        ModifiedAtUtc = table.Column<DateTime>(nullable: true)
      },
      constraints: table =>
      {
        table.PrimaryKey($"PK_{DbTelegramContact.TableName}", c => c.UserId);
      });
  }

  protected override void Up(MigrationBuilder migrationBuilder)
  {
    CreateTelegramContactsTable(migrationBuilder);
  }

  protected override void Down(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.DropTable(DbTelegramContact.TableName);
  }
}
