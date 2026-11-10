using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CedarDelivery.Services.Interfaces;
using CedarDelivery.ViewModels.Api;

namespace CedarDelivery.ApiControllers
{
    // /api/addresses: the signed-in user's saved delivery addresses.
    [Route("api/addresses")]
    [Authorize]
    public class AddressesApiController : BaseApiController
    {
        private readonly IAddressService _addressService;

        public AddressesApiController(IAddressService addressService)
        {
            _addressService = addressService;
        }

        [HttpGet]
        public async Task<ActionResult<List<AddressDto>>> GetMine()
        {
            return Ok(await _addressService.GetMineAsync(RequiredUserId));
        }

        [HttpPost]
        public async Task<ActionResult<AddressDto>> Create(AddressRequest request)
        {
            var address = await _addressService.CreateAsync(RequiredUserId, request);
            return StatusCode(StatusCodes.Status201Created, address);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<AddressDto>> Update(int id, AddressRequest request)
        {
            return Ok(await _addressService.UpdateAsync(id, RequiredUserId, request));
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _addressService.DeleteAsync(id, RequiredUserId);
            return NoContent();
        }

        // PATCH /api/addresses/3/default
        [HttpPatch("{id:int}/default")]
        public async Task<ActionResult<AddressDto>> SetDefault(int id)
        {
            return Ok(await _addressService.SetDefaultAsync(id, RequiredUserId));
        }
    }
}
