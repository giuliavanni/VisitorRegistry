#nullable enable

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
        private readonly VisitorService _visitorService;

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
            var visitorDto = await _visitorService.GetById(id);
            if (visitorDto == null)
                return NotFound();

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

            var presence = await _visitorService.GetLatestPresence(id);

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

        // =========================
        // Aggiungi nuovo visitatore (POST)
        // =========================
        [HttpPost]
        public virtual async Task<IActionResult> AddVisitor([FromForm] VisitorCreateDTO newVisitor)
        {
            if (!ModelState.IsValid)
                return BadRequest("Dati non validi");

            var newId = await _visitorService.Create(newVisitor);

            var createdVisitor = await _visitorService.GetById(newId);
            if (createdVisitor == null)
                return StatusCode(500, "Errore durante la creazione del visitatore");

            return Json(new
            {
                id = createdVisitor.Id,
                nome = createdVisitor.Nome,
                cognome = createdVisitor.Cognome,
                checkIn = newVisitor.CheckIn?.ToString("dd/MM/yyyy HH:mm") ?? "—",
                checkOut = newVisitor.CheckOut?.ToString("dd/MM/yyyy HH:mm") ?? "—",
                statoVisita = newVisitor.CheckIn == null ? "Visita programmata" : "Visita in corso",
                currentPresenceId = newId
            });
        }

        // =========================
        // Modifica visitatore (POST)
        // =========================
        [HttpPost]
        public virtual async Task<IActionResult> EditVisitor([FromForm] VisitorEditDTO editedVisitor)
        {
            if (!ModelState.IsValid)
                return BadRequest("Dati non validi");

            // Recupera il visitatore esistente
            var existingVisitor = await _visitorService.GetById(editedVisitor.Id);
            if (existingVisitor == null)
                return NotFound();

            // Mantiene il QR Code esistente (NON MODIFICABILE)
            editedVisitor.QrCode = existingVisitor.QrCode;

            // Recupera l'ultima presenza per aggiornarla
            var latestPresence = await _visitorService.GetLatestPresence(editedVisitor.Id);

            // Usa il metodo che aggiorna sia Visitor che Presence
            var success = await _visitorService.VisitorUpdateWithPresence(
                editedVisitor,
                latestPresence?.Id
            );

            if (!success)
                return StatusCode(500, "Errore durante l'aggiornamento del visitatore");

            return Json(new { message = "Visitatore aggiornato con successo" });
        }
    }
}
