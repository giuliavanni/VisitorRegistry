using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using VisitorRegistry.Services.Visitors;
using VisitorRegistry.Web.Features.Home;

namespace VisitorRegistry.Web.Features.Home
{
    public partial class HomeController : Controller
    {
        private readonly VisitorService _visitorService;

        public HomeController(VisitorService visitorService)
        {
            _visitorService = visitorService;
        }

        // =========================
        // Mostra il form per nuovo visitatore
        // =========================
        [HttpGet]
        public virtual IActionResult Index()
        {
            return View("Home"); // Cerca Home.cshtml
        }

        // =========================
        // Salva il visitatore nel DB
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual async Task<IActionResult> Index(VisitorViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Home", model);

            var dto = new VisitorCreateDTO
            {
                Nome = model.Nome,
                Cognome = model.Cognome,
                DataVisita = DateTime.Now,
                QrCode = Guid.NewGuid().ToString()
            };

            await _visitorService.Create(dto);

            return RedirectToAction("Success");
        }

        // =========================
        // Pagina di conferma dopo inserimento
        // =========================
        [HttpGet]
        public virtual IActionResult Success()
        {
            return View(); // Visualizza conferma o QR code
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
