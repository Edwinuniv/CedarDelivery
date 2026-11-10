using CedarDelivery.Models;
using CedarDelivery.Repositories.Interfaces;
using CedarDelivery.Services.Interfaces;
using CedarDelivery.ViewModels.Api;

namespace CedarDelivery.Services.Implementations
{
    public class StoreService : IStoreService
    {
        private readonly IStoreRepository _storeRepository;
        private readonly IReviewRepository _reviewRepository;
        private readonly IActivityLogService _activityLog;

        public StoreService(IStoreRepository storeRepository, IReviewRepository reviewRepository, IActivityLogService activityLog)
        {
            _storeRepository = storeRepository;
            _reviewRepository = reviewRepository;
            _activityLog = activityLog;
        }

        public async Task<PagedResult<StoreDto>> GetStoresAsync(StoreQuery query)
        {
            var (stores, totalCount) = await _storeRepository.GetApprovedPagedAsync(
                query.Category, query.Search, query.Page, query.PageSize);

            return new PagedResult<StoreDto>
            {
                Items = await WithRatingsAsync(stores.Select(StoreDto.FromEntity).ToList()),
                Page = query.Page,
                PageSize = query.PageSize,
                TotalCount = totalCount
            };
        }

        public async Task<StoreDto> GetStoreAsync(int id, string? userId, bool isAdmin)
        {
            var store = await _storeRepository.GetByIdAsync(id);

            if (store == null || !CanView(store, userId, isAdmin))
            {
                throw new NotFoundException("Store not found.");
            }

            return (await WithRatingsAsync(new List<StoreDto> { StoreDto.FromEntity(store) }))[0];
        }

        public async Task<MenuDto> GetMenuAsync(int id, string? userId, bool isAdmin)
        {
            var store = await _storeRepository.GetWithMenuAsync(id);

            if (store == null || !CanView(store, userId, isAdmin))
            {
                throw new NotFoundException("Store not found.");
            }

            // Customers only see available products, and categories that have some.
            var categories = store.ProductCategories
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.Name)
                .Select(c => new MenuCategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Products = c.Products
                        .Where(p => p.IsAvailable)
                        .OrderBy(p => p.Name)
                        .Select(ProductDto.FromEntity)
                        .ToList()
                })
                .Where(c => c.Products.Count > 0)
                .ToList();

            return new MenuDto
            {
                Store = (await WithRatingsAsync(new List<StoreDto> { StoreDto.FromEntity(store) }))[0],
                Categories = categories
            };
        }

        public async Task<List<StoreDto>> GetMyStoresAsync(string userId)
        {
            var stores = await _storeRepository.GetByOwnerAsync(userId);
            return await WithRatingsAsync(stores.Select(StoreDto.FromEntity).ToList());
        }

        public async Task<StoreDto> CreateStoreAsync(string userId, StoreRequest request)
        {
            var store = new Store
            {
                OwnerId = userId,
                // New stores wait for an Admin, and start closed until the merchant opens them.
                IsApproved = false,
                IsOpen = false,
                CreatedAt = DateTime.Now
            };
            ApplyRequest(store, request);

            await _storeRepository.AddAsync(store);
            await _activityLog.LogAsync(userId, ActivityType.StoreUpdated, $"Created store \"{store.Name}\" (#{store.Id}).");

            return StoreDto.FromEntity(store);
        }

        public async Task<StoreDto> UpdateStoreAsync(int id, string userId, bool isAdmin, StoreRequest request)
        {
            var store = await GetStoreToManageAsync(id, userId, isAdmin);

            ApplyRequest(store, request);
            await _storeRepository.UpdateAsync(store);
            await _activityLog.LogAsync(userId, ActivityType.StoreUpdated, $"Updated store \"{store.Name}\" (#{store.Id}).");

            return StoreDto.FromEntity(store);
        }

        public async Task<StoreDto> SetOpenAsync(int id, string userId, bool isAdmin, bool isOpen)
        {
            var store = await GetStoreToManageAsync(id, userId, isAdmin);

            if (isOpen && !store.IsApproved)
            {
                throw new BadRequestException("The store can open after an admin approves it.");
            }

            store.IsOpen = isOpen;
            await _storeRepository.UpdateAsync(store);
            await _activityLog.LogAsync(userId, ActivityType.StoreUpdated,
                $"{(isOpen ? "Opened" : "Closed")} store \"{store.Name}\" (#{store.Id}).");

            return StoreDto.FromEntity(store);
        }

        public async Task<List<StoreDto>> GetPendingStoresAsync()
        {
            var stores = await _storeRepository.GetPendingAsync();
            return stores.Select(StoreDto.FromEntity).ToList();
        }

        public async Task<StoreDto> SetApprovalAsync(int id, string adminId, bool isApproved)
        {
            var store = await _storeRepository.GetByIdAsync(id)
                ?? throw new NotFoundException("Store not found.");

            store.IsApproved = isApproved;

            // A suspended store must also stop taking orders.
            if (!isApproved)
            {
                store.IsOpen = false;
            }

            await _storeRepository.UpdateAsync(store);
            await _activityLog.LogAsync(adminId, ActivityType.StoreUpdated,
                $"{(isApproved ? "Approved" : "Unapproved")} store \"{store.Name}\" (#{store.Id}).");

            return StoreDto.FromEntity(store);
        }

        // ---------- Helpers ----------

        // Adds average rating and review count, using one query for all the stores.
        private async Task<List<StoreDto>> WithRatingsAsync(List<StoreDto> stores)
        {
            var ratings = await _reviewRepository.GetStoreRatingsAsync(stores.Select(s => s.Id));

            foreach (var store in stores)
            {
                if (ratings.TryGetValue(store.Id, out var rating))
                {
                    store.AverageRating = rating.Average;
                    store.ReviewCount = rating.Count;
                }
            }

            return stores;
        }

        // Approved stores are public. Unapproved ones are visible only to their owner and admins.
        private static bool CanView(Store store, string? userId, bool isAdmin)
        {
            return store.IsApproved || isAdmin || (userId != null && store.OwnerId == userId);
        }

        // Loads a store and checks the user owns it (admins can manage any store).
        private async Task<Store> GetStoreToManageAsync(int id, string userId, bool isAdmin)
        {
            var store = await _storeRepository.GetByIdAsync(id)
                ?? throw new NotFoundException("Store not found.");

            if (store.OwnerId != userId && !isAdmin)
            {
                throw new ForbiddenException("You can only manage your own stores.");
            }

            return store;
        }

        // Copies the editable fields from the request. [Required] already guarantees
        // Category, OpensAt and ClosesAt are present, so .Value is safe here.
        private static void ApplyRequest(Store store, StoreRequest request)
        {
            store.Name = request.Name.Trim();
            store.Description = request.Description?.Trim();
            store.Category = request.Category!.Value;
            store.LogoUrl = request.LogoUrl;
            store.Address = request.Address.Trim();
            store.Latitude = request.Latitude;
            store.Longitude = request.Longitude;
            store.Phone = request.Phone;
            store.OpensAt = request.OpensAt!.Value;
            store.ClosesAt = request.ClosesAt!.Value;
            store.DeliveryFee = request.DeliveryFee;
            store.MinimumOrder = request.MinimumOrder;
        }
    }
}
