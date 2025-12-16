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

        public bool IsInside { get; set; }

        public DateTime Timestamp { get; set; }

        // Navigazione EF
        public Visitor Visitor { get; set; } = null!;
    }
}
