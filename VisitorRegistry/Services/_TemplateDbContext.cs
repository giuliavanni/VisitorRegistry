using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using VisitorRegistry.Infrastructure;
using VisitorRegistry.Services.Shared;
using UserModel = VisitorRegistry.Services.Shared.User;

namespace VisitorRegistry.Services
{
    public class TemplateDbContext : DbContext
    {
        public TemplateDbContext()
        {
        }

        public TemplateDbContext(DbContextOptions<TemplateDbContext> options) : base(options)
        {
            //DataGenerator.InitializeUsers(this);
        }

        public DbSet<UserModel> Users { get; set; }
        public DbSet<Visitor> Visitors { get; set; }
        public DbSet<Presence> Presences { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Relazione Visitor → Presence (1 a molti)
            modelBuilder.Entity<Visitor>()
                .HasMany(v => v.Presences)
                .WithOne(p => p.Visitor)
                .HasForeignKey(p => p.VisitorId)
                .OnDelete(DeleteBehavior.Cascade);

            var userId = Guid.NewGuid();
            var dateVisita = new DateTime(2025, 12, 24);
            var datePresence = new DateTime(2025, 12, 24, 10, 0, 0);
            
            var sha256 = SHA256.Create();
            var hashed = Convert.ToBase64String(
                sha256.ComputeHash(Encoding.ASCII.GetBytes("password"))
            );

            // Esempio di seeding dati iniziali
            modelBuilder.Entity<UserModel>().HasData(
                new UserModel { Id = userId, Email = "admin@example.com", Password = hashed, FirstName = "Costanzo", LastName = "Buonarroti", NickName = "Admin" }
            );

            modelBuilder.Entity<Visitor>().HasData(
            new Visitor { Id = 1, Nome = "Mario", Cognome = "Rossi", DataVisita = dateVisita, QrCode = "abcdefg" }
            );

            modelBuilder.Entity<Presence>().HasData(
                new Presence { Id = 1, Date = datePresence, VisitorId = 1, CheckInTime = new DateTime(2025, 12, 24, 10, 0, 0),CheckOutTime = null }
            );
        }
    }
}


public class Visitor
{
    public int Id { get; set; }
    public string Nome { get; set; }
    public string Cognome { get; set; }
    public DateTime DataVisita { get; set; }
    public string QrCode { get; set; }


    // Un visitor può avere molte presenze
    public ICollection<Presence> Presences { get; set; }
}

public class Presence
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public DateTime CheckInTime { get; set; }   
    public DateTime? CheckOutTime { get; set; }

    // Chiave esterna e navigation property
    public int VisitorId { get; set; }
    public Visitor Visitor { get; set; }
}