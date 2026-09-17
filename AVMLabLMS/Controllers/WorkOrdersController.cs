using Microsoft.AspNetCore.Mvc;
using AVMLabLMS.Data;
using Microsoft.EntityFrameworkCore;

namespace AVMLabLMS.Controllers
{
    public class WorkOrdersController : Controller
    {
        private readonly AppDbContext _context;

        public WorkOrdersController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Create()
        {
            var tests = await _context.Tests
                .Where(t => t.IsActive)
                .Select(t => new { id = t.TestId, code = t.TestCode, name = t.TestName, rate = t.Rate })
                .ToListAsync();
            
            ViewBag.TestsJson = System.Text.Json.JsonSerializer.Serialize(tests);
            return View();
        }
    }
}
