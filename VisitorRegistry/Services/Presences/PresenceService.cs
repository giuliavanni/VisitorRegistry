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
    }

    // =====================================
    // Risultato tipizzato (MOLTO meglio di bool)
    // =====================================
    public enum PresenceActionResult
    {
        InvalidQr,
        CheckedIn,
        CheckedOut
    }
}

