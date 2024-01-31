using System;
using System.Threading;
using System.Threading.Tasks;
using LT.DigitalOffice.TelegramService.Business.Features.Bot.Resources;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace LT.DigitalOffice.TelegramService.Business.Features.Bot;

public class UpdateHandler
{
  private readonly ITelegramBotClient _botClient;
  private readonly ILogger<UpdateHandler> _logger;

  public UpdateHandler(ITelegramBotClient botClient, ILogger<UpdateHandler> logger)
  {
    _botClient = botClient;
    _logger = logger;
  }

  public async Task HandleUpdateAsync(Update update, CancellationToken ct)
  {
    try
    {
      var handler = update switch
      {
        { Message: { } message } => MessageReceivedAsync(message, ct),
        _ => UnknownUpdateReceivedAsync(update)
      };

      await handler;
    }
    catch(Exception exception)
    {
      HandleError(exception);
    }
  }

  private void HandleError(Exception exception)
  {
    var errorMessage = exception switch
    {
      ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
      _ => exception.ToString()
    };

    _logger.LogInformation("HandleError: {ErrorMessage}", errorMessage);
  }

  private async Task MessageReceivedAsync(Message message, CancellationToken ct)
  {
    _logger.LogInformation("Receive message type: {MessageType}", message.Type);
    if (message.Text is not { } messageText)
      return;

    var action = messageText.Split(' ')[0] switch
    {
      "/menu" => SendMenuAsync(_botClient, message, ct),
      _ => SendUsageAsync(_botClient, message, ct)
    };
    await action;

    _logger.LogInformation("The message was sent with id: {SentMessageId}", action.Result.MessageId);
  }

  private Task<Message> SendMenuAsync(ITelegramBotClient botClient, Message message, CancellationToken ct)
  {
    InlineKeyboardMarkup inlineKeyboard = new(
      new[]
      {
        new []
        {
          InlineKeyboardButton.WithCallbackData(BotResources.EventsCalendarOption, "1")
        },
        new []
        {
          InlineKeyboardButton.WithCallbackData(BotResources.BookingOption, "2"),
          InlineKeyboardButton.WithCallbackData(BotResources.SurveysOption, "3"),
        },
      });

    return botClient.SendTextMessageAsync(
      chatId: message.Chat.Id,
      text: BotResources.MenuMessage,
      replyMarkup: inlineKeyboard,
      cancellationToken: ct);
  }

  private Task<Message> SendUsageAsync(ITelegramBotClient botClient, Message message, CancellationToken ct)
  {
    return botClient.SendTextMessageAsync(
      chatId: message.Chat.Id,
      text: BotResources.UsageMessage,
      replyMarkup: new ReplyKeyboardRemove(),
      cancellationToken: ct);
  }

  private Task UnknownUpdateReceivedAsync(Update update)
  {
    _logger.LogInformation("Unknown update type: {UpdateType}", update.Type);
    return Task.CompletedTask;
  }
}
