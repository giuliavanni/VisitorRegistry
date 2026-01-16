using System;

namespace VisitorRegistry.Web.Features.Presence
{
    public class PresenceDetailsViewModel
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Cognome { get; set; }
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public DateTime DataVisita { get; set; }
        public string Referente { get; set; }
        public string Ditta { get; set; }
        public string QrCode { get; set; }
    }
}
