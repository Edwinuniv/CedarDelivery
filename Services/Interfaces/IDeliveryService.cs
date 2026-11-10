using CedarDelivery.ViewModels.Api;

namespace CedarDelivery.Services.Interfaces
{
    // Everything a driver does: go online, find a delivery, pick it up, deliver it.
    public interface IDeliveryService
    {
        // Driver profile
        Task<DriverProfileDto> GetProfileAsync(string userId);
        Task<DriverProfileDto> SetOnlineAsync(string userId, bool isOnline);
        Task<DriverProfileDto> UpdateLocationAsync(string userId, decimal latitude, decimal longitude);

        // Finding and accepting work
        Task<PagedResult<AvailableDeliveryDto>> GetAvailableAsync(string userId, PageQuery query);
        Task<DeliveryDto> AcceptAsync(int orderId, string userId);
        Task ReleaseAsync(int deliveryId, string userId);

        // On the road
        Task<DeliveryDto?> GetCurrentAsync(string userId);
        Task<DeliveryDto> PickUpAsync(int deliveryId, string userId);
        Task<DeliveryDto> DeliverAsync(int deliveryId, string userId);

        // History and pay
        Task<PagedResult<DeliveryDto>> GetMyDeliveriesAsync(string userId, DeliveryQuery query);
        Task<EarningsDto> GetEarningsAsync(string userId);
    }
}
