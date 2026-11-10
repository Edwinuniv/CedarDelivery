using CedarDelivery.Models;
using CedarDelivery.ViewModels.Api;

namespace CedarDelivery.Services.Interfaces
{
    public interface IOrderService
    {
        // Customer
        Task<OrderDetailsDto> PlaceOrderAsync(string userId, PlaceOrderRequest request);
        Task<PagedResult<OrderSummaryDto>> GetMyOrdersAsync(string userId, PageQuery query);
        Task<OrderDetailsDto> CancelAsync(int orderId, string userId, bool isAdmin, string? reason);

        // Anyone involved in the order (customer, store owner, driver) or an admin.
        Task<OrderDetailsDto> GetOrderAsync(int orderId, string userId, bool isAdmin);

        // Merchant
        Task<PagedResult<OrderSummaryDto>> GetStoreOrdersAsync(int storeId, string userId, bool isAdmin, OrderQuery query);
        Task<OrderDetailsDto> UpdateStatusByStoreAsync(int orderId, string userId, bool isAdmin, OrderStatus status, string? note);

        // Admin
        Task<PagedResult<OrderSummaryDto>> GetAllOrdersAsync(OrderQuery query);
    }
}
