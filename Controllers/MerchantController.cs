using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CedarDelivery.Models;
using CedarDelivery.Repositories.Interfaces;
using CedarDelivery.Services;
using CedarDelivery.Services.Interfaces;
using CedarDelivery.ViewModels;
using CedarDelivery.ViewModels.Api;

namespace CedarDelivery.Controllers
{
    // Merchant pages: my stores, store form, menu management, store orders.
    // Ownership checks happen in the services; this controller only builds pages.
    [Authorize(Policy = "MerchantOrAdmin")]
    public class MerchantController : BaseController
    {
        private readonly IStoreService _storeService;
        private readonly IProductService _productService;
        private readonly IOrderService _orderService;

        public MerchantController(UserManager<User> userManager, IUserRepository userRepository,
            IStoreService storeService, IProductService productService, IOrderService orderService)
            : base(userManager, userRepository)
        {
            _storeService = storeService;
            _productService = productService;
            _orderService = orderService;
        }

        // ---------- Stores ----------

        public async Task<IActionResult> Index()
        {
            return View(await _storeService.GetMyStoresAsync(RequiredUserId));
        }

        public IActionResult Create()
        {
            return View("StoreForm", new StoreFormViewModel
            {
                Store = new StoreRequest { OpensAt = new TimeOnly(9, 0), ClosesAt = new TimeOnly(23, 0), DeliveryFee = 2 }
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create([Bind(Prefix = "Store")] StoreRequest store)
        {
            if (!ModelState.IsValid)
            {
                return View("StoreForm", new StoreFormViewModel { Store = store });
            }

            var created = await _storeService.CreateStoreAsync(RequiredUserId, store);
            TempData["Success"] = "Store created. Add your menu while an admin reviews it.";
            return RedirectToAction(nameof(Menu), new { id = created.Id });
        }

        public async Task<IActionResult> Edit(int id)
        {
            var store = await GetMyStoreAsync(id);
            return View("StoreForm", new StoreFormViewModel
            {
                StoreId = id,
                Store = new StoreRequest
                {
                    Name = store.Name,
                    Description = store.Description,
                    Category = store.Category,
                    LogoUrl = store.LogoUrl,
                    Address = store.Address,
                    Latitude = store.Latitude,
                    Longitude = store.Longitude,
                    Phone = store.Phone,
                    OpensAt = store.OpensAt,
                    ClosesAt = store.ClosesAt,
                    DeliveryFee = store.DeliveryFee,
                    MinimumOrder = store.MinimumOrder
                }
            });
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, [Bind(Prefix = "Store")] StoreRequest store)
        {
            if (!ModelState.IsValid)
            {
                return View("StoreForm", new StoreFormViewModel { StoreId = id, Store = store });
            }

            await _storeService.UpdateStoreAsync(id, RequiredUserId, IsAdmin, store);
            TempData["Success"] = "Store saved.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> SetOpen(int id, bool isOpen)
        {
            await TryAsync(() => _storeService.SetOpenAsync(id, RequiredUserId, IsAdmin, isOpen),
                isOpen ? "Store is open for orders." : "Store is closed.");
            return RedirectToAction(nameof(Index));
        }

        // ---------- Menu ----------

        public async Task<IActionResult> Menu(int id)
        {
            return View(await BuildMenuPageAsync(id));
        }

        [HttpPost]
        public async Task<IActionResult> AddCategory(int id, [Bind(Prefix = "NewCategory")] CategoryRequest newCategory)
        {
            if (!ModelState.IsValid)
            {
                var page = await BuildMenuPageAsync(id);
                page.NewCategory = newCategory;
                return View(nameof(Menu), page);
            }

            await TryAsync(() => _productService.CreateCategoryAsync(id, RequiredUserId, IsAdmin, newCategory), "Category added.");
            return RedirectToAction(nameof(Menu), new { id });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCategory(int categoryId, int id)
        {
            await TryAsync(() => _productService.DeleteCategoryAsync(categoryId, RequiredUserId, IsAdmin), "Category deleted.");
            return RedirectToAction(nameof(Menu), new { id });
        }

        [HttpPost]
        public async Task<IActionResult> AddProduct(int id, [Bind(Prefix = "NewProduct")] ProductRequest newProduct)
        {
            if (!ModelState.IsValid)
            {
                var page = await BuildMenuPageAsync(id);
                page.NewProduct = newProduct;
                return View(nameof(Menu), page);
            }

            await TryAsync(() => _productService.CreateProductAsync(id, RequiredUserId, IsAdmin, newProduct), "Product added.");
            return RedirectToAction(nameof(Menu), new { id });
        }

        public async Task<IActionResult> EditProduct(int productId, int id)
        {
            var products = await _productService.GetStoreProductsAsync(id, RequiredUserId, IsAdmin);
            var product = products.FirstOrDefault(p => p.Id == productId)
                ?? throw new NotFoundException("Product not found.");

            return View(new ProductFormViewModel
            {
                ProductId = productId,
                StoreId = id,
                Categories = await _productService.GetCategoriesAsync(id, RequiredUserId, IsAdmin),
                Product = new ProductRequest
                {
                    ProductCategoryId = product.ProductCategoryId,
                    Name = product.Name,
                    Description = product.Description,
                    Price = product.Price,
                    ImageUrl = product.ImageUrl,
                    IsAvailable = product.IsAvailable
                }
            });
        }

        [HttpPost]
        public async Task<IActionResult> EditProduct(int productId, int id, [Bind(Prefix = "Product")] ProductRequest product)
        {
            if (!ModelState.IsValid)
            {
                return View(new ProductFormViewModel
                {
                    ProductId = productId,
                    StoreId = id,
                    Categories = await _productService.GetCategoriesAsync(id, RequiredUserId, IsAdmin),
                    Product = product
                });
            }

            if (await TryAsync(() => _productService.UpdateProductAsync(productId, RequiredUserId, IsAdmin, product), "Product saved."))
            {
                return RedirectToAction(nameof(Menu), new { id });
            }
            return RedirectToAction(nameof(EditProduct), new { productId, id });
        }

        [HttpPost]
        public async Task<IActionResult> SetAvailability(int productId, int id, bool isAvailable)
        {
            await TryAsync(() => _productService.SetAvailabilityAsync(productId, RequiredUserId, IsAdmin, isAvailable));
            return RedirectToAction(nameof(Menu), new { id });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteProduct(int productId, int id)
        {
            await TryAsync(() => _productService.DeleteProductAsync(productId, RequiredUserId, IsAdmin), "Product deleted.");
            return RedirectToAction(nameof(Menu), new { id });
        }

        // ---------- Orders ----------

        // GET /Merchant/Orders/5?status=Pending
        public async Task<IActionResult> Orders(int id, [FromQuery] OrderQuery query)
        {
            if (!ModelState.IsValid)
            {
                query = new OrderQuery();
            }
            query.PageSize = 20;

            return View(new StoreOrdersViewModel
            {
                Store = await GetMyStoreAsync(id),
                Query = query,
                Orders = await _orderService.GetStoreOrdersAsync(id, RequiredUserId, IsAdmin, query)
            });
        }

        // ---------- Helpers ----------

        // The merchant's own store (admins may open any store).
        private async Task<StoreDto> GetMyStoreAsync(int id)
        {
            if (IsAdmin)
            {
                return await _storeService.GetStoreAsync(id, RequiredUserId, true);
            }

            var mine = await _storeService.GetMyStoresAsync(RequiredUserId);
            return mine.FirstOrDefault(s => s.Id == id)
                ?? throw new NotFoundException("Store not found.");
        }

        private async Task<MenuManageViewModel> BuildMenuPageAsync(int id)
        {
            return new MenuManageViewModel
            {
                Store = await GetMyStoreAsync(id),
                Categories = await _productService.GetCategoriesAsync(id, RequiredUserId, IsAdmin),
                Products = await _productService.GetStoreProductsAsync(id, RequiredUserId, IsAdmin)
            };
        }
    }
}
