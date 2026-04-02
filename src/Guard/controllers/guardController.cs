using Guard.service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SharedLibraries.model.dto;

namespace Guard.controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class guardController : ControllerBase
    {
        private readonly GuardService _service;

        public guardController(GuardService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetGuards()
        {
            var response = await _service.GetGuard();
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);

        }

        [HttpPost]
        public async Task<IActionResult> CreateGuards(List<guardDto> guards)
        {
            var response = await _service.CreateGuard(guards);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateGuards(List<guardDtoResponse> guards)
        {
            var response = await _service.UpdateGuard(guards);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteGuards(List<guardDtoDelete> guards)
        {
            var response = await _service.DeleteGuard(guards);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }
    }
}
