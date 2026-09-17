using Microsoft.AspNetCore.Mvc;

namespace AVMLabLMS.Controllers
{
    public class WorkOrdersController : Controller
    {
        public IActionResult Create()
        {
            return View();
        }
    }
}
