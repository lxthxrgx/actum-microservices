using Counterparty.service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SharedLibraries.model.dto;

namespace Counterparty.controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class counterpartyController : ControllerBase
    {
        private readonly CounterpartyService _counterpartyService;

        public counterpartyController(CounterpartyService counterpartyService)
        {
            _counterpartyService = counterpartyService;
        }

        [HttpGet]
        public async Task<IActionResult> GetCounterparty()
        {
            var response = await _counterpartyService.GetCounterparty();
            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCounterpartyById(counterpartyIdDto counterparty)
        {
            var response = await _counterpartyService.GetCounterpartyById(counterparty);

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCounterparty(List<counterpartyDto> counterparty)
        {
            var response = await _counterpartyService.CreateCounterparty(counterparty);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateCounterparty([FromBody] List<counterpartyDto> counterparty)
        {
            var response = await _counterpartyService.UpdateCounterparty(counterparty);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteCounterparty(List<counterpartyDeleteDto> counterparty)
        {
            var response = await _counterpartyService.DeleteCounterparty(counterparty);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

    }
}
