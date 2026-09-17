using AVMLabLMS.DTOs;
using AVMLabLMS.Services;
using Microsoft.AspNetCore.Mvc;

namespace AVMLabLMS.ApiControllers
{
    [ApiController]
    [Route("api/reports")]
    public class ReportsApiController : ControllerBase
    {
        private readonly ReportService _reportService;

        public ReportsApiController(ReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet("daily-summary")]
        public async Task<ActionResult<List<DailySummaryDTO>>> GetDailySummary([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            var fromDate = from ?? DateTime.UtcNow.AddDays(-30);
            var toDate = to ?? DateTime.UtcNow;

            var summary = await _reportService.GetDailySummaryAsync(fromDate, toDate);
            return Ok(summary);
        }

        [HttpGet("outstanding-clients")]
        public async Task<ActionResult<object>> GetOutstandingClients()
        {
            var report = await _reportService.GetDashboardReportAsync();
            return Ok(report);
        }

        [HttpGet("gateway-fees")]
        public async Task<ActionResult<object>> GetGatewayFees([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            var fees = await _reportService.GetTotalGatewayFeesAsync(from, to);
            return Ok(new { TotalGatewayFees = fees });
        }
    }
}
