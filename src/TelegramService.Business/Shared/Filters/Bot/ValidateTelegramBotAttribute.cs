using System;
using Microsoft.AspNetCore.Mvc;

namespace LT.DigitalOffice.TelegramService.Business.Shared.Filters.Bot;

[AttributeUsage(AttributeTargets.Method)]
public class ValidateTelegramBotAttribute : TypeFilterAttribute
{
  public ValidateTelegramBotAttribute() : base(typeof(ValidateTelegramBotFilter))
  {
  }
}
