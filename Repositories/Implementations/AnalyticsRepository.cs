using Microsoft.EntityFrameworkCore;
using CedarDelivery.Data;
using CedarDelivery.Models;
using CedarDelivery.Repositories.Interfaces;
using CedarDelivery.ViewModels.Api;

namespace CedarDelivery.Repositories.Implementations
{
    public class AnalyticsRepository : IAnalyticsRepository
    {
        private readonly ApplicationDbContext _context;

        public AnalyticsRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<AnalyticsDto> GetDashboardAsync(DateTime since, int days)
        {
            var periodOrders = _context.Orders.Where(o => o.CreatedAt >= since);
            var delivered = periodOrders.Where(o => o.Status == OrderStatus.Delivered);

            var dto = new AnalyticsDto
            {
                Days = days,
                TotalOrders = await periodOrders.CountAsync(),
                DeliveredOrders = await delivered.CountAsync(),
                CancelledOrRejectedOrders = await periodOrders.CountAsync(o =>
                    o.Status == OrderStatus.Cancelled || o.Status == OrderStatus.Rejected),
                // (decimal?) so an empty period sums to null -> 0 instead of failing.
                Revenue = await delivered.SumAsync(o => (decimal?)o.Total) ?? 0,
                DeliveryFees = await delivered.SumAsync(o => (decimal?)o.DeliveryFee) ?? 0,

                ActiveOrders = await _context.Orders.CountAsync(o =>
                    o.Status != OrderStatus.Delivered && o.Status != OrderStatus.Cancelled && o.Status != OrderStatus.Rejected),
                ApprovedStores = await _context.Stores.CountAsync(s => s.IsApproved),
                PendingStores = await _context.Stores.CountAsync(s => !s.IsApproved),
                OnlineDrivers = await _context.DriverProfiles.CountAsync(d => d.IsOnline && d.IsApproved),
                TotalUsers = await _context.Users.CountAsync(),
                PendingApplications = await _context.RoleApplications.CountAsync(a => a.Status == ApplicationStatus.Pending)
            };

            dto.AverageOrderValue = dto.DeliveredOrders == 0 ? 0 : Math.Round(dto.Revenue / dto.DeliveredOrders, 2);

            // Orders and revenue per day (GROUP BY the date part of CreatedAt).
            var perDay = await periodOrders
                .GroupBy(o => o.CreatedAt.Date)
                .Select(g => new DailyStatDto
                {
                    Date = g.Key,
                    Orders = g.Count(),
                    // Only delivered orders count as revenue.
                    Revenue = g.Sum(o => o.Status == OrderStatus.Delivered ? o.Total : 0)
                })
                .ToListAsync();

            // Fill days with no orders with zeros, so a chart has every day.
            dto.OrdersPerDay = Enumerable.Range(0, days)
                .Select(i => since.Date.AddDays(i))
                .Select(date => perDay.FirstOrDefault(d => d.Date == date) ?? new DailyStatDto { Date = date })
                .ToList();

            // Top 5 stores by revenue from delivered orders.
            dto.TopStores = await delivered
                .GroupBy(o => new { o.StoreId, o.Store.Name })
                .Select(g => new TopStoreDto
                {
                    StoreId = g.Key.StoreId,
                    StoreName = g.Key.Name,
                    DeliveredOrders = g.Count(),
                    Revenue = g.Sum(o => o.Total)
                })
                .OrderByDescending(s => s.Revenue)
                .Take(5)
                .ToListAsync();

            return dto;
        }
    }
}
