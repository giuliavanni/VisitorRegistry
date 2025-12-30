using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisitorRegistry.Services.Shared
{
    public class Presence
    {
        public int Id { get; set; }

        public int VisitorId { get; set; }
        public DateTime CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }

        // Navigazione EF
        public Visitor Visitor { get; set; } = null!;
    }
}
