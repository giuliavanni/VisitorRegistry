using Microsoft.AspNetCore.Mvc;
using QRCoder;
using System;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using VisitorRegistry.Services.Visitors;

namespace VisitorRegistry.Web.Features.Presence
{
    public partial class PresenceController : Controller
    {
        private readonly VisitorService _visitorService;

        public PresenceController(VisitorService visitorService)
        {
            _visitorService = visitorService;
        }

        
        [HttpGet]
        public virtual IActionResult Scan(string mode = "in")
        {
            ViewBag.Mode = mode;
            return View();
        }

        [HttpPost]
        public virtual IActionResult ProcessScan(string qrCode, string mode)
        {
            ViewBag.Mode = mode;
            ViewBag.Success = true;
            ViewBag.Message = mode == "in"
                ? "Check-in effettuato con successo"
                : "Check-out effettuato con successo";

            return View("Scan");
        }

        [HttpGet]
        public virtual async Task<IActionResult> Details(int id)
        {
            var visitorDto = await _visitorService.GetById(id);
            if (visitorDto == null)
                return NotFound();

            var presence = await _visitorService.GetLatestPresence(id);

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
            var presence = await _visitorService.GetPresenceById(presenceId);
            if (presence == null)
                return NotFound();

            var visitorDto = await _visitorService.GetById(presence.VisitorId);
            if (visitorDto == null)
                return NotFound();

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
                presenceId = presence.Id,
                visitorId = visitorDto.Id,
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

        [HttpPost]
        public virtual async Task<IActionResult> ForceCheckOut(int presenceId)
        {
            var presence = await _visitorService.GetPresenceById(presenceId);
            if (presence == null)
                return NotFound();

            if (presence.CheckOutTime.HasValue)
                return BadRequest(new { success = false });

            presence.CheckOutTime = DateTime.Now;
            

            return Json(new
            {
                success = true,
                checkOutTime = presence.CheckOutTime.Value.ToString("dd/MM/yyyy HH:mm")
            });
        }
    }
}
