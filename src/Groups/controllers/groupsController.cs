using Groups.service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SharedLibraries.model.dto;

namespace Groups.controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class groupsController : ControllerBase
    {
        private readonly groupsService _groupsService;

        public groupsController(groupsService groupsService)
        {
            _groupsService = groupsService;
        }

        [HttpGet]
        public async Task<IActionResult> GetGroups()
        {
            var response = await _groupsService.GetGroups();
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);

        }

        [HttpPost]
        public async Task<IActionResult> CreateGroups(List<GroupDto> groups)
        {
            var response = await _groupsService.CreateGroups(groups);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateGroups(List<GroupDtoResponse> groups)
        {
            var response = await _groupsService.UpdateGroups(groups);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteGroups(List<GroupDtoDelete> groupIds)
        {
            var response = await _groupsService.DeleteGroups(groupIds);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }
    }
}
