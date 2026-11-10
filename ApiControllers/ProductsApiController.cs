using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CedarDelivery.Services.Interfaces;
using CedarDelivery.ViewModels.Api;

namespace CedarDelivery.ApiControllers
{
    // Menu management for merchants: categories and products of their stores.
    // Customers read the menu from GET /api/stores/{id}/menu instead.
    [Route("api")]
    [Authorize(Policy = "MerchantOrAdmin")]
    public class ProductsApiController : BaseApiController
    {
        private readonly IProductService _productService;

        public ProductsApiController(IProductService productService)
        {
            _productService = productService;
        }

        // ---------- Categories ----------

        // GET /api/stores/5/categories
        [HttpGet("stores/{storeId:int}/categories")]
        public async Task<ActionResult<List<CategoryDto>>> GetCategories(int storeId)
        {
            return Ok(await _productService.GetCategoriesAsync(storeId, RequiredUserId, IsAdmin));
        }

        // POST /api/stores/5/categories
        [HttpPost("stores/{storeId:int}/categories")]
        public async Task<ActionResult<CategoryDto>> CreateCategory(int storeId, CategoryRequest request)
        {
            var category = await _productService.CreateCategoryAsync(storeId, RequiredUserId, IsAdmin, request);
            return StatusCode(StatusCodes.Status201Created, category);
        }

        // PUT /api/categories/3
        [HttpPut("categories/{id:int}")]
        public async Task<ActionResult<CategoryDto>> UpdateCategory(int id, CategoryRequest request)
        {
            return Ok(await _productService.UpdateCategoryAsync(id, RequiredUserId, IsAdmin, request));
        }

        // DELETE /api/categories/3
        [HttpDelete("categories/{id:int}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            await _productService.DeleteCategoryAsync(id, RequiredUserId, IsAdmin);
            return NoContent();
        }

        // ---------- Products ----------

        // GET /api/stores/5/products   (all products, including unavailable ones)
        [HttpGet("stores/{storeId:int}/products")]
        public async Task<ActionResult<List<ProductDto>>> GetProducts(int storeId)
        {
            return Ok(await _productService.GetStoreProductsAsync(storeId, RequiredUserId, IsAdmin));
        }

        // POST /api/stores/5/products
        [HttpPost("stores/{storeId:int}/products")]
        public async Task<ActionResult<ProductDto>> CreateProduct(int storeId, ProductRequest request)
        {
            var product = await _productService.CreateProductAsync(storeId, RequiredUserId, IsAdmin, request);
            return StatusCode(StatusCodes.Status201Created, product);
        }

        // PUT /api/products/12
        [HttpPut("products/{id:int}")]
        public async Task<ActionResult<ProductDto>> UpdateProduct(int id, ProductRequest request)
        {
            return Ok(await _productService.UpdateProductAsync(id, RequiredUserId, IsAdmin, request));
        }

        // PATCH /api/products/12/availability   { "isAvailable": false }
        [HttpPatch("products/{id:int}/availability")]
        public async Task<ActionResult<ProductDto>> SetAvailability(int id, SetProductAvailabilityRequest request)
        {
            return Ok(await _productService.SetAvailabilityAsync(id, RequiredUserId, IsAdmin, request.IsAvailable!.Value));
        }

        // DELETE /api/products/12
        [HttpDelete("products/{id:int}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            await _productService.DeleteProductAsync(id, RequiredUserId, IsAdmin);
            return NoContent();
        }
    }
}
