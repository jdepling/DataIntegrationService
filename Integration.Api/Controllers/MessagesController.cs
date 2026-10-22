using Integration.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Integration.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly IMessageService _messageService;

        public MessagesController(IMessageService messageService)
        {
            _messageService = messageService ?? throw new ArgumentNullException(nameof(messageService));
        }

        /// <summary>
        ///     Creates a new integration message.
        /// </summary>
        /// <param name="request">The integration message request.</param>
        /// <returns>An IActionResult indicating the result of the operation.</returns>
        [HttpPost]
        [Authorize(Policy = "SubmitMessages")]
        public async Task<IActionResult> Post(IntegrationMessageRequest request)
        {
            var id = await _messageService.CreateMessageAsync(request);

            return Accepted(new { id });
        }
    }
}
