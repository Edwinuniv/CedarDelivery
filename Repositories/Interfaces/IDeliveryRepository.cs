using CedarDelivery.Models;

namespace CedarDelivery.Repositories.Interfaces
{
    public interface IDeliveryRepository : IGenericRepository<Delivery>
    {
        // Delivery with its order, store, customer, timeline and driver.
        Task<Delivery?> GetWithOrderAsync(int id);

        // The driver's current job (Assigned or PickedUp), if any.
        Task<Delivery?> GetActiveByDriverAsync(int driverProfileId);

        Task<(List<Delivery> Items, int TotalCount)> GetByDriverPagedAsync(int driverProfileId, DeliveryStatus? status, int page, int pageSize);

        // Sum of DriverEarning for delivered jobs since a date (null = all time), and how many.
        Task<(decimal Total, int Count)> GetEarningsAsync(int driverProfileId, DateTime? since);

        // Inserts the delivery. Returns false if another driver took the order first
        // (the unique index on OrderId rejects the second insert).
        Task<bool> TryAssignAsync(Delivery delivery);
    }
}
