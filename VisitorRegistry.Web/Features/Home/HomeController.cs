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

        [HttpGet]
        public virtual IActionResult Start()
        {
            return View();
        }

        [HttpGet]
        public virtual IActionResult InsertData()
        {
            return View("Register");
        }


        // Mostra form registrazione visitatore
        [HttpGet]
        public virtual IActionResult Register()
        {
            return View("Home");
        }

        // Salva visitatore e genera QR code + check-in automatico
        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual async Task<IActionResult> Register(VisitorViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Home", model);

            var qrContent = Guid.NewGuid().ToString();

            var dto = new VisitorCreateDTO
            {
                Nome = model.Nome,
                Cognome = model.Cognome,
                Ditta = model.Ditta,
                Referente = model.Referente,
                DataVisita = DateTime.Now,
                QrCode = qrContent
            };

            // Inserisce visitatore nel DB
            var visitorId = await _visitorService.Create(dto);

            // Check-in automatico
            await _presenceService.ToggleByVisitorIdAsync(visitorId);

            return RedirectToAction("SuccessRegistration", new { qr = qrContent, nome = model.Nome });
        }

        // Mostra QR code della registrazione
        [HttpGet]
        public virtual IActionResult SuccessRegistration(string qr, string nome)
        {

            ViewBag.Nome = nome;

            if (string.IsNullOrWhiteSpace(qr))
            {
                qr = "TEST-QR-123";
            }

            var qrGenerator = new QRCodeGenerator();
            var qrData = qrGenerator.CreateQrCode(qr, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new QRCode(qrData);

            using var ms = new MemoryStream();
            using var bitmap = qrCode.GetGraphic(20);
            bitmap.Save(ms, ImageFormat.Png);

            ViewBag.QrCodeImage = Convert.ToBase64String(ms.ToArray());
            ViewBag.QrContent = qr;

            return View();
        }



        [HttpPost]
        public virtual async Task<IActionResult> ScanBadge(string qrCode)
        {
            var visitor = await _visitorService.GetByQrCodeAsync(qrCode);

            if (visitor == null)
                return View("BadgeNotValid");

            var lastPresence = await _visitorService.GetLatestPresence(visitor.Id);

            if (lastPresence != null && lastPresence.CheckOutTime == null)
            {
                // VISITA APERTA ? USCITA
                return View("ConfirmCheckout", visitor);
            }

            // NESSUNA VISITA APERTA ? ENTRATA
            return View("ConfirmCheckin", visitor);
        }


        // Cambia lingua
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