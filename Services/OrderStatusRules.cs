using CedarDelivery.Models;

namespace CedarDelivery.Services
{
    // The order status flow in one place, used by every service that changes a status:
    //
    //   Pending -> Accepted -> Preparing -> ReadyForPickup -> PickedUp -> Delivered
    //      |          |           |              |
    //      |          +-----------+--------------+--> Cancelled (admin only after Pending)
    //      +--> Rejected (store)   +--> Cancelled (customer, only while Pending)
    //
    // Who may make each change (merchant, driver, customer, admin) is checked in the services.
    public static class OrderStatusRules
    {
        private static readonly Dictionary<OrderStatus, OrderStatus[]> AllowedNext = new()
        {
            [OrderStatus.Pending] = new[] { OrderStatus.Accepted, OrderStatus.Rejected, OrderStatus.Cancelled },
            [OrderStatus.Accepted] = new[] { OrderStatus.Preparing, OrderStatus.Cancelled },
            [OrderStatus.Preparing] = new[] { OrderStatus.ReadyForPickup, OrderStatus.Cancelled },
            [OrderStatus.ReadyForPickup] = new[] { OrderStatus.PickedUp, OrderStatus.Cancelled },
            [OrderStatus.PickedUp] = new[] { OrderStatus.Delivered },
            [OrderStatus.Delivered] = Array.Empty<OrderStatus>(),
            [OrderStatus.Rejected] = Array.Empty<OrderStatus>(),
            [OrderStatus.Cancelled] = Array.Empty<OrderStatus>()
        };

        // The statuses a merchant sets from the store dashboard.
        public static readonly OrderStatus[] MerchantStatuses =
        {
            OrderStatus.Accepted, OrderStatus.Rejected, OrderStatus.Preparing, OrderStatus.ReadyForPickup
        };

        public static bool IsFinal(OrderStatus status) =>
            status is OrderStatus.Delivered or OrderStatus.Rejected or OrderStatus.Cancelled;

        // Moves the order to a new status and adds a line to its timeline.
        // Throws if the flow doesn't allow it (e.g. Delivered -> Preparing).
        public static void ChangeStatus(Order order, OrderStatus newStatus, string changedByUserId, string? note)
        {
            if (!AllowedNext[order.Status].Contains(newStatus))
            {
                throw new BadRequestException($"An order that is {order.Status} can't become {newStatus}.");
            }

            order.Status = newStatus;
            order.StatusHistory.Add(new OrderStatusHistory
            {
                Status = newStatus,
                ChangedAt = DateTime.Now,
                ChangedByUserId = changedByUserId,
                Note = note
            });
        }

        // Text for the customer's notification.
        public static string CustomerMessage(OrderStatus status, string storeName) => status switch
        {
            OrderStatus.Accepted => $"{storeName} accepted your order.",
            OrderStatus.Rejected => $"{storeName} couldn't take your order.",
            OrderStatus.Preparing => $"{storeName} is preparing your order.",
            OrderStatus.ReadyForPickup => "Your order is ready and waiting for a driver.",
            OrderStatus.PickedUp => "A driver picked up your order and is on the way.",
            OrderStatus.Delivered => "Your order was delivered. Enjoy!",
            OrderStatus.Cancelled => "Your order was cancelled.",
            _ => $"Your order is now {status}."
        };
    }
}
