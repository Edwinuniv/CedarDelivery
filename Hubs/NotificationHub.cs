using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using CedarDelivery.Constants;
using CedarDelivery.Services;
using CedarDelivery.Services.Interfaces;

namespace CedarDelivery.Hubs
{
    // The live connection between the browser and the server (SignalR).
    // - Every signed-in user receives their own notifications here
    //   (SignalR knows the user from the login cookie: Clients.User(userId)).
    // - An open order page joins that order's group to get live status and driver location.
    [Authorize]
    public class NotificationHub : Hub
    {
        private readonly IOrderService _orderService;

        public NotificationHub(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public static string OrderGroup(int orderId) => $"order-{orderId}";

        // Called by the order page. Only people involved in the order may follow it:
        // GetOrderAsync throws NotFound for everyone else, so they are simply not added.
        public async Task JoinOrder(int orderId)
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return;
            }

            try
            {
                await _orderService.GetOrderAsync(orderId, userId, Context.User!.IsInRole(Roles.Admin));
                await Groups.AddToGroupAsync(Context.ConnectionId, OrderGroup(orderId));
            }
            catch (NotFoundException)
            {
                // Not involved in this order: ignore.
            }
        }
    }
}
