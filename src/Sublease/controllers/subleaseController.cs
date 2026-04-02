using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SharedLibraries.model.dto;
using Sublease.service;

namespace Sublease.controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class subleaseController : ControllerBase
    {
        private readonly subleaseService _service;

        public subleaseController(subleaseService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var response = await _service.GetSublease();

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpGet("id")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var response = await _service.GetSubleaseById(id);

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] List<subleaseDto> sublease)
        {
            var response = await _service.CreateSublease(sublease);

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpPut]
        public async Task<IActionResult> Put(List<subleaseDtoResponse> sublease)
        {
            var response = await _service.UpdateSublease(sublease);

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(List<subleaseDtoDelete> sublease)
        {
            var response = await _service.DeleteSubleases(sublease);

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
    }
}
