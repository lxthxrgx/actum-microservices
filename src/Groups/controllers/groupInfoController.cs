using Groups.service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SharedLibraries.model;
using SharedLibraries.model.dto;

namespace Groups.controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class groupInfoController : ControllerBase
    {
        private readonly GroupInfoService _groupInfoService;

        public groupInfoController(GroupInfoService groupInfoService)
        {
            _groupInfoService = groupInfoService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllGroupInfo()
        {
            var response = await _groupInfoService.GetAllGroupInfo();
            if (!response.Success)
                return BadRequest(response);
            return Ok(response);
        }

        [HttpPost("{groupId}/rent/sublease")]
        public async Task<IActionResult> CreateSublease(Guid groupId, [FromBody] SubleaseRentInfoDto dto)
            => Ok(await _groupInfoService.CreateSubleaseRentInfo(groupId, dto));

        [HttpPost("{groupId}/rent/type1")]
        public async Task<IActionResult> CreateType1(Guid groupId, [FromBody] RentType1InfoDto dto)
            => Ok(await _groupInfoService.CreateRentType1Info(groupId, dto));

        [HttpPost("{groupId}/rent/type2")]
        public async Task<IActionResult> CreateType2(Guid groupId, [FromBody] RentType2InfoDto dto)
            => Ok(await _groupInfoService.CreateRentType2Info(groupId, dto));

        [HttpPut("{groupId}/rent/sublease")]
        public async Task<IActionResult> UpdateSublease(Guid groupId, [FromBody] SubleaseRentInfoDto dto)
            => Ok(await _groupInfoService.UpdateSubleaseRentInfo(groupId, dto));

        [HttpPut("{groupId}/rent/type1")]
        public async Task<IActionResult> UpdateType1(Guid groupId, [FromBody] RentType1InfoDto dto)
            => Ok(await _groupInfoService.UpdateRentType1Info(groupId, dto));

        [HttpPut("{groupId}/rent/type2")]
        public async Task<IActionResult> UpdateType2(Guid groupId, [FromBody] RentType2InfoDto dto)
            => Ok(await _groupInfoService.UpdateRentType2Info(groupId, dto));

        [HttpDelete("{groupId}/rent")]
        public async Task<IActionResult> DeleteRentInfo(Guid groupId)
            => Ok(await _groupInfoService.DeleteGroupRentInfo(groupId));
    }
}
