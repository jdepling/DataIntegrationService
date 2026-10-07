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
        ///   Retrieves a list of failed integration messages.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetFailedMessages()
        {
            var messages = await _replayMessageService.GetFailedMessagesAsync();

            return Ok(messages);
        }

        /// <summary>
        ///  Retrieves a specific failed integration message by its ID.
        /// </summary>
        /// <param name="id">The ID of the failed integration message to retrieve.</param>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetFailedMessage(Guid id)
        {
            var message = await _replayMessageService.GetFailedMessageAsync(id);

            if (message == null)
                return NotFound();

            return Ok(message);
        }

        /// <summary>
        ///    Replays a failed integration message by its ID.
        /// </summary>
        /// <param name="id">The ID of the failed integration message.</param>
        [HttpPost("{id}/replay")]
        public async Task<IActionResult> Replay(Guid id)
        {
            var outboxId = await _replayMessageService.ReplayAsync(id);

            if (outboxId == null)
                return NotFound();

            return Accepted(new { id = outboxId });
        }

        /// <summary>
        ///   Replays a failed integration message by its ID with a fixed payload.
        /// </summary>
        /// <param name="id">The ID of the failed integration message.</param>
        /// <param name="payload">The fixed payload for the message.</param>
        /// <returns>The ID of the replayed message, or null if not found.</returns>
        [HttpPost("{id}/replay-fixed")]
        public async Task<IActionResult> ReplayFixed(Guid id, [FromBody] string payload)
        {
            var outboxId = await _replayMessageService.ReplayWithPayloadAsync(id, payload);

            if (outboxId == null)
                return NotFound();

            return Accepted(new { id = outboxId });
        }
    }
}
