using System.ComponentModel.DataAnnotations;

namespace VisitorRegistry.Web.Features.Home
{
    public class VisitorViewModel
    {
        [Required(ErrorMessage = "Nome obbligatorio")]
        public string Nome { get; set; }

        [Required(ErrorMessage = "Cognome obbligatorio")]
        public string Cognome { get; set; }
        public string Ditta { get; set; }
        public string Referente { get; set; }
    }
}