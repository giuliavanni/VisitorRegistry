#nullable enable

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VisitorRegistry.Services.Shared;

namespace VisitorRegistry.Services.Visitors
{
    public class VisitorService
    {
        private readonly TemplateDbContext _db;

        public VisitorService(TemplateDbContext db)
        {
            _db = db;
        }

        // =========================
        // CREATE
        // =========================
        public async Task<int> Create(VisitorCreateDTO dto)
        {
            var visitor = new Visitor
            {
                Nome = dto.Nome,
                Cognome = dto.Cognome,
                DataVisita = dto.DataVisita ?? DateTime.Now,
                QrCode = dto.QrCode ?? Guid.NewGuid().ToString(),
                Ditta = dto.Ditta,
                Referente = dto.Referente
            };
            

            if (dto.CheckIn.HasValue || dto.CheckOut.HasValue)
            {
                visitor.Presences = new List<Presence>
                {
                    new Presence
                    {
                        CheckInTime = dto.CheckIn ?? DateTime.Now,
                        CheckOutTime = dto.CheckOut
                    }
                };
            }

            _db.Visitors.Add(visitor);
            await _db.SaveChangesAsync();

            return visitor.Id;
        }

        // =========================
        // READ - LIST
        // =========================
        public async Task<List<VisitorListDTO>> GetAll()
        {
            var data = await _db.Visitors
                .Include(v => v.Presences)
                .OrderBy(v => v.Cognome)
                .ToListAsync(); // ? qui FINISCE SQL

            return data.Select(v =>
            {
                var lastPresence = v.Presences
                    .OrderByDescending(p => p.CheckInTime)
                    .FirstOrDefault();

                return new VisitorListDTO
                {
                    Id = v.Id,
                    Nome = v.Nome,
                    Cognome = v.Cognome,

                    CheckIn = lastPresence?.CheckInTime,
                    CheckOut = lastPresence?.CheckOutTime,
                    CurrentPresenceId = lastPresence?.Id,
                    DataVisita = v.DataVisita,

                    StatoVisita = lastPresence == null
                        ? "Visita programmata"
                        : lastPresence.CheckOutTime == null
                            ? "Dentro"
                            : "Uscito"
                };
            }).ToList();
        }



        // =========================
        // READ - DETAIL
        // =========================
        public async Task<VisitorDetailDTO?> GetById(int id)
        {
            return await _db.Visitors
                .Include(v => v.Presences)
                .Where(v => v.Id == id)
                .Select(v => new VisitorDetailDTO
                {
                    Id = v.Id,
                    Nome = v.Nome,
                    Cognome = v.Cognome,
                    DataVisita = v.DataVisita,
                    QrCode = v.QrCode,
                    Ditta = v.Ditta,
                    Referente = v.Referente,
                    CheckInTime = v.Presences
                        .OrderByDescending(p => p.CheckInTime)
                        .Select(p => (DateTime?)p.CheckInTime)
                        .FirstOrDefault(),
                    CheckOutTime = v.Presences
                        .OrderByDescending(p => p.CheckInTime)
                        .Select(p => p.CheckOutTime)
                        .FirstOrDefault()
                })
                .FirstOrDefaultAsync();
        }

