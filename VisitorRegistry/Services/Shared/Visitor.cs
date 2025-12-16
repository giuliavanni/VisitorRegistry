using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;
using System.Text;

namespace VisitorRegistry.Services.Shared
{
    public class Visitor
    {
        public int Id { get; set; }

        public string Nome { get; set; } = string.Empty;

        public string Cognome { get; set; } = string.Empty;

        public string QrCode { get; set; } = string.Empty;

        // Navigazione EF
        public ICollection<Presence> Presences { get; set; } = new List<Presence>();
    }
}