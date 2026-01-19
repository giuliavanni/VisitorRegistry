using Microsoft.AspNetCore.Mvc;
using QRCoder;
using System;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using VisitorRegistry.Services.Visitors;
using VisitorRegistry.Web.Features.Presence;

namespace VisitorRegistry.Web.Features.Presence
{
    public partial class PresenceController : Controller
    {
        private readonly VisitorService _visitorService;

        // Inietta VisitorService tramite DI
        public PresenceController(VisitorService visitorService)
        {
            _visitorService = visitorService;
        }

        // Dettagli visita
        [HttpGet]
        public virtual async Task<IActionResult> Details(int id)
        {
            // Prende i dati del visitatore
            var visitorDto = await _visitorService.GetById(id);
            if (visitorDto == null)
                return NotFound();

            // Prende l'ultima presenza
            var presence = await _visitorService.GetLatestPresence(id);

            // Genera QR code in Base64
            string qrBase64 = "";
            if (!string.IsNullOrEmpty(visitorDto.QrCode))
            {
                using var qrGenerator = new QRCodeGenerator();
                using var qrData = qrGenerator.CreateQrCode(visitorDto.QrCode, QRCodeGenerator.ECCLevel.Q);
                using var qrCode = new QRCode(qrData);
                using var ms = new MemoryStream();
                using var bitmap = qrCode.GetGraphic(20);
                bitmap.Save(ms, ImageFormat.Png);
                qrBase64 = Convert.ToBase64String(ms.ToArray());
            }

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
        
        [HttpGet]
        public virtual async Task<IActionResult> DetailsJson(int presenceId)
        {
            // Recupera la presenza CORRETTA
            var presence = await _visitorService.GetPresenceById(presenceId);
            if (presence == null)
                return NotFound();

            // Recupera il visitatore
            var visitorDto = await _visitorService.GetById(presence.VisitorId);
            if (visitorDto == null)
                return NotFound();

            // Genera QR Base64
            string qrBase64 = null;
            if (!string.IsNullOrEmpty(visitorDto.QrCode))
            {
                using var qrGenerator = new QRCodeGenerator();
                using var qrData = qrGenerator.CreateQrCode(visitorDto.QrCode, QRCodeGenerator.ECCLevel.Q);
                using var qrCode = new QRCode(qrData);
                using var ms = new MemoryStream();
                using var bitmap = qrCode.GetGraphic(20);
                bitmap.Save(ms, ImageFormat.Png);
                qrBase64 = Convert.ToBase64String(ms.ToArray());
            }

            return Json(new
            {
                nome = visitorDto.Nome,
                cognome = visitorDto.Cognome,
                ditta = visitorDto.Ditta,
                referente = visitorDto.Referente,
                dataVisita = visitorDto.DataVisita,
                checkInTime = presence.CheckInTime,
                checkOutTime = presence.CheckOutTime,
                qrCode = visitorDto.QrCode,
                qrCodeImageBase64 = qrBase64
            });
        }

    }

}


