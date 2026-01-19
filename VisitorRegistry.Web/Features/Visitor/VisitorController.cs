using Microsoft.AspNetCore.Mvc;
using QRCoder;
using System;
using System.IO;
using System.Threading.Tasks;
using VisitorRegistry.Services.Visitors;
using VisitorRegistry.Web.Features.Presence;

namespace VisitorRegistry.Web.Features.Visitor
{
    public partial class VisitorController : Controller
    {
        // Campo per il service
        private readonly VisitorService _visitorService;

        // Costruttore che inietta il service
        public VisitorController(VisitorService visitorService)
        {
            _visitorService = visitorService;
        }

        // =========================
        // Lista visitatori
        // =========================
        public virtual async Task<IActionResult> Index()
        {
            var visitors = await _visitorService.GetAll();
            return View("VisitorList", visitors);
        }

        // =========================
        // Dettagli visitatore
        // =========================
        [HttpGet]
        public virtual async Task<IActionResult> Details(int id)
        {
            // Recupera il visitatore
            var visitorDto = await _visitorService.GetById(id); // VisitorDetailDTO
            if (visitorDto == null)
                return NotFound();

            // Genera QR code Base64 dal codice salvato
            string qrBase64 = "";
            if (!string.IsNullOrEmpty(visitorDto.QrCode))
            {
                using var qrGenerator = new QRCodeGenerator();
                using var qrData = qrGenerator.CreateQrCode(visitorDto.QrCode, QRCodeGenerator.ECCLevel.Q);
                using var qrCode = new QRCode(qrData);
                using var ms = new MemoryStream();
                using var bitmap = qrCode.GetGraphic(20);
                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                qrBase64 = Convert.ToBase64String(ms.ToArray());
            }

            // Recupera ultima presenza (check-in/check-out)
            var presence = await _visitorService.GetLatestPresence(id);

            // Popola il ViewModel
            var viewModel = new PresenceDetailsViewModel
            {
                Id = visitorDto.Id,
                Nome = visitorDto.Nome,
                Cognome = visitorDto.Cognome,
                DataVisita = visitorDto.DataVisita,
                QrCode = visitorDto.QrCode,
                QrCodeImageBase64 = qrBase64,
                CheckInTime = presence?.CheckInTime,
                CheckOutTime = presence?.CheckOutTime,
                Ditta = visitorDto.Ditta ?? "-",
                Referente = visitorDto.Referente ?? "-"
            };

            return View(viewModel);
        }
    }
}


