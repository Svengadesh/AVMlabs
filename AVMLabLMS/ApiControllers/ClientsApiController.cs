using AVMLabLMS.DTOs;
using AVMLabLMS.Services;
using Microsoft.AspNetCore.Mvc;

namespace AVMLabLMS.ApiControllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClientsApiController : ControllerBase
    {
        private readonly ClientService _clientService;

        public ClientsApiController(ClientService clientService)
        {
            _clientService = clientService;
        }

        [HttpGet]
        public async Task<ActionResult<List<ClientDTO>>> GetClients([FromQuery] string search, [FromQuery] string country, [FromQuery] int page = 1)
        {
            var clients = await _clientService.GetClientsAsync(search, country, page);
            return Ok(clients);
        }

        [HttpGet("{id}/ledger")]
        public async Task<ActionResult<ClientLedgerDTO>> GetLedger(int id)
        {
            var ledger = await _clientService.GetClientLedgerAsync(id);
            if (ledger == null) return NotFound();
            return Ok(ledger);
        }

        [HttpGet("{id}/nbl-status")]
        public async Task<ActionResult<object>> GetNblStatus(int id)
        {
            var status = await _clientService.GetNblStatusAsync(id);
            return Ok(new { NblStatus = status });
        }

        [HttpPut("{id}/toggle")]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var success = await _clientService.ToggleStatusAsync(id);
            if (!success) return NotFound();
            return Ok(new { Message = "Status toggled successfully." });
        }
    }
}
