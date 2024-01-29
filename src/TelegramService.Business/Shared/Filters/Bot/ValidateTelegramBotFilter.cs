using System;
using System.Net;
using LT.DigitalOffice.TelegramService.Broker.Configurations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace LT.DigitalOffice.TelegramService.Business.Shared.Filters.Bot;

public class ValidateTelegramBotFilter : IActionFilter
{
  private readonly string _botSecretToken;

  public ValidateTelegramBotFilter(IOptions<TelegramBotConfig> options)
  {
    _botSecretToken = options.Value.BotSecretToken;
  }

  public void OnActionExecuted(ActionExecutedContext context)
  {
  }

  public void OnActionExecuting(ActionExecutingContext context)
  {
    if (!IsValidToken(context.HttpContext.Request))
    {
      context.Result = new ObjectResult("\"X-Telegram-Bot-Api-Secret-Token\" is invalid")
      {
        StatusCode = (int)HttpStatusCode.Forbidden
      };
    }
  }

  private bool IsValidToken(HttpRequest request)
  {
    var isSecretTokenProvided = request.Headers.TryGetValue("X-Telegram-Bot-Api-Secret-Token", out var secretTokenHeader);
    return isSecretTokenProvided && string.Equals(secretTokenHeader, _botSecretToken, StringComparison.Ordinal);
  }
}
