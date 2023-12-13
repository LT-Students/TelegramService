namespace LT.DigitalOffice.TelegramService.Broker.Configurations;

public record MemoryCacheConfig
{
  public const string SectionName = "MemoryCache";

  public double CacheLiveInMinutes { get; set; }
}
