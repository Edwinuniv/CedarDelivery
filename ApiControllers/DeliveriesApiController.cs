using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CedarDelivery.Services.Interfaces;
using CedarDelivery.ViewModels.Api;

namespace CedarDelivery.ApiControllers
{
    // Driver endpoints: /api/driver (profile) and /api/deliveries (work).
    [Route("api")]
    [Authorize(Policy = "DriverOrAdmin")]
    public class DeliveriesApiController : BaseApiController
    {
        private readonly IDeliveryService _deliveryService;

        public DeliveriesApiController(IDeliveryService deliveryService)
        {
            _deliveryService = deliveryService;
        }

        // ---------- Driver profile ----------

        // GET /api/driver/profile
        [HttpGet("driver/profile")]
        public async Task<ActionResult<DriverProfileDto>> GetProfile()
        {
            return Ok(await _deliveryService.GetProfileAsync(RequiredUserId));
        }

        // PATCH /api/driver/online   { "isOnline": true }
        [HttpPatch("driver/online")]
        public async Task<ActionResult<DriverProfileDto>> SetOnline(SetOnlineRequest request)
        {
            return Ok(await _deliveryService.SetOnlineAsync(RequiredUserId, request.IsOnline!.Value));
        }

        // PATCH /api/driver/location   { "latitude": 33.89, "longitude": 35.50 }
        [HttpPatch("driver/location")]
        public async Task<ActionResult<DriverProfileDto>> UpdateLocation(UpdateLocationRequest request)
        {
            return Ok(await _deliveryService.UpdateLocationAsync(RequiredUserId, request.Latitude!.Value, request.Longitude!.Value));
        }

        // ---------- Deliveries ----------

        // GET /api/deliveries/available?page=1
        [HttpGet("deliveries/available")]
        public async Task<ActionResult<PagedResult<AvailableDeliveryDto>>> GetAvailable([FromQuery] PageQuery query)
        {
            return Ok(await _deliveryService.GetAvailableAsync(RequiredUserId, query));
        }

        // POST /api/deliveries/accept/15   (15 = order id)
        [HttpPost("deliveries/accept/{orderId:int}")]
        public async Task<ActionResult<DeliveryDto>> Accept(int orderId)
        {
            return Ok(await _deliveryService.AcceptAsync(orderId, RequiredUserId));
        }

        // GET /api/deliveries/current   (204 No Content when the driver has no active job)
        [HttpGet("deliveries/current")]
        public async Task<ActionResult<DeliveryDto>> GetCurrent()
        {
            var delivery = await _deliveryService.GetCurrentAsync(RequiredUserId);
            return delivery == null ? NoContent() : Ok(delivery);
        }

        // POST /api/deliveries/3/release   (give the job back before pickup)
        [HttpPost("deliveries/{id:int}/release")]
        public async Task<IActionResult> Release(int id)
        {
            await _deliveryService.ReleaseAsync(id, RequiredUserId);
            return NoContent();
        }

        // POST /api/deliveries/3/pickup
        [HttpPost("deliveries/{id:int}/pickup")]
        public async Task<ActionResult<DeliveryDto>> PickUp(int id)
        {
            return Ok(await _deliveryService.PickUpAsync(id, RequiredUserId));
        }

        // POST /api/deliveries/3/deliver
        [HttpPost("deliveries/{id:int}/deliver")]
        public async Task<ActionResult<DeliveryDto>> Deliver(int id)
        {
            return Ok(await _deliveryService.DeliverAsync(id, RequiredUserId));
        }

        // GET /api/deliveries/mine?status=Delivered&page=1
        [HttpGet("deliveries/mine")]
        public async Task<ActionResult<PagedResult<DeliveryDto>>> GetMine([FromQuery] DeliveryQuery query)
        {
            return Ok(await _deliveryService.GetMyDeliveriesAsync(RequiredUserId, query));
        }

        // GET /api/deliveries/earnings
        [HttpGet("deliveries/earnings")]
        public async Task<ActionResult<EarningsDto>> GetEarnings()
        {
            return Ok(await _deliveryService.GetEarningsAsync(RequiredUserId));
        }
    }
}
