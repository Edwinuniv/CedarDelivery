using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CedarDelivery.Services.Interfaces;
using CedarDelivery.ViewModels.Api;

namespace CedarDelivery.ApiControllers
{
    // /api/stores: browse stores (public), manage your stores (Merchant), approve stores (Admin).
    // Controllers stay thin: the rules live in StoreService.
    [Route("api/stores")]
    public class StoresApiController : BaseApiController
    {
        private readonly IStoreService _storeService;

        public StoresApiController(IStoreService storeService)
        {
            _storeService = storeService;
        }

        // GET /api/stores?category=Restaurant&search=pizza&page=1&pageSize=10
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<PagedResult<StoreDto>>> GetStores([FromQuery] StoreQuery query)
        {
            return Ok(await _storeService.GetStoresAsync(query));
        }

        // GET /api/stores/5
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<StoreDto>> GetStore(int id)
        {
            return Ok(await _storeService.GetStoreAsync(id, CurrentUserId, IsAdmin));
        }

        // GET /api/stores/5/menu
        [HttpGet("{id:int}/menu")]
        [AllowAnonymous]
        public async Task<ActionResult<MenuDto>> GetMenu(int id)
        {
            return Ok(await _storeService.GetMenuAsync(id, CurrentUserId, IsAdmin));
        }

        // GET /api/stores/mine
        [HttpGet("mine")]
        [Authorize(Policy = "MerchantOrAdmin")]
        public async Task<ActionResult<List<StoreDto>>> GetMyStores()
        {
            return Ok(await _storeService.GetMyStoresAsync(RequiredUserId));
        }

        // POST /api/stores
        [HttpPost]
        [Authorize(Policy = "MerchantOrAdmin")]
        public async Task<ActionResult<StoreDto>> CreateStore(StoreRequest request)
        {
            var store = await _storeService.CreateStoreAsync(RequiredUserId, request);

            // 201 Created with a Location header pointing to GET /api/stores/{id}.
            return CreatedAtAction(nameof(GetStore), new { id = store.Id }, store);
        }

        // PUT /api/stores/5
        [HttpPut("{id:int}")]
        [Authorize(Policy = "MerchantOrAdmin")]
        public async Task<ActionResult<StoreDto>> UpdateStore(int id, StoreRequest request)
        {
            return Ok(await _storeService.UpdateStoreAsync(id, RequiredUserId, IsAdmin, request));
        }

        // PATCH /api/stores/5/open   { "isOpen": true }
        [HttpPatch("{id:int}/open")]
        [Authorize(Policy = "MerchantOrAdmin")]
        public async Task<ActionResult<StoreDto>> SetOpen(int id, SetStoreOpenRequest request)
        {
            return Ok(await _storeService.SetOpenAsync(id, RequiredUserId, IsAdmin, request.IsOpen!.Value));
        }

        // GET /api/stores/pending
        [HttpGet("pending")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<List<StoreDto>>> GetPendingStores()
        {
            return Ok(await _storeService.GetPendingStoresAsync());
        }

        // PATCH /api/stores/5/approval   { "isApproved": true }
        [HttpPatch("{id:int}/approval")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<StoreDto>> SetApproval(int id, SetStoreApprovalRequest request)
        {
            return Ok(await _storeService.SetApprovalAsync(id, RequiredUserId, request.IsApproved!.Value));
        }
    }
}
