using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using VisitorRegistry.Services;

namespace VisitorRegistry.Web.Features.CheckIn
{
    public partial class CheckInController : Controller
    {
        private readonly PresenceService _presenceService;

        public CheckInController(PresenceService presenceService)
        {
            _presenceService = presenceService;
        }

        [HttpPost]
        public virtual async Task<IActionResult> CheckIn(string qrCode)
        {
            var success = await _presenceService.CheckIn(qrCode);
            return success ? Ok() : BadRequest();
        }

        [HttpPost]
        public virtual async Task<IActionResult> CheckOut(string qrCode)
        {
            var success = await _presenceService.CheckOut(qrCode);
            return success ? Ok() : BadRequest();
        }
    }
}
