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

        // Pagina di scan QR per check-out
        [HttpGet]
        public virtual IActionResult Scan()
        {
            return View();
        }

        // Submit QR per check-out
        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual async Task<IActionResult> Scan(string qrCode)
        {
            var result = await _presenceService.ToggleByQrAsync(qrCode);

            switch (result)
            {
                case PresenceActionResult.CheckedIn:
                    // Non dovrebbe mai succedere, perché check-in automatico
                    ViewBag.Message = "Check-IN registrato automaticamente alla creazione";
                    ViewBag.Success = true;
                    break;

                case PresenceActionResult.CheckedOut:
                    ViewBag.Message = "Check-OUT completato";
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

