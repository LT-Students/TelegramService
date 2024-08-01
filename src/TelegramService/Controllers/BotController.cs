using System.Threading;
using System.Threading.Tasks;
using LT.DigitalOffice.TelegramService.Business.Features.Bot;
using LT.DigitalOffice.TelegramService.Business.Shared.Filters.Bot;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;

namespace LT.DigitalOffice.TelegramService.Controllers;

[ApiController]
[Route("[controller]")]
public class BotController : ControllerBase
{
  [HttpPost]
  [ValidateTelegramBot]
  public async Task<IActionResult> Post(
    [FromBody] Update update,
    [FromServices] UpdateHandler handleUpdateService,
    CancellationToken cancellationToken)
  {
    await handleUpdateService.HandleUpdateAsync(update, cancellationToken);
    return Ok();
  }
}
