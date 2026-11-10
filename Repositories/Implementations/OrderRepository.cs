using Microsoft.EntityFrameworkCore;
using CedarDelivery.Data;
using CedarDelivery.Models;
using CedarDelivery.Repositories.Interfaces;

namespace CedarDelivery.Repositories.Implementations
{
    public class OrderRepository : GenericRepository<Order>, IOrderRepository
    {
        public OrderRepository(ApplicationDbContext context) : base(context) { }

        public async Task<Order?> GetWithDetailsAsync(int id)
        {
            return await _context.Orders
                .Include(o => o.Store)
                .Include(o => o.Customer)
                .Include(o => o.Items)
                .Include(o => o.StatusHistory)
                .Include(o => o.Delivery)
                    .ThenInclude(d => d!.DriverProfile)
                        .ThenInclude(p => p.User)
                .AsSplitQuery() // several collections: separate SQL queries avoid a huge joined result
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<(List<Order> Items, int TotalCount)> GetByCustomerPagedAsync(string customerId, int page, int pageSize)
        {
            return await PageAsync(_context.Orders.Where(o => o.CustomerId == customerId), page, pageSize);
        }

        public async Task<(List<Order> Items, int TotalCount)> GetByStorePagedAsync(int storeId, OrderStatus? status, int page, int pageSize)
        {
            var query = _context.Orders.Where(o => o.StoreId == storeId);
            if (status.HasValue)
            {
                query = query.Where(o => o.Status == status.Value);
            }
            return await PageAsync(query, page, pageSize);
        }

        public async Task<(List<Order> Items, int TotalCount)> GetAllPagedAsync(OrderStatus? status, int? storeId, int page, int pageSize)
        {
            var query = _context.Orders.AsQueryable();
            if (status.HasValue)
            {
                query = query.Where(o => o.Status == status.Value);
            }
            if (storeId.HasValue)
            {
                query = query.Where(o => o.StoreId == storeId.Value);
            }
            return await PageAsync(query, page, pageSize);
        }

        public async Task<bool> OrderNumberExistsAsync(string orderNumber)
        {
            return await _context.Orders.AnyAsync(o => o.OrderNumber == orderNumber);
        }

        public async Task PlaceOrderAsync(Order order, List<CartItem> cartItems)
        {
            _context.Orders.Add(order);           // also adds order.Items and order.StatusHistory
            _context.CartItems.RemoveRange(cartItems);
            await _context.SaveChangesAsync();
        }

        // Shared paging for the order lists: newest first, with what OrderSummaryDto needs.
        private static async Task<(List<Order> Items, int TotalCount)> PageAsync(IQueryable<Order> query, int page, int pageSize)
        {
            var totalCount = await query.CountAsync();

            var items = await query
                .Include(o => o.Store)
                .Include(o => o.Customer)
                .Include(o => o.Items)
                .OrderByDescending(o => o.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsSplitQuery()
                .ToListAsync();

            return (items, totalCount);
        }
    }
}
