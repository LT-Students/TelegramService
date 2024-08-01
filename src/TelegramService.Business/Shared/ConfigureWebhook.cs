using System;
using System.Threading;
using System.Threading.Tasks;
using LT.DigitalOffice.TelegramService.Broker.Configurations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace LT.DigitalOffice.TelegramService.Business.Shared;

public class ConfigureWebhook : IHostedService
{
  private readonly ILogger<ConfigureWebhook> _logger;
  private readonly IServiceProvider _serviceProvider;
  private readonly TelegramBotConfig _telegramBotConfig;

  public ConfigureWebhook(
    ILogger<ConfigureWebhook> logger,
    IServiceProvider serviceProvider,
    IOptions<TelegramBotConfig> botOptions)
  {
    _logger = logger;
    _serviceProvider = serviceProvider;
    _telegramBotConfig = botOptions.Value;
  }

  public async Task StartAsync(CancellationToken ct)
  {
    using var scope = _serviceProvider.CreateScope();
    var botClient = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();

    var webhookAddress = $"{_telegramBotConfig.BotHostAddress}{_telegramBotConfig.BotRoute}";
    _logger.LogInformation("Setting webhook: {webhookAddress}", webhookAddress);
    await botClient.SetWebhookAsync(
      url: webhookAddress,
      allowedUpdates: Array.Empty<UpdateType>(),
      secretToken: _telegramBotConfig.BotSecretToken,
      cancellationToken: ct);
  }

  public async Task StopAsync(CancellationToken ct)
  {
    using var scope = _serviceProvider.CreateScope();
    var botClient = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();

    _logger.LogInformation("Removing webhook");
    await botClient.DeleteWebhookAsync(cancellationToken: ct);
  }
}
