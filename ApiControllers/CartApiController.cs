using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CedarDelivery.Services.Interfaces;
using CedarDelivery.ViewModels.Api;

namespace CedarDelivery.ApiControllers
{
    // /api/cart: the signed-in user's cart. Every call returns the whole updated cart.
    [Route("api/cart")]
    [Authorize]
    public class CartApiController : BaseApiController
    {
        private readonly ICartService _cartService;

        public CartApiController(ICartService cartService)
        {
            _cartService = cartService;
        }

        // GET /api/cart
        [HttpGet]
        public async Task<ActionResult<CartDto>> GetCart()
        {
            return Ok(await _cartService.GetCartAsync(RequiredUserId));
        }

        // POST /api/cart/items   { "productId": 1, "quantity": 2 }
        [HttpPost("items")]
        public async Task<ActionResult<CartDto>> AddItem(AddCartItemRequest request)
        {
            return Ok(await _cartService.AddItemAsync(RequiredUserId, request));
        }

        // PUT /api/cart/items/4   { "quantity": 3 }   (0 removes it)
        [HttpPut("items/{id:int}")]
        public async Task<ActionResult<CartDto>> UpdateItem(int id, UpdateCartItemRequest request)
        {
            return Ok(await _cartService.UpdateItemAsync(id, RequiredUserId, request.Quantity));
        }

        // DELETE /api/cart/items/4
        [HttpDelete("items/{id:int}")]
        public async Task<ActionResult<CartDto>> RemoveItem(int id)
        {
            return Ok(await _cartService.RemoveItemAsync(id, RequiredUserId));
        }

        // DELETE /api/cart
        [HttpDelete]
        public async Task<ActionResult<CartDto>> Clear()
        {
            return Ok(await _cartService.ClearAsync(RequiredUserId));
        }
    }
}
