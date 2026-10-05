using Integration.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Integration.Api.Controllers
{
    [Route("api/admin/messages")]
    [ApiController]
    public class AdminMessagesController : ControllerBase
    {
        private readonly IReplayMessageService _replayMessageService;

        public AdminMessagesController(IReplayMessageService replayMessageService)
        {
            _replayMessageService = replayMessageService ?? throw new ArgumentNullException(nameof(replayMessageService));
        }

        [HttpPost("{id}/replay")]
        public async Task<IActionResult> Replay(Guid id)
        {
            var outboxId = await _replayMessageService.ReplayAsync(id);

            if (outboxId == null)
            {
                return NotFound();
            }

            return Accepted(new { id = outboxId });
        }
    }
}
