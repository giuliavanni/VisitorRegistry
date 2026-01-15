using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using VisitorRegistry.Services;

namespace VisitorRegistry.Web.Features.Presence
{
    public partial class PresenceController : Controller
    {
        private readonly PresenceService _presenceService;

        public PresenceController(PresenceService presenceService)
        {
            _presenceService = presenceService;
        }

        [HttpGet]
        public virtual IActionResult Scan(string mode)
        {
            ViewBag.Mode = mode ?? "in"; // default a check-in
            ViewBag.Success = null;
            ViewBag.Message = null;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual async Task<IActionResult> Scan(string qrCode, string mode)
        {
            ViewBag.Mode = mode ?? "in";

            PresenceActionResult result;

            if (mode == "in")
            {
                result = await _presenceService.CheckInByQrAsync(qrCode);
            }
            else
            {
                result = await _presenceService.CheckOutByQrAsync(qrCode);
            }

            // Messaggi coerenti con il tentativo dell’utente
            switch (result)
            {
                case PresenceActionResult.AlreadyCheckedIn:
                    ViewBag.Message = "Non puoi fare Check-In: sei già dentro";
                    ViewBag.Success = false;
                    break;
                case PresenceActionResult.AlreadyCheckedOut:
                    ViewBag.Message = "Non puoi fare Check-Out: sei già fuori";
                    ViewBag.Success = false;
                    break;
                case PresenceActionResult.CheckedIn:
                    ViewBag.Message = "Check-In completato";
                    ViewBag.Success = true;
                    break;
                case PresenceActionResult.CheckedOut:
                    ViewBag.Message = "Check-Out completato";
                    ViewBag.Success = true;
                    break;
                default:
                    ViewBag.Message = "QR non valido";
                    ViewBag.Success = false;
                    break;
            }

            return View();
        }
    }
}
