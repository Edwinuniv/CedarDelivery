using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CedarDelivery.Services.Interfaces;
using CedarDelivery.ViewModels.Api;

namespace CedarDelivery.ApiControllers
{
    // Orders for customers, store owners and admins.
    // Drivers move orders through PickedUp/Delivered in the deliveries API.
    [Route("api")]
    [Authorize]
    public class OrdersApiController : BaseApiController
    {
        private readonly IOrderService _orderService;

        public OrdersApiController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // POST /api/orders   (checkout: turns the cart into an order)
        [HttpPost("orders")]
        public async Task<ActionResult<OrderDetailsDto>> PlaceOrder(PlaceOrderRequest request)
        {
            var order = await _orderService.PlaceOrderAsync(RequiredUserId, request);
            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
        }

        // GET /api/orders/mine?page=1
        [HttpGet("orders/mine")]
        public async Task<ActionResult<PagedResult<OrderSummaryDto>>> GetMyOrders([FromQuery] PageQuery query)
        {
            return Ok(await _orderService.GetMyOrdersAsync(RequiredUserId, query));
        }

        // GET /api/orders/15
        [HttpGet("orders/{id:int}")]
        public async Task<ActionResult<OrderDetailsDto>> GetOrder(int id)
        {
            return Ok(await _orderService.GetOrderAsync(id, RequiredUserId, IsAdmin));
        }

        // POST /api/orders/15/cancel   { "reason": "Ordered by mistake" }
        [HttpPost("orders/{id:int}/cancel")]
        public async Task<ActionResult<OrderDetailsDto>> Cancel(int id, CancelOrderRequest request)
        {
            return Ok(await _orderService.CancelAsync(id, RequiredUserId, IsAdmin, request.Reason));
        }

        // GET /api/stores/5/orders?status=Pending&page=1
        [HttpGet("stores/{storeId:int}/orders")]
        [Authorize(Policy = "MerchantOrAdmin")]
        public async Task<ActionResult<PagedResult<OrderSummaryDto>>> GetStoreOrders(int storeId, [FromQuery] OrderQuery query)
        {
            return Ok(await _orderService.GetStoreOrdersAsync(storeId, RequiredUserId, IsAdmin, query));
        }

        // PATCH /api/orders/15/status   { "status": "Accepted", "note": "Ready in 20 min" }
        [HttpPatch("orders/{id:int}/status")]
        [Authorize(Policy = "MerchantOrAdmin")]
        public async Task<ActionResult<OrderDetailsDto>> UpdateStatus(int id, UpdateOrderStatusRequest request)
        {
            return Ok(await _orderService.UpdateStatusByStoreAsync(id, RequiredUserId, IsAdmin, request.Status!.Value, request.Note));
        }

        // GET /api/orders?status=Delivered&storeId=5&page=1   (all orders)
        [HttpGet("orders")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<PagedResult<OrderSummaryDto>>> GetAllOrders([FromQuery] OrderQuery query)
        {
            return Ok(await _orderService.GetAllOrdersAsync(query));
        }
    }
}
