using System;

namespace VisitorRegistry.Services.Visitors
{
    public class VisitorCreateDTO
    {
        public string Nome { get; set; } = "";
        public string Cognome { get; set; } = "";
        public string Ditta { get; set; } = "";
        public string Referente { get; set; } = "";
        public DateTime? DataVisita { get; set; }
        public string? QrCode { get; set; }
        public DateTime? CheckIn { get; set; }
        public DateTime? CheckOut { get; set; }
    }

    public class VisitorUpdateDTO
    {
        public int Id { get; set; }
        public string Nome { get; set; } = "";
        public string Cognome { get; set; } = "";
        public string Ditta { get; set; } = "";
        public string Referente { get; set; } = "";
        public DateTime? DataVisita { get; set; }
        public string? QrCode { get; set; }
    }

    public class VisitorDetailDTO
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Cognome { get; set; }
        public string Ditta { get; set; }
        public string Referente { get; set; }
        public DateTime? DataVisita { get; set; }
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public string QrCode { get; set; }
    }

    public class VisitorListDTO
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Cognome { get; set; }
        public DateTime? CheckIn { get; set; }
        public DateTime? CheckOut { get; set; }
        public DateTime? DataVisita { get; set; }
        public string StatoVisita { get; set; } = "-";
        public int? CurrentPresenceId { get; set; }   //id della visita mostrata nella riga
        public string QrCode { get; set; }
    }
    public class VisitorEditDTO
    {
        public int? PresenceId { get; set; }
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Cognome { get; set; }
        public string Ditta { get; set; }
        public string Referente { get; set; }
        public DateTime? DataVisita { get; set; }
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public string QrCode { get; set; } // rimane invariato
    }


}
