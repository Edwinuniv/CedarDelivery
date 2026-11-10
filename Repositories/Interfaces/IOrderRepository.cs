using CedarDelivery.Models;

namespace CedarDelivery.Repositories.Interfaces
{
    public interface IOrderRepository : IGenericRepository<Order>
    {
        // Order with everything the order page shows: store, customer, items, timeline, driver.
        Task<Order?> GetWithDetailsAsync(int id);

        Task<(List<Order> Items, int TotalCount)> GetByCustomerPagedAsync(string customerId, int page, int pageSize);
        Task<(List<Order> Items, int TotalCount)> GetByStorePagedAsync(int storeId, OrderStatus? status, int page, int pageSize);
        // Admin view; both filters optional.
        Task<(List<Order> Items, int TotalCount)> GetAllPagedAsync(OrderStatus? status, int? storeId, int page, int pageSize);

        Task<bool> OrderNumberExistsAsync(string orderNumber);

        // Saves the new order and empties the cart in ONE SaveChanges,
        // so both happen or neither does (EF wraps a SaveChanges in a transaction).
        Task PlaceOrderAsync(Order order, List<CartItem> cartItems);
    }
}
