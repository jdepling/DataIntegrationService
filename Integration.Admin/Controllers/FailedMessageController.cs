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

        /// <summary>
        ///     Displays the details of a failed message by its ID.
        /// </summary>
        /// <param name="id">The ID of the failed message to display.</param>
        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            var message = await _adminApiClient.GetFailedMessageAsync(id);

            if (message == null)
            {
                return NotFound();
            }

            return View(message);
        }

        /// <summary>
        ///    Displays a list of all failed messages.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var messages = await _adminApiClient.GetFailedMessagesAsync();

            return View(messages);
        }

        /// <summary>
        ///   Replays a failed message by its ID. If the replay is successful, it redirects to the index page; otherwise, it returns a NotFound result.
        /// </summary>
        /// <param name="id">The ID of the failed message to replay.</param>
        [HttpPost]
        public async Task<IActionResult> Replay(Guid id)
        {
            var success = await _adminApiClient.ReplayMessageAsync(id);

            if (!success)
                return NotFound();

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        ///  Replays a failed message by its ID with a fixed payload. If the replay is successful, it redirects to the index page; otherwise, it returns a NotFound result.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="payload"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ReplayFixed(Guid id, string payload)
        {
            var success = await _adminApiClient.ReplayFixedMessageAsync(id, payload);

            if (!success)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}