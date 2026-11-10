using CedarDelivery.Models;
using CedarDelivery.Repositories.Interfaces;
using CedarDelivery.Services.Interfaces;
using CedarDelivery.ViewModels.Api;

namespace CedarDelivery.Services.Implementations
{
    public class DeliveryService : IDeliveryService
    {
        private readonly IDriverProfileRepository _driverProfileRepository;
        private readonly IDeliveryRepository _deliveryRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly INotificationService _notifications;
        private readonly IRealtimeService _realtime;
        private readonly IActivityLogService _activityLog;

        public DeliveryService(
            IDriverProfileRepository driverProfileRepository,
            IDeliveryRepository deliveryRepository,
            IOrderRepository orderRepository,
            INotificationService notifications,
            IRealtimeService realtime,
            IActivityLogService activityLog)
        {
            _driverProfileRepository = driverProfileRepository;
            _deliveryRepository = deliveryRepository;
            _orderRepository = orderRepository;
            _notifications = notifications;
            _realtime = realtime;
            _activityLog = activityLog;
        }

        // ---------- Driver profile ----------

        public async Task<DriverProfileDto> GetProfileAsync(string userId)
        {
            return DriverProfileDto.FromEntity(await GetProfileEntityAsync(userId));
        }

        public async Task<DriverProfileDto> SetOnlineAsync(string userId, bool isOnline)
        {
            var profile = await GetProfileEntityAsync(userId);

            if (isOnline && !profile.IsApproved)
            {
                throw new BadRequestException("Your driver application is still waiting for approval.");
            }

            profile.IsOnline = isOnline;
            await _driverProfileRepository.SaveAsync();
            return DriverProfileDto.FromEntity(profile);
        }

        public async Task<DriverProfileDto> UpdateLocationAsync(string userId, decimal latitude, decimal longitude)
        {
            var profile = await GetProfileEntityAsync(userId);

            profile.CurrentLatitude = latitude;
            profile.CurrentLongitude = longitude;
            profile.LocationUpdatedAt = DateTime.Now;

            await _driverProfileRepository.SaveAsync();

            // While carrying an order, the customer's order page shows the driver moving.
            var active = await _deliveryRepository.GetActiveByDriverAsync(profile.Id);
            if (active != null && active.Status == DeliveryStatus.PickedUp)
            {
                await _realtime.DriverLocationAsync(active.OrderId, latitude, longitude);
            }

            return DriverProfileDto.FromEntity(profile);
        }

        // ---------- Finding and accepting work ----------

        public async Task<PagedResult<AvailableDeliveryDto>> GetAvailableAsync(string userId, PageQuery query)
        {
            await GetReadyDriverAsync(userId);

            var (orders, totalCount) = await _orderRepository.GetAwaitingDriverPagedAsync(query.Page, query.PageSize);

            return new PagedResult<AvailableDeliveryDto>
            {
                Items = orders.Select(AvailableDeliveryDto.FromOrder).ToList(),
                Page = query.Page,
                PageSize = query.PageSize,
                TotalCount = totalCount
            };
        }

        public async Task<DeliveryDto> AcceptAsync(int orderId, string userId)
        {
            var profile = await GetReadyDriverAsync(userId);

            // One job at a time keeps the flow simple and food warm.
            if (await _deliveryRepository.GetActiveByDriverAsync(profile.Id) != null)
            {
                throw new BadRequestException("Finish your current delivery first.");
            }

            var order = await _orderRepository.GetWithDetailsAsync(orderId)
                ?? throw new NotFoundException("Order not found.");

            if (order.Status != OrderStatus.ReadyForPickup || order.Delivery != null)
            {
                throw new BadRequestException("This delivery is no longer available.");
            }

            var delivery = new Delivery
            {
                OrderId = order.Id,
                DriverProfileId = profile.Id,
                Status = DeliveryStatus.Assigned,
                AssignedAt = DateTime.Now,
                // REQUIREMENTS.md: drivers earn the full delivery fee.
                DriverEarning = order.DeliveryFee
            };

            // Two drivers can press "Accept" at the same moment. The check above passes for both,
            // but the unique index on Deliveries.OrderId lets only the first insert through.
            if (!await _deliveryRepository.TryAssignAsync(delivery))
            {
                throw new BadRequestException("Another driver already took this delivery.");
            }

            // Status is still ReadyForPickup, but the order page now shows the driver.
            await _realtime.OrderChangedAsync(order.Id, order.Status);

            await _notifications.NotifyAsync(order.CustomerId,
                $"Order {order.OrderNumber}: driver assigned",
                "A driver is heading to the store to pick up your order.",
                NotificationType.OrderStatusChanged,
                OrderUrl(order.Id));
            await _notifications.NotifyAsync(order.Store.OwnerId,
                $"Order {order.OrderNumber}: driver assigned",
                "A driver is on the way to pick up this order.",
                NotificationType.OrderStatusChanged,
                OrderUrl(order.Id));
            await _activityLog.LogAsync(userId, ActivityType.DeliveryAccepted, $"Accepted delivery of order {order.OrderNumber}.");

            return await GetDeliveryDtoAsync(delivery.Id);
        }

        public async Task ReleaseAsync(int deliveryId, string userId)
        {
            var delivery = await GetOwnDeliveryAsync(deliveryId, userId);

            if (delivery.Status != DeliveryStatus.Assigned)
            {
                throw new BadRequestException("You can only give back a delivery before picking it up.");
            }

            // Deleting the row frees the order so another driver can accept it.
            await _deliveryRepository.DeleteAsync(delivery.Id);
            await _realtime.OrderChangedAsync(delivery.OrderId, delivery.Order.Status);
            await _activityLog.LogAsync(userId, ActivityType.DeliveryReleased, $"Released delivery of order {delivery.Order.OrderNumber}.");
        }

        // ---------- On the road ----------

        public async Task<DeliveryDto?> GetCurrentAsync(string userId)
        {
            var profile = await GetProfileEntityAsync(userId);
            var delivery = await _deliveryRepository.GetActiveByDriverAsync(profile.Id);
            return delivery == null ? null : DeliveryDto.FromEntity(delivery);
        }

        public async Task<DeliveryDto> PickUpAsync(int deliveryId, string userId)
        {
            var delivery = await GetOwnDeliveryAsync(deliveryId, userId);

            if (delivery.Status != DeliveryStatus.Assigned)
            {
                throw new BadRequestException("This delivery was already picked up or is closed.");
            }

            // The order and the delivery move together.
            OrderStatusRules.ChangeStatus(delivery.Order, OrderStatus.PickedUp, userId, null);
            delivery.Status = DeliveryStatus.PickedUp;
            delivery.PickedUpAt = DateTime.Now;
            await _deliveryRepository.SaveAsync();
            await _realtime.OrderChangedAsync(delivery.OrderId, delivery.Order.Status);

            await _notifications.NotifyAsync(delivery.Order.CustomerId,
                $"Order {delivery.Order.OrderNumber}: on the way",
                OrderStatusRules.CustomerMessage(OrderStatus.PickedUp, delivery.Order.Store.Name),
                NotificationType.OrderStatusChanged,
                OrderUrl(delivery.OrderId));

            return DeliveryDto.FromEntity(delivery);
        }

        public async Task<DeliveryDto> DeliverAsync(int deliveryId, string userId)
        {
            var delivery = await GetOwnDeliveryAsync(deliveryId, userId);

            if (delivery.Status != DeliveryStatus.PickedUp)
            {
                throw new BadRequestException("Pick up the order before marking it delivered.");
            }

            OrderStatusRules.ChangeStatus(delivery.Order, OrderStatus.Delivered, userId, null);
            delivery.Status = DeliveryStatus.Delivered;
            delivery.DeliveredAt = DateTime.Now;

            // Cash on delivery: the driver collected the money at the door.
            if (delivery.Order.PaymentMethod == PaymentMethod.CashOnDelivery)
            {
                delivery.Order.PaymentStatus = PaymentStatus.Paid;
            }

            await _deliveryRepository.SaveAsync();
            await _realtime.OrderChangedAsync(delivery.OrderId, delivery.Order.Status);

            await _notifications.NotifyAsync(delivery.Order.CustomerId,
                $"Order {delivery.Order.OrderNumber}: delivered",
                OrderStatusRules.CustomerMessage(OrderStatus.Delivered, delivery.Order.Store.Name) + " You can now rate the store and the driver.",
                NotificationType.OrderStatusChanged,
                OrderUrl(delivery.OrderId));
            await _notifications.NotifyAsync(delivery.Order.Store.OwnerId,
                $"Order {delivery.Order.OrderNumber}: delivered",
                "The order reached the customer.",
                NotificationType.OrderStatusChanged,
                OrderUrl(delivery.OrderId));
            await _activityLog.LogAsync(userId, ActivityType.DeliveryCompleted, $"Delivered order {delivery.Order.OrderNumber}.");

            return DeliveryDto.FromEntity(delivery);
        }

        // ---------- History and pay ----------

        public async Task<PagedResult<DeliveryDto>> GetMyDeliveriesAsync(string userId, DeliveryQuery query)
        {
            var profile = await GetProfileEntityAsync(userId);
            var (items, totalCount) = await _deliveryRepository.GetByDriverPagedAsync(profile.Id, query.Status, query.Page, query.PageSize);

            return new PagedResult<DeliveryDto>
            {
                Items = items.Select(DeliveryDto.FromEntity).ToList(),
                Page = query.Page,
                PageSize = query.PageSize,
                TotalCount = totalCount
            };
        }

        public async Task<EarningsDto> GetEarningsAsync(string userId)
        {
            var profile = await GetProfileEntityAsync(userId);

            var today = DateTime.Today;
            // Weeks start on Monday: go back (DayOfWeek + 6) % 7 days.
            var startOfWeek = today.AddDays(-(((int)today.DayOfWeek + 6) % 7));

            var (todayTotal, _) = await _deliveryRepository.GetEarningsAsync(profile.Id, today);
            var (weekTotal, _) = await _deliveryRepository.GetEarningsAsync(profile.Id, startOfWeek);
            var (allTimeTotal, count) = await _deliveryRepository.GetEarningsAsync(profile.Id, null);

            return new EarningsDto
            {
                Today = todayTotal,
                ThisWeek = weekTotal,
                AllTime = allTimeTotal,
                CompletedDeliveries = count
            };
        }

        // ---------- Helpers ----------

        private async Task<DriverProfile> GetProfileEntityAsync(string userId)
        {
            return await _driverProfileRepository.GetByUserIdAsync(userId)
                ?? throw new NotFoundException("You don't have a driver profile yet. Apply to become a driver first.");
        }

        // A driver who may take work right now: approved and online.
        private async Task<DriverProfile> GetReadyDriverAsync(string userId)
        {
            var profile = await GetProfileEntityAsync(userId);

            if (!profile.IsApproved)
            {
                throw new BadRequestException("Your driver application is still waiting for approval.");
            }
            if (!profile.IsOnline)
            {
                throw new BadRequestException("Go online to see and accept deliveries.");
            }

            return profile;
        }

        // Someone else's delivery looks the same as a missing one.
        private async Task<Delivery> GetOwnDeliveryAsync(int deliveryId, string userId)
        {
            var delivery = await _deliveryRepository.GetWithOrderAsync(deliveryId);

            if (delivery == null || delivery.DriverProfile.UserId != userId)
            {
                throw new NotFoundException("Delivery not found.");
            }

            return delivery;
        }

        private async Task<DeliveryDto> GetDeliveryDtoAsync(int deliveryId)
        {
            var delivery = await _deliveryRepository.GetWithOrderAsync(deliveryId)
                ?? throw new NotFoundException("Delivery not found.");
            return DeliveryDto.FromEntity(delivery);
        }

        private static string OrderUrl(int orderId) => $"/Orders/Details/{orderId}";
    }
}
