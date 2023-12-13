using System.Reflection;
using LT.DigitalOffice.TelegramService.DataLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace LT.DigitalOffice.TelegramService.DataLayer;

public class TelegramServiceDbContext : DbContext
{
  public DbSet<DbTelegramContact> TelegramContacts { get; set; }

  public TelegramServiceDbContext(DbContextOptions<TelegramServiceDbContext> options) : base(options)
  {
  }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.ApplyConfigurationsFromAssembly(Assembly.Load("LT.DigitalOffice.TelegramService.DataLayer"));
  }
}
