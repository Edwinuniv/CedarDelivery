using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CedarDelivery.Services.Interfaces;
using CedarDelivery.ViewModels.Api;

namespace CedarDelivery.ApiControllers
{
    // Ratings for stores and drivers, left by customers after delivery.
    [Route("api")]
    [Authorize]
    public class ReviewsApiController : BaseApiController
    {
        private readonly IReviewService _reviewService;

        public ReviewsApiController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        // POST /api/reviews   { "orderId": 15, "targetType": "Store", "rating": 5, "comment": "Great!" }
        [HttpPost("reviews")]
        public async Task<ActionResult<ReviewDto>> Create(CreateReviewRequest request)
        {
            var review = await _reviewService.CreateAsync(RequiredUserId, request);
            return StatusCode(StatusCodes.Status201Created, review);
        }

        // GET /api/reviews/mine
        [HttpGet("reviews/mine")]
        public async Task<ActionResult<List<ReviewDto>>> GetMine()
        {
            return Ok(await _reviewService.GetMineAsync(RequiredUserId));
        }

        // GET /api/stores/5/reviews?page=1   (public)
        [HttpGet("stores/{storeId:int}/reviews")]
        [AllowAnonymous]
        public async Task<ActionResult<ReviewListDto>> GetStoreReviews(int storeId, [FromQuery] PageQuery query)
        {
            return Ok(await _reviewService.GetStoreReviewsAsync(storeId, query));
        }

        // GET /api/driver/reviews   (the signed-in driver's own reviews)
        [HttpGet("driver/reviews")]
        [Authorize(Policy = "DriverOrAdmin")]
        public async Task<ActionResult<ReviewListDto>> GetDriverReviews([FromQuery] PageQuery query)
        {
            return Ok(await _reviewService.GetDriverReviewsAsync(RequiredUserId, query));
        }
    }
}
