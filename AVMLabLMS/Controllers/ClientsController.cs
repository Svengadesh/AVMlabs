using Microsoft.AspNetCore.Mvc;

namespace AVMLabLMS.Controllers
{
    public class ClientsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Ledger(int id)
        {
            ViewBag.ClientId = id;
            return View();
        }
    }
}
