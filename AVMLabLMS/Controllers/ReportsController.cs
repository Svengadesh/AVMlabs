using Microsoft.AspNetCore.Mvc;

namespace AVMLabLMS.Controllers
{
    public class ReportsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
