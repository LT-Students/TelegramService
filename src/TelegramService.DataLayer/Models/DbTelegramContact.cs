using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LT.DigitalOffice.TelegramService.DataLayer.Models;

public class DbTelegramContact
{
  public const string TableName = "TelegramContacts";

  public Guid UserId { get; set; }
  public long TelegramUserId { get; set; }
  public string TelegramUserName { get; set; }
  public bool IsActive { get; set; }
  public Guid CreatedBy { get; set; }
  public DateTime CreatedAtUtc { get; set; }
  public Guid? ModifiedBy { get; set; }
  public DateTime? ModifiedAtUtc { get; set; }
}

public class DbTelegramContactConfiguration : IEntityTypeConfiguration<DbTelegramContact>
{
  public void Configure(EntityTypeBuilder<DbTelegramContact> builder)
  {
    builder.ToTable(DbTelegramContact.TableName);

    builder.HasKey(c => c.UserId);
  }
}
