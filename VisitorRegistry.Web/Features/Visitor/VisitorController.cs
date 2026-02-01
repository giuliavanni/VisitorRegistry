#nullable enable

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using System;
using System.IO;
using System.Threading.Tasks;
using VisitorRegistry.Services.Shared;
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
        [HttpPost]
        public virtual async Task<IActionResult> CancelVisitor(int id)
        {
            var success = await _visitorService.Delete(id);
            if (!success)
                return NotFound();
            return Ok();
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

            
            var latestPresence = await _visitorService.GetLatestPresence(newId);
            int? actualPresenceId = latestPresence?.Id;

            // Determinare lo stato visita
            string statoVisita;
            if (newVisitor.CheckIn == null)
            {
                statoVisita = "Visita programmata";
            }
            else if (newVisitor.CheckOut == null)
            {
                statoVisita = "Visita in corso";
            }
            else
            {
                statoVisita = "Visita terminata";
            }

            return Json(new
            {
                id = createdVisitor.Id,
                nome = createdVisitor.Nome,
                cognome = createdVisitor.Cognome,
                checkIn = newVisitor.CheckIn?.ToString("dd/MM/yyyy HH:mm") ?? "—",
                checkOut = newVisitor.CheckOut?.ToString("dd/MM/yyyy HH:mm") ?? "—",
                ditta = createdVisitor.Ditta,
                referente = createdVisitor.Referente,
                statoVisita = statoVisita,
                currentPresenceId = actualPresenceId  
            });
        }
        // =========================
        // Modifica visitatore (POST)
        // =========================
        [HttpPost]
        public virtual async Task<IActionResult> EditVisitor([FromForm] VisitorEditDTO editedVisitor)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var success = await _visitorService.VisitorUpdateWithPresence(editedVisitor);

            if (!success)
                return StatusCode(500);

            return Json(new
            {
                visitorId = editedVisitor.Id,
                nome = editedVisitor.Nome,
                cognome = editedVisitor.Cognome,
                ditta = editedVisitor.Ditta,
                referente = editedVisitor.Referente
            });
        }
        

        [HttpPost]
        public virtual async Task<IActionResult> Delete(int id)
        {
            var result = await _visitorService.Delete(id);
            return Json(new { success = result });
        }



        // =========================
        // FORZA CHECK-OUT (POST)
        // =========================
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public virtual async Task<IActionResult> UpdatePresence(int visitorId, string mode)
        {
            if (mode != "out")
            {
                return Json(new
                {
                    success = false,
                    message = "Modalità non valida"
                });
            }

            var presence = await _visitorService.ForceCheckoutAsync(visitorId);

            if (presence == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Presenza non trovata o già chiusa"
                });
            }

            return Json(new
            {
                success = true,
                checkOutTime = presence.CheckOutTime!.Value.ToString("yyyy-MM-ddTHH:mm:ss")
            });
        }

    }
}
