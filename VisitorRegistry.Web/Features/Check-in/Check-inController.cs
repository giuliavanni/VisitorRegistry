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
            var result = await _presenceService.ToggleByQrAsync(qrCode);

            if (result == PresenceActionResult.CheckedIn)
                return Ok(new { message = "Check-in completato" });
            else if (result == PresenceActionResult.CheckedOut)
                return Ok(new { message = "Check-out completato" });
            else
                return BadRequest(new { message = "QR non valido" });
        }

        [HttpPost]
        public virtual async Task<IActionResult> CheckOut(string qrCode)
        {
            var result = await _presenceService.ToggleByQrAsync(qrCode);

            if (result == PresenceActionResult.CheckedOut)
                return Ok(new { message = "Check-out completato" });
            else if (result == PresenceActionResult.CheckedIn)
                return Ok(new { message = "Check-in completato" });
            else
                return BadRequest(new { message = "QR non valido" });
        }
    }
}

