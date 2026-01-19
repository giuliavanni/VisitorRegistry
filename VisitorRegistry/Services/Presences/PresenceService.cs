using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VisitorRegistry.Services
{
    public class PresenceService
    {
        private readonly TemplateDbContext _db;

        public PresenceService(TemplateDbContext db)
        {
            _db = db;
        }

        public virtual async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
        }

        // =====================================
        // Toggle check-in / check-out via QR
        // =====================================
        public virtual async Task<PresenceActionResult> ToggleByQrAsync(string qrCode)
        {
            var visitor = await _db.Visitors
                .FirstOrDefaultAsync(v => v.QrCode == qrCode);

            if (visitor == null)
                return PresenceActionResult.InvalidQr;

            return await ToggleByVisitorIdAsync(visitor.Id);
        }

        // =====================================
        // Toggle check-in / check-out via VisitorId
        // =====================================
        public virtual async Task<PresenceActionResult> ToggleByVisitorIdAsync(int visitorId)
        {
            var openPresence = await _db.Presences
                .Where(p => p.VisitorId == visitorId && p.CheckOutTime == null)
                .OrderByDescending(p => p.CheckInTime)
                .FirstOrDefaultAsync();

            if (openPresence == null)
            {
                _db.Presences.Add(new Presence
                {
                    VisitorId = visitorId,
                    CheckInTime = DateTime.Now
                });

                await _db.SaveChangesAsync();
                return PresenceActionResult.CheckedIn;
            }
            else
            {
                openPresence.CheckOutTime = DateTime.Now;
                await _db.SaveChangesAsync();
                return PresenceActionResult.CheckedOut;
            }
        }

        // =====================================
        // Visitatori attualmente dentro
        // =====================================
        public virtual async Task<List<Presence>> GetVisitorsInsideAsync()
        {
            return await _db.Presences
                .Where(p => p.CheckOutTime == null)
                .Include(p => p.Visitor)
                .ToListAsync();
        }

        // =====================================
        // Storico presenze di un visitatore
        // =====================================
        public virtual async Task<List<Presence>> GetPresencesByVisitorAsync(int visitorId)
        {
            return await _db.Presences
                .Where(p => p.VisitorId == visitorId)
                .OrderByDescending(p => p.CheckInTime)
                .ToListAsync();
        }

        public virtual async Task<PresenceActionResult> CheckInByQrAsync(string qrCode)
        {
            var visitor = await _db.Visitors
                .Include(v => v.Presences)
                .FirstOrDefaultAsync(v => v.QrCode == qrCode);

            if (visitor == null)
                return PresenceActionResult.InvalidQr;

            var lastPresence = visitor.Presences.OrderByDescending(p => p.CheckInTime).FirstOrDefault();

            if (lastPresence != null && lastPresence.CheckOutTime == null)
                return PresenceActionResult.AlreadyCheckedIn;

            var newPresence = new Presence
            {
                VisitorId = visitor.Id,
                CheckInTime = DateTime.Now
            };

            _db.Presences.Add(newPresence);
            await _db.SaveChangesAsync();

            return PresenceActionResult.CheckedIn;
        }

        public virtual async Task<PresenceActionResult> CheckOutByQrAsync(string qrCode)
        {
            var visitor = await _db.Visitors
                .Include(v => v.Presences)
                .FirstOrDefaultAsync(v => v.QrCode == qrCode);

            if (visitor == null)
                return PresenceActionResult.InvalidQr;

            var lastPresence = visitor.Presences.OrderByDescending(p => p.CheckInTime).FirstOrDefault();

            if (lastPresence == null || lastPresence.CheckOutTime != null)
                return PresenceActionResult.AlreadyCheckedOut;

            lastPresence.CheckOutTime = DateTime.Now;
            await _db.SaveChangesAsync();

            return PresenceActionResult.CheckedOut;
        }
        // Restituisce una singola visita per ID presenza
        public virtual async Task<Presence?> GetByIdAsync(int presenceId)
        {
            return await _db.Presences
                .Include(p => p.Visitor)  // Include i dati del visitatore
                .FirstOrDefaultAsync(p => p.Id == presenceId);
        }

        // Opzionale: tutte le visite di un visitor
        public virtual async Task<List<Presence>> GetAllByVisitorIdAsync(int visitorId)
        {
            return await _db.Presences
                .Where(p => p.VisitorId == visitorId)
                .OrderByDescending(p => p.CheckInTime)
                .ToListAsync();
        }
    }

    // =====================================
    // Risultato tipizzato 
    // =====================================
    public enum PresenceActionResult
    {
        CheckedIn,
        CheckedOut,
        AlreadyCheckedIn,
        AlreadyCheckedOut,
        InvalidQr
    }

}

