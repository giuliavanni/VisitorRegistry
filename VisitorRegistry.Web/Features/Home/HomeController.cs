using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using QRCoder;
using System;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using VisitorRegistry.Services;
using VisitorRegistry.Services.Visitors;
using VisitorRegistry.Web.Features.Home;

namespace VisitorRegistry.Web.Features.Home
{
    public partial class HomeController : Controller
    {
        private readonly VisitorService _visitorService;
        private readonly PresenceService _presenceService;

        public HomeController(VisitorService visitorService, PresenceService presenceService)
        {
            _visitorService = visitorService;
            _presenceService = presenceService;
        }

        // =========================
        // Mostra form registrazione visitatore
        // =========================
        [HttpGet]
        public virtual IActionResult Index()
        {
            return View("Home");
        }

        // =========================
        // Salva visitatore e genera QR code + check-in automatico
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual async Task<IActionResult> Index(VisitorViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Home", model);

            var qrContent = Guid.NewGuid().ToString();

            var dto = new VisitorCreateDTO
            {
                Nome = model.Nome,
                Cognome = model.Cognome,
                DataVisita = DateTime.Now,
                QrCode = qrContent
            };

            // Inserisce visitatore nel DB
            var visitorId = await _visitorService.Create(dto);

            // Check-in automatico
            await _presenceService.CheckInAsync(visitorId);

            return RedirectToAction("Success", new { qr = qrContent });
        }

        // =========================
        // Mostra QR code della registrazione
        // =========================
        [HttpGet]
        public virtual IActionResult Success(string qr)
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrData = qrGenerator.CreateQrCode(qr, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new QRCode(qrData);
            using var ms = new MemoryStream();
            using var bitmap = qrCode.GetGraphic(20);

            bitmap.Save(ms, ImageFormat.Png);
            var qrImage = Convert.ToBase64String(ms.ToArray());

            ViewBag.QrCodeImage = qrImage;
            ViewBag.QrContent = qr;

            return View();
        }

        // =========================
        // Scan QR code per check-in / check-out
        // =========================
        [HttpPost]
        public virtual async Task<IActionResult> ScanQr(string qr)
        {
            var found = await _visitorService.GetByQrCode(qr);
            if (found == null)
                return NotFound("QR non valido");

            var current = await _presenceService.GetVisitorsInsideAsync();
            bool isInside = current.Exists(p => p.VisitorId == found.Id);

            if (isInside)
            {
                await _presenceService.CheckOutAsync(found.Id);
                return Ok(new { message = "Check-out completato" });
            }
            else
            {
                await _presenceService.CheckInAsync(found.Id);
                return Ok(new { message = "Check-in completato" });
            }
        }

        // =========================
        // Cambia lingua
        // =========================
        [HttpPost]
        public virtual IActionResult ChangeLanguageTo(string cultureName)
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(cultureName)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1), Secure = true }
            );

            return Redirect(Request.GetTypedHeaders().Referer.ToString());
        }
    }
}
