using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using VisitorRegistry.Services.Visitors;
using VisitorRegistry.Services.Shared;

namespace VisitorRegistry.Web.Features.Visitors
{
    public partial class VisitorController : Controller
    {
        private readonly VisitorService _visitorService;

        public VisitorController(VisitorService visitorService)
        {
            _visitorService = visitorService;
        }

        // =========================
        // LIST PAGE
        // =========================
        [HttpGet]
        public virtual async Task<IActionResult> Index()
        {
            var visitors = await _visitorService.GetAll();
            return View(visitors); // passerà la lista dei Visitor alla View
        }

        // =========================
        // DETAILS PAGE
        // =========================
        [HttpGet]
        public virtual async Task<IActionResult> Details(int id)
        {
            var visitor = await _visitorService.GetById(id);
            if (visitor == null)
                return NotFound();

            return View(visitor);
        }

        // =========================
        // CREATE PAGE
        // =========================
        [HttpGet]
        public virtual IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual async Task<IActionResult> Create(VisitorCreateDTO dto)
        {
            if (ModelState.IsValid)
            {
                await _visitorService.Create(dto);
                return RedirectToAction(nameof(Index));
            }
            return View(dto);
        }

        // =========================
        // EDIT PAGE
        // =========================
        [HttpGet]
        public virtual async Task<IActionResult> Edit(int id)
        {
            var visitor = await _visitorService.GetById(id);
            if (visitor == null)
                return NotFound();

            // Mappa VisitorDetailDTO a VisitorUpdateDTO per l’edit
            var dto = new VisitorUpdateDTO
            {
                Nome = visitor.Nome,
                Cognome = visitor.Cognome,
                DataVisita = visitor.DataVisita,
                QrCode = visitor.QrCode
            };

            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual async Task<IActionResult> Edit(int id, VisitorUpdateDTO dto)
        {
            if (ModelState.IsValid)
            {
                var success = await _visitorService.Update(id, dto);
                if (!success) return NotFound();

                return RedirectToAction(nameof(Index));
            }
            return View(dto);
        }

        // =========================
        // DELETE PAGE
        // =========================
        [HttpGet]
        public virtual async Task<IActionResult> Delete(int id)
        {
            var visitor = await _visitorService.GetById(id);
            if (visitor == null) return NotFound();

            return View(visitor);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public virtual async Task<IActionResult> DeleteConfirmed(int id)
        {
            var success = await _visitorService.Delete(id);
            if (!success) return NotFound();

            return RedirectToAction(nameof(Index));
        }
    }
}
