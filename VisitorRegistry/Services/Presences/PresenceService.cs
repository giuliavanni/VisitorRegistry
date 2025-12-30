using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VisitorRegistry.Services.Visitors;

namespace VisitorRegistry.Services
{
    public class PresenceService
    {
        private readonly TemplateDbContext _db;
        private readonly VisitorService _visitorService;

        public PresenceService(TemplateDbContext db, VisitorService visitorService)
        {
            _db = db;
            _visitorService = visitorService;
        }

        // =========================
        // Check-in tramite QR code
        // =========================
        public async Task<bool> CheckIn(string qrCode)
        {
            var visitor = await _db.Visitors.FirstOrDefaultAsync(v => v.QrCode == qrCode);
            if (visitor == null)
                return false;

            var presence = new Presence
            {
                VisitorId = visitor.Id,
                CheckInTime = DateTime.Now,
                CheckOutTime = null
            };

            _db.Presences.Add(presence);
            await _db.SaveChangesAsync();
            return true;
        }

        // =========================
        // Check-out tramite QR code
        // =========================
        public async Task<bool> CheckOut(string qrCode)
        {
            var visitor = await _db.Visitors.FirstOrDefaultAsync(v => v.QrCode == qrCode);
            if (visitor == null)
                return false;

            var presence = await _db.Presences
                .Where(p => p.VisitorId == visitor.Id && p.CheckOutTime == null)
                .OrderByDescending(p => p.CheckInTime)
                .FirstOrDefaultAsync();

            if (presence == null)
                return false;

            presence.CheckOutTime = DateTime.Now;
            await _db.SaveChangesAsync();
            return true;
        }

        // =========================
        // Check-in automatico tramite visitorId (opzionale)
        // =========================
        public async Task<Presence> CheckInAsync(int visitorId)
        {
            var presence = new Presence
            {
                VisitorId = visitorId,
                CheckInTime = DateTime.Now,
                CheckOutTime = null
            };

            _db.Presences.Add(presence);
            await _db.SaveChangesAsync();

            return presence;
        }

        // =========================
        // Check-out automatico tramite visitorId (opzionale)
        // =========================
        public async Task<bool> CheckOutAsync(int visitorId)
        {
            var presence = await _db.Presences
                .Where(p => p.VisitorId == visitorId && p.CheckOutTime == null)
                .OrderByDescending(p => p.CheckInTime)
                .FirstOrDefaultAsync();

            if (presence == null)
                return false;

            presence.CheckOutTime = DateTime.Now;
            await _db.SaveChangesAsync();
            return true;
        }

        // =========================
        // Ottieni tutte le presenze di un visitatore
        // =========================
        public async Task<List<Presence>> GetPresencesByVisitorAsync(int visitorId)
        {
            return await _db.Presences
                .Where(p => p.VisitorId == visitorId)
                .OrderByDescending(p => p.CheckInTime)
                .ToListAsync();
        }

        // =========================
        // Ottieni visitatori attualmente dentro
        // =========================
        public async Task<List<Presence>> GetVisitorsInsideAsync()
        {
            return await _db.Presences
                .Where(p => p.CheckOutTime == null)
                .Include(p => p.Visitor)
                .ToListAsync();
        }
    }
}
