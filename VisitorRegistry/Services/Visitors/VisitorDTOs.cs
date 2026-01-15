using System;

namespace VisitorRegistry.Services.Visitors
{
    public class VisitorCreateDTO
    {
        public string Nome { get; set; }
        public string Cognome { get; set; }
        public DateTime DataVisita { get; set; }
        public string QrCode { get; set; }
    }

    public class VisitorUpdateDTO
    {
        public string Nome { get; set; }
        public string Cognome { get; set; }
        public DateTime DataVisita { get; set; }
        public string QrCode { get; set; }
    }

    public class VisitorDetailDTO
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Cognome { get; set; }
        public DateTime DataVisita { get; set; }
        public string QrCode { get; set; }
    }

    public class VisitorListDTO
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Cognome { get; set; }
        public DateTime? CheckIn { get; set; }
        public DateTime? CheckOut { get; set; }
        public string StatoVisita { get; set; } = "Non registrato";
        public string QrCode { get; set; }
    }
}
