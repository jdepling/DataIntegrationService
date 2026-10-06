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

        /// <summary>
        ///    Replays a failed integration message by its ID.
        /// </summary>
        /// <param name="id">The ID of the failed integration message.</param>
        /// <returns>An IActionResult indicating the result of the operation.</returns>
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

        /// <summary>
        ///   Retrieves a list of failed integration messages.
        /// </summary>
        /// <returns>An IActionResult indicating the result of the operation.</returns>
        [HttpGet]
        public async Task<IActionResult> GetFailedMessages()
        {
            var messages = await _replayMessageService.GetFailedMessagesAsync();

            return Ok(messages);
        }
    }
}
