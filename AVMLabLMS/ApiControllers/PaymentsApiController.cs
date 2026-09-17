using AVMLabLMS.DTOs;
using AVMLabLMS.Services;
using Microsoft.AspNetCore.Mvc;

namespace AVMLabLMS.ApiControllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsApiController : ControllerBase
    {
        private readonly PaymentService _paymentService;

        public PaymentsApiController(PaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost]
        public async Task<ActionResult<PaymentResponseDTO>> RecordPayment([FromBody] RecordPaymentDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _paymentService.RecordPaymentAsync(dto);
            if (!response.Success)
                return BadRequest(new { Error = response.Message });

            return Ok(response);
        }
    }
}