        // =========================
        // UPDATE
        // =========================
        public async Task<bool> VisitorUpdate(VisitorEditDTO dto)
        {
            var visitor = await _db.Visitors.FindAsync(dto.Id);
            if (visitor == null)
                return false;

            visitor.Nome = dto.Nome;
            visitor.Cognome = dto.Cognome;
            visitor.Ditta = dto.Ditta;
            visitor.Referente = dto.Referente;
            visitor.DataVisita = dto.DataVisita.Value;

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdatePlanned(VisitorUpdateDTO dto)
        {
            var visitor = await _db.Visitors
                .FirstOrDefaultAsync(v => v.Id == dto.Id);

            if (visitor == null)
                return false;

            visitor.Nome = dto.Nome;
            visitor.Cognome = dto.Cognome;
            visitor.Ditta = dto.Ditta;
            visitor.Referente = dto.Referente;
            visitor.DataVisita = dto.DataVisita.Value;

            await _db.SaveChangesAsync();
            return true;
        }


        // =========================
        // UPDATE CON PRESENCE
        // =========================
        public async Task<bool> VisitorUpdateWithPresence(VisitorEditDTO dto)
        {
            var visitor = await _db.Visitors.FindAsync(dto.Id);
            if (visitor == null) return false;

            visitor.Nome = dto.Nome;
            visitor.Cognome = dto.Cognome;
            visitor.Ditta = dto.Ditta;
            visitor.Referente = dto.Referente;

            await _db.SaveChangesAsync();
            return true;
        }


        // =========================
        // DELETE
        // =========================
        public async Task<bool> Delete(int id)
        {
            var visitor = await _db.Visitors
                .Include(v => v.Presences)  // carica anche le presenze
                .FirstOrDefaultAsync(v => v.Id == id);

            if (visitor == null)
                return false;

            // elimina prima le presenze collegate
            _db.Presences.RemoveRange(visitor.Presences);

            // poi elimina il visitor
            _db.Visitors.Remove(visitor);

            await _db.SaveChangesAsync();
            return true;
        }

        // =========================
        // GET BY QR CODE
        // =========================
        public async Task<VisitorDetailDTO?> GetByQrCode(string qr)
        {
            return await _db.Visitors
                .Include(v => v.Presences)
                .Where(v => v.QrCode == qr)
                .Select(v => new VisitorDetailDTO
                {
                    Id = v.Id,
                    Nome = v.Nome,
                    Cognome = v.Cognome,
                    DataVisita = v.DataVisita,
                    QrCode = v.QrCode,
                    Ditta = v.Ditta,
                    Referente = v.Referente,
                    CheckInTime = v.Presences
                        .OrderByDescending(p => p.CheckInTime)
                        .Select(p => (DateTime?)p.CheckInTime)
                        .FirstOrDefault(),
                    CheckOutTime = v.Presences
                        .OrderByDescending(p => p.CheckInTime)
                        .Select(p => p.CheckOutTime)
                        .FirstOrDefault()
                })
                .FirstOrDefaultAsync();
        }

        public async Task<Visitor?> GetByQrCodeAsync(string qrCode)
        {
            return await _db.Visitors
                .FirstOrDefaultAsync(v => v.QrCode == qrCode);
        }

        // =========================
        // PRESENCE
        // =========================
        public async Task<Presence?> GetLatestPresence(int visitorId)
        {
            return await _db.Presences
                .Where(p => p.VisitorId == visitorId)
                .OrderByDescending(p => p.CheckInTime)
                .FirstOrDefaultAsync();
        }

        public async Task<Presence?> GetPresenceById(int presenceId)
        {
            return await _db.Presences
                .FirstOrDefaultAsync(p => p.Id == presenceId);
        }

        public async Task<bool> UpdatePresence(int visitorId, string mode)
        {
            var presence = await _db.Presences
                .Where(p => p.VisitorId == visitorId)
                .OrderByDescending(p => p.CheckInTime)
                .FirstOrDefaultAsync();

            if (mode == "out")
            {
                if (presence == null)
                {
                    presence = new Presence
                    {
                        VisitorId = visitorId,
                        CheckInTime = DateTime.Now,
                        CheckOutTime = DateTime.Now
                    };
                    _db.Presences.Add(presence);
                }
                else if (presence.CheckOutTime == null)
                {
                    presence.CheckOutTime = DateTime.Now;
                }

                await _db.SaveChangesAsync();
                return true;
            }

            if (mode == "in")
            {
                _db.Presences.Add(new Presence
                {
                    VisitorId = visitorId,
                    CheckInTime = DateTime.Now
                });

                await _db.SaveChangesAsync();
                return true;
            }

            return false;
        }

        // =========================
        // FORCE CHECK-OUT
        // =========================
        public async Task<Presence?> ForceCheckoutAsync(int visitorId)
        {
            var presence = await _db.Presences
                .Where(p => p.VisitorId == visitorId && p.CheckOutTime == null)
                .OrderByDescending(p => p.CheckInTime)
                .FirstOrDefaultAsync();

            if (presence == null)
                return null;

            presence.CheckOutTime = DateTime.Now;
            await _db.SaveChangesAsync();

            return presence;
        }

    }
}
