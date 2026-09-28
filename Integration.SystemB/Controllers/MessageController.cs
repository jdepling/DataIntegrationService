using Integration.SystemB.Models;
using Integration.SystemB.Services;
using Microsoft.AspNetCore.Mvc;

namespace Integration.SystemB.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MessagesController : ControllerBase
    {
        private readonly IMessageService _messageService;

        public MessagesController(IMessageService messageService)
        {
            _messageService = messageService;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateMessageRequest request, CancellationToken cancellationToken)
        {
            var response = await _messageService.CreateAsync(request, cancellationToken);

            return StatusCode(StatusCodes.Status201Created, response);
        }
    }
}