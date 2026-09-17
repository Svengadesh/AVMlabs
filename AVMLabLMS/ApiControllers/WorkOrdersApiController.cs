using AVMLabLMS.DTOs;
using AVMLabLMS.Services;
using Microsoft.AspNetCore.Mvc;

namespace AVMLabLMS.ApiControllers
{
    [ApiController]
    [Route("api/workorders")]
    public class WorkOrdersApiController : ControllerBase
    {
        private readonly WorkOrderService _woService;

        public WorkOrdersApiController(WorkOrderService woService)
        {
            _woService = woService;
        }

        [HttpPost]
        public async Task<ActionResult<WorkOrderResponseDTO>> CreateWorkOrder([FromBody] CreateWorkOrderDTO dto)
        {
            try
            {
                var response = await _woService.CreateWorkOrderAsync(dto);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpGet("intransit")]
        public async Task<ActionResult<List<WorkOrderResponseDTO>>> GetInTransit()
        {
            var wos = await _woService.GetInTransitWorkOrdersAsync();
            return Ok(wos);
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] string newStatus)
        {
            var success = await _woService.UpdateStatusAsync(id, newStatus);
            if (!success) return BadRequest(new { Error = "Invalid status or Work Order not found." });
            return Ok(new { Message = "Status updated successfully." });
        }
    }
}
