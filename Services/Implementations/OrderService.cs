using System.Security.Cryptography;
using CedarDelivery.Models;
using CedarDelivery.Repositories.Interfaces;
using CedarDelivery.Services.Interfaces;
using CedarDelivery.ViewModels.Api;

namespace CedarDelivery.Services.Implementations
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICartItemRepository _cartItemRepository;
        private readonly IAddressRepository _addressRepository;
        private readonly IStoreRepository _storeRepository;
        private readonly IDriverProfileRepository _driverProfileRepository;
        private readonly INotificationService _notifications;
        private readonly IRealtimeService _realtime;
        private readonly IActivityLogService _activityLog;

        public OrderService(
            IOrderRepository orderRepository,
            ICartItemRepository cartItemRepository,
            IAddressRepository addressRepository,
            IStoreRepository storeRepository,
            IDriverProfileRepository driverProfileRepository,
            INotificationService notifications,
            IRealtimeService realtime,
            IActivityLogService activityLog)
        {
            _orderRepository = orderRepository;
            _cartItemRepository = cartItemRepository;
            _addressRepository = addressRepository;
            _storeRepository = storeRepository;
            _driverProfileRepository = driverProfileRepository;
            _notifications = notifications;
            _realtime = realtime;
            _activityLog = activityLog;
        }

        // ---------- Customer ----------

        public async Task<OrderDetailsDto> PlaceOrderAsync(string userId, PlaceOrderRequest request)
        {
            // MVP: cash only (see REQUIREMENTS.md). Card payment is an Extra for later.
            if (request.PaymentMethod != PaymentMethod.CashOnDelivery)
            {
                throw new BadRequestException("Card payment is coming soon. Please choose cash on delivery.");
            }

            var cartItems = await _cartItemRepository.GetByUserAsync(userId);
            if (cartItems.Count == 0)
            {
                throw new BadRequestException("Your cart is empty.");
            }

            // Same checks as the cart page: available items, store open, minimum order.
            var now = DateTime.Now;
            var cart = CartService.BuildCart(cartItems, now);
            if (!cart.CanCheckout)
            {
                throw new BadRequestException(string.Join(" ", cart.Warnings));
            }

            var address = await _addressRepository.GetByIdAsync(request.AddressId!.Value);
            if (address == null || address.UserId != userId)
            {
                throw new BadRequestException("Choose one of your saved addresses.");
            }

            var store = cartItems[0].Product.Store;

            var order = new Order
            {
                OrderNumber = await GenerateOrderNumberAsync(now),
                CustomerId = userId,
                StoreId = store.Id,
                Status = OrderStatus.Pending,
                Subtotal = cart.Subtotal,
                DeliveryFee = cart.DeliveryFee,
                Total = cart.Total,
                PaymentMethod = PaymentMethod.CashOnDelivery,
                PaymentStatus = PaymentStatus.Unpaid,
                Notes = request.Notes?.Trim(),
                // Copy the address as text so later edits to the saved address don't change this order.
                DeliveryAddress = BuildAddressText(address),
                DeliveryLatitude = address.Latitude,
                DeliveryLongitude = address.Longitude,
                CreatedAt = now,
                // Copy name and price: the order keeps them even if the product changes later.
                Items = cartItems.Select(c => new OrderItem
                {
                    ProductId = c.ProductId,
                    ProductName = c.Product.Name,
                    UnitPrice = c.Product.Price,
                    Quantity = c.Quantity
                }).ToList(),
                StatusHistory = new List<OrderStatusHistory>
                {
                    new OrderStatusHistory
                    {
                        Status = OrderStatus.Pending,
                        ChangedAt = now,
                        ChangedByUserId = userId,
                        Note = "Order placed."
                    }
                }
            };

            // Order saved and cart emptied together (one transaction).
            await _orderRepository.PlaceOrderAsync(order, cartItems);

            await _notifications.NotifyAsync(store.OwnerId,
                $"New order {order.OrderNumber}",
                $"{order.Items.Sum(i => i.Quantity)} item(s), total ${order.Total:0.00}.",
                NotificationType.NewOrder,
                OrderUrl(order.Id),
                sendEmail: true);
            await _notifications.NotifyAsync(userId,
                $"Order {order.OrderNumber} received",
                $"{order.Items.Sum(i => i.Quantity)} item(s) from {store.Name}, total ${order.Total:0.00}, paid in cash on delivery. We'll let you know when the store accepts it.",
                NotificationType.OrderStatusChanged,
                OrderUrl(order.Id),
                sendEmail: true);
            await _activityLog.LogAsync(userId, ActivityType.OrderPlaced, $"Placed order {order.OrderNumber} at \"{store.Name}\".");

            return await GetDetailsDtoAsync(order.Id);
        }

        public async Task<PagedResult<OrderSummaryDto>> GetMyOrdersAsync(string userId, PageQuery query)
        {
            var (orders, totalCount) = await _orderRepository.GetByCustomerPagedAsync(userId, query.Page, query.PageSize);
            return ToPagedSummaries(orders, totalCount, query);
        }

        public async Task<OrderDetailsDto> CancelAsync(int orderId, string userId, bool isAdmin, string? reason)
        {
            var order = await GetOrderEntityAsync(orderId);

            if (!isAdmin)
            {
                if (order.CustomerId != userId)
                {
                    throw new ForbiddenException("You can only cancel your own orders.");
                }
                // After the store accepts, food is being made: only an admin can cancel then.
                if (order.Status != OrderStatus.Pending)
                {
                    throw new BadRequestException("The store already accepted this order. Contact support to cancel it.");
                }
            }

            var note = string.IsNullOrWhiteSpace(reason)
                ? (isAdmin ? "Cancelled by support." : "Cancelled by the customer.")
                : reason.Trim();

            OrderStatusRules.ChangeStatus(order, OrderStatus.Cancelled, userId, note);

            // A driver may already be assigned; free them.
            if (order.Delivery != null)
            {
                order.Delivery.Status = DeliveryStatus.Cancelled;
            }

            await _orderRepository.SaveAsync();
            await _realtime.OrderChangedAsync(order.Id, order.Status);

            // Tell everyone involved except the person who cancelled.
            var message = $"Order {order.OrderNumber} was cancelled. {note}";
            if (order.CustomerId != userId)
            {
                await _notifications.NotifyAsync(order.CustomerId, "Order cancelled", message, NotificationType.OrderStatusChanged, OrderUrl(order.Id));
            }
            if (order.Store.OwnerId != userId)
            {
                await _notifications.NotifyAsync(order.Store.OwnerId, "Order cancelled", message, NotificationType.OrderStatusChanged, OrderUrl(order.Id));
            }
            if (order.Delivery != null && order.Delivery.DriverProfile.UserId != userId)
            {
                await _notifications.NotifyAsync(order.Delivery.DriverProfile.UserId, "Delivery cancelled", message, NotificationType.OrderStatusChanged, OrderUrl(order.Id));
            }

            await _activityLog.LogAsync(userId, ActivityType.OrderCancelled, $"Cancelled order {order.OrderNumber}.");

            return OrderDetailsDto.FromEntity(order);
        }

        // ---------- Shared ----------

        public async Task<OrderDetailsDto> GetOrderAsync(int orderId, string userId, bool isAdmin)
        {
            var order = await _orderRepository.GetWithDetailsAsync(orderId);

            // Not involved = "not found", so order ids can't be probed.
            if (order == null || !CanView(order, userId, isAdmin))
            {
                throw new NotFoundException("Order not found.");
            }

            return OrderDetailsDto.FromEntity(order);
        }

        // ---------- Merchant ----------

        public async Task<PagedResult<OrderSummaryDto>> GetStoreOrdersAsync(int storeId, string userId, bool isAdmin, OrderQuery query)
        {
            var store = await _storeRepository.GetByIdAsync(storeId)
                ?? throw new NotFoundException("Store not found.");

            if (store.OwnerId != userId && !isAdmin)
            {
                throw new ForbiddenException("You can only see orders of your own stores.");
            }

            var (orders, totalCount) = await _orderRepository.GetByStorePagedAsync(storeId, query.Status, query.Page, query.PageSize);
            return ToPagedSummaries(orders, totalCount, query);
        }

        public async Task<OrderDetailsDto> UpdateStatusByStoreAsync(int orderId, string userId, bool isAdmin, OrderStatus status, string? note)
        {
            if (!OrderStatusRules.MerchantStatuses.Contains(status))
            {
                throw new BadRequestException("A store can set Accepted, Rejected, Preparing or ReadyForPickup. Drivers set PickedUp and Delivered.");
            }

            var order = await GetOrderEntityAsync(orderId);

            if (order.Store.OwnerId != userId && !isAdmin)
            {
                throw new ForbiddenException("You can only manage orders of your own stores.");
            }

            OrderStatusRules.ChangeStatus(order, status, userId, note?.Trim());
            await _orderRepository.SaveAsync();
            await _realtime.OrderChangedAsync(order.Id, order.Status);

            await _notifications.NotifyAsync(order.CustomerId,
                $"Order {order.OrderNumber}: {status}",
                OrderStatusRules.CustomerMessage(status, order.Store.Name),
                NotificationType.OrderStatusChanged,
                OrderUrl(order.Id));

            // Food is ready: let every online driver know there is a job.
            if (status == OrderStatus.ReadyForPickup)
            {
                foreach (var driver in await _driverProfileRepository.GetOnlineApprovedAsync())
                {
                    await _notifications.NotifyAsync(driver.UserId,
                        "New delivery available",
                        $"{order.Store.Name} → {order.DeliveryAddress}. Earn ${order.DeliveryFee:0.00}.",
                        NotificationType.DeliveryAvailable,
                        "/Deliveries");
                }
            }

            await _activityLog.LogAsync(userId, ActivityType.OrderStatusChanged, $"Order {order.OrderNumber} -> {status}.");

            return OrderDetailsDto.FromEntity(order);
        }

        // ---------- Admin ----------

        public async Task<PagedResult<OrderSummaryDto>> GetAllOrdersAsync(OrderQuery query)
        {
            var (orders, totalCount) = await _orderRepository.GetAllPagedAsync(query.Status, query.StoreId, query.Page, query.PageSize);
            return ToPagedSummaries(orders, totalCount, query);
        }

        // ---------- Helpers ----------

        // The customer, the store owner, the assigned driver and admins can see an order.
        private static bool CanView(Order order, string userId, bool isAdmin)
        {
            return isAdmin
                || order.CustomerId == userId
                || order.Store.OwnerId == userId
                || order.Delivery?.DriverProfile.UserId == userId;
        }

        private async Task<Order> GetOrderEntityAsync(int orderId)
        {
            return await _orderRepository.GetWithDetailsAsync(orderId)
                ?? throw new NotFoundException("Order not found.");
        }

        private async Task<OrderDetailsDto> GetDetailsDtoAsync(int orderId)
        {
            return OrderDetailsDto.FromEntity(await GetOrderEntityAsync(orderId));
        }

        // e.g. "CD-20261110-K7Q2M". Random, so numbers don't reveal how many orders exist.
        // Retries in the rare case of a duplicate; the unique index is the final guard.
        private async Task<string> GenerateOrderNumberAsync(DateTime now)
        {
            const string letters = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // no 0/O or 1/I to avoid confusion

            for (var attempt = 0; attempt < 5; attempt++)
            {
                var code = new string(Enumerable.Range(0, 5)
                    .Select(_ => letters[RandomNumberGenerator.GetInt32(letters.Length)])
                    .ToArray());
                var orderNumber = $"CD-{now:yyyyMMdd}-{code}";

                if (!await _orderRepository.OrderNumberExistsAsync(orderNumber))
                {
                    return orderNumber;
                }
            }

            throw new InvalidOperationException("Could not generate a unique order number.");
        }

        // Address as one line, plus the customer's notes for the driver ("ring twice").
        private static string BuildAddressText(Address address)
        {
            var text = AddressDto.ToText(address);
            if (!string.IsNullOrWhiteSpace(address.Notes))
            {
                text += $" ({address.Notes})";
            }
            return text.Length > 500 ? text.Substring(0, 500) : text;
        }

        private static PagedResult<OrderSummaryDto> ToPagedSummaries(List<Order> orders, int totalCount, PageQuery query)
        {
            return new PagedResult<OrderSummaryDto>
            {
                Items = orders.Select(OrderSummaryDto.FromEntity).ToList(),
                Page = query.Page,
                PageSize = query.PageSize,
                TotalCount = totalCount
            };
        }

        // The order page every role opens from a notification (built in Phase 4).
        private static string OrderUrl(int orderId) => $"/Orders/Details/{orderId}";
    }
}
