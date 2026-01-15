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
                DataVisita = dto.DataVisita,
                QrCode = dto.QrCode
            };

            _db.Visitors.Add(visitor);
            await _db.SaveChangesAsync();

            return visitor.Id;
        }

        // =========================
        // READ - LIST
        // =========================
        public async Task<List<VisitorListDTO>> GetAll()
        {
            return await _db.Visitors
                .Include(v => v.Presences)
                .OrderBy(v => v.Cognome)
                .Select(v => new VisitorListDTO
                {
                    Id = v.Id,
                    Nome = v.Nome,
                    Cognome = v.Cognome,

                    CheckIn = v.Presences
                        .OrderByDescending(p => p.CheckInTime)
                        .Select(p => (DateTime?)p.CheckInTime)
                        .FirstOrDefault(),

                    CheckOut = v.Presences
                        .OrderByDescending(p => p.CheckInTime)
                        .Select(p => p.CheckOutTime)
                        .FirstOrDefault(),

                    StatoVisita =
                        v.Presences.Any() == false
                            ? "Visita programmata"
                            : v.Presences
                                .OrderByDescending(p => p.CheckInTime)
                                .Select(p => p.CheckOutTime == null ? "Dentro" : "Uscito")
                                .FirstOrDefault()
                })
                .ToListAsync();
        }

        // =========================
        // READ - DETAIL
        // =========================
        public async Task<VisitorDetailDTO?> GetById(int id)
        {
            return await _db.Visitors
                .Where(v => v.Id == id)
                .Select(v => new VisitorDetailDTO
                {
                    Id = v.Id,
                    Nome = v.Nome,
                    Cognome = v.Cognome,
                    DataVisita = v.DataVisita,
                    QrCode = v.QrCode
                })
                .FirstOrDefaultAsync();
        }

        // =========================
        // UPDATE
        // =========================
        public async Task<bool> Update(int id, VisitorUpdateDTO dto)
        {
            var visitor = await _db.Visitors.FindAsync(id);
            if (visitor == null)
                return false;

            visitor.Nome = dto.Nome;
            visitor.Cognome = dto.Cognome;
            visitor.DataVisita = dto.DataVisita;
            visitor.QrCode = dto.QrCode;

            await _db.SaveChangesAsync();
            return true;
        }

        // =========================
        // DELETE
        // =========================
        public async Task<bool> Delete(int id)
        {
            var visitor = await _db.Visitors.FindAsync(id);
            if (visitor == null)
                return false;

            _db.Visitors.Remove(visitor);
            await _db.SaveChangesAsync();
            return true;
        }

        public virtual async Task<VisitorDetailDTO?> GetByQrCode(string qr)
        {
            return await _db.Visitors
                .Where(v => v.QrCode == qr)
                .Select(v => new VisitorDetailDTO
                {
                    Id = v.Id,
                    Nome = v.Nome,
                    Cognome = v.Cognome,
                    DataVisita = v.DataVisita,
                    QrCode = v.QrCode
                })
                .FirstOrDefaultAsync();
        }

        public virtual async Task<Visitor?> GetByQrCodeAsync(string qrCode)
        {
            return await _db.Visitors
                .FirstOrDefaultAsync(v => v.QrCode == qrCode);
        }
    }
}
