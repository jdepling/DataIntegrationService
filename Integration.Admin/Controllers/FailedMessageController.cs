using Integration.Admin.Services;
using Microsoft.AspNetCore.Mvc;

namespace Integration.Admin.Controllers
{
    public class FailedMessagesController : Controller
    {
        private readonly IAdminApiClient _adminApiClient;

        public FailedMessagesController(IAdminApiClient adminApiClient)
        {
            _adminApiClient = adminApiClient ?? throw new ArgumentNullException(nameof(adminApiClient));
        }

        public async Task<IActionResult> Index()
        {
            var messages = await _adminApiClient.GetFailedMessagesAsync();

            return View(messages);
        }

        [HttpPost]
        public async Task<IActionResult> Replay(Guid id)
        {
            var success = await _adminApiClient.ReplayMessageAsync(id);

            if (!success)
                return NotFound();

            return RedirectToAction(nameof(Index));
        }
    }
}