using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using VisitorRegistry.Services;
using PresenceModel = VisitorRegistry.Services.Shared.Presence; // alias per la classe Presence

namespace VisitorRegistry.Web.Features.Presence
{
    public partial class PresenceController : Controller
    {
        private readonly PresenceService _presenceService;

        public PresenceController(PresenceService presenceService)
        {
            _presenceService = presenceService;
        }

        // Pagina di scan QR per check-in / check-out
        [HttpGet]
        public virtual IActionResult Scan(string mode, string qrCode)
        {
            ViewBag.Mode = mode ?? "in"; // default a check-in
            ViewBag.QRCode = qrCode;
            ViewBag.Success = null;
            ViewBag.Message = null;
            return View();
        }

        // Dettagli di una singola visita
        [HttpGet]
        [HttpGet]
        public virtual async Task<IActionResult> Details(int id)
        {
            var presence = await _presenceService.GetByIdAsync(id);
            if (presence == null)
                return NotFound();

            var model = new PresenceDetailsViewModel
            {
                Nome = presence.Visitor.Nome,
                Cognome = presence.Visitor.Cognome,
                Ditta = presence.Visitor.Ditta ?? "—",
                Referente = presence.Visitor.Referente ?? "—",
                CheckInTime = presence.CheckInTime,
                CheckOutTime = presence.CheckOutTime,
                DataVisita = presence.CheckInTime != default(DateTime) ? presence.CheckInTime.Date : DateTime.Today,
                QrCode = presence.Visitor.QrCode
            };

            return Json(model);
        }

        
        // Submit QR per check-in / check-out
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

