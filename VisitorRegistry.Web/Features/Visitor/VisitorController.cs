using Microsoft.AspNetCore.Mvc;
using VisitorRegistry.Services.Visitors;
using System.Threading.Tasks;

namespace VisitorRegistry.Web.Features.Visitor
{
    public partial class VisitorController : Controller
    {
        private readonly VisitorService _visitorService;

        public VisitorController(VisitorService visitorService)
        {
            _visitorService = visitorService;
        }

        public virtual async Task<IActionResult> Index()
        {
            var visitors = await _visitorService.GetAll();
            return View("VisitorList", visitors);
        }
    }
}
