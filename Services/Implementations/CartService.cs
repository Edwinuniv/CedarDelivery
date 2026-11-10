using CedarDelivery.Models;
using CedarDelivery.Repositories.Interfaces;
using CedarDelivery.Services.Interfaces;
using CedarDelivery.ViewModels.Api;

namespace CedarDelivery.Services.Implementations
{
    public class CartService : ICartService
    {
        private const int MaxQuantity = 50;

        private readonly ICartItemRepository _cartItemRepository;
        private readonly IProductRepository _productRepository;
        private readonly IStoreRepository _storeRepository;

        public CartService(ICartItemRepository cartItemRepository, IProductRepository productRepository, IStoreRepository storeRepository)
        {
            _cartItemRepository = cartItemRepository;
            _productRepository = productRepository;
            _storeRepository = storeRepository;
        }

        public async Task<CartDto> GetCartAsync(string userId)
        {
            var items = await _cartItemRepository.GetByUserAsync(userId);
            return BuildCart(items, DateTime.Now);
        }

        public async Task<CartDto> AddItemAsync(string userId, AddCartItemRequest request)
        {
            var product = await _productRepository.GetByIdAsync(request.ProductId!.Value);
            var store = product == null ? null : await _storeRepository.GetByIdAsync(product.StoreId);

            if (product == null || store == null || !store.IsApproved)
            {
                throw new NotFoundException("Product not found.");
            }

            if (!product.IsAvailable)
            {
                throw new BadRequestException($"{product.Name} is not available right now.");
            }

            // Rule: one store per cart (one order = one store = one driver trip).
            var cart = await _cartItemRepository.GetByUserAsync(userId);
            if (cart.Any(c => c.Product.StoreId != product.StoreId))
            {
                throw new BadRequestException("Your cart has items from another store. Empty it first to order from this store.");
            }

            var existing = cart.FirstOrDefault(c => c.ProductId == product.Id);
            if (existing != null)
            {
                if (existing.Quantity + request.Quantity > MaxQuantity)
                {
                    throw new BadRequestException($"You can add up to {MaxQuantity} of one product.");
                }
                existing.Quantity += request.Quantity;
                await _cartItemRepository.SaveAsync();
            }
            else
            {
                await _cartItemRepository.AddAsync(new CartItem
                {
                    UserId = userId,
                    ProductId = product.Id,
                    Quantity = request.Quantity,
                    AddedAt = DateTime.Now
                });
            }

            return await GetCartAsync(userId);
        }

        public async Task<CartDto> UpdateItemAsync(int itemId, string userId, int quantity)
        {
            var item = await GetOwnItemAsync(itemId, userId);

            if (quantity == 0)
            {
                await _cartItemRepository.DeleteAsync(item.Id);
            }
            else
            {
                item.Quantity = quantity;
                await _cartItemRepository.SaveAsync();
            }

            return await GetCartAsync(userId);
        }

        public async Task<CartDto> RemoveItemAsync(int itemId, string userId)
        {
            var item = await GetOwnItemAsync(itemId, userId);
            await _cartItemRepository.DeleteAsync(item.Id);
            return await GetCartAsync(userId);
        }

        public async Task<CartDto> ClearAsync(string userId)
        {
            await _cartItemRepository.ClearAsync(userId);
            return new CartDto();
        }

        // Works out prices and warnings from cart items loaded with Product.Store.
        // Also used by OrderService at checkout, so the cart page and checkout
        // always apply the same rules.
        public static CartDto BuildCart(List<CartItem> items, DateTime now)
        {
            var cart = new CartDto();
            if (items.Count == 0)
            {
                return cart;
            }

            var store = items[0].Product.Store;
            cart.StoreId = store.Id;
            cart.StoreName = store.Name;
            cart.MinimumOrder = store.MinimumOrder;

            cart.Items = items.Select(c => new CartItemDto
            {
                Id = c.Id,
                ProductId = c.ProductId,
                ProductName = c.Product.Name,
                ImageUrl = c.Product.ImageUrl,
                UnitPrice = c.Product.Price,
                Quantity = c.Quantity,
                LineTotal = c.Product.Price * c.Quantity,
                IsAvailable = c.Product.IsAvailable
            }).ToList();

            cart.Subtotal = cart.Items.Sum(i => i.LineTotal);
            cart.DeliveryFee = store.DeliveryFee;
            cart.Total = cart.Subtotal + cart.DeliveryFee;

            foreach (var item in cart.Items.Where(i => !i.IsAvailable))
            {
                cart.Warnings.Add($"{item.ProductName} is no longer available. Remove it to continue.");
            }

            if (!store.IsAcceptingOrders(now))
            {
                cart.Warnings.Add($"{store.Name} is closed right now.");
            }

            if (cart.Subtotal < store.MinimumOrder)
            {
                cart.Warnings.Add($"The minimum order at {store.Name} is ${store.MinimumOrder:0.00}.");
            }

            return cart;
        }

        private async Task<CartItem> GetOwnItemAsync(int itemId, string userId)
        {
            var item = await _cartItemRepository.GetByIdAsync(itemId);

            if (item == null || item.UserId != userId)
            {
                throw new NotFoundException("Cart item not found.");
            }

            return item;
        }
    }
}
