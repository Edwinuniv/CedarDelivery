using System.ComponentModel.DataAnnotations;
using CedarDelivery.Models;

namespace CedarDelivery.ViewModels.Api
{
    // Short version for order lists.
    public class OrderSummaryDto
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = "";
        public int StoreId { get; set; }
        public string StoreName { get; set; } = "";
        public string CustomerName { get; set; } = "";
        public OrderStatus Status { get; set; }
        public decimal Total { get; set; }
        public int ItemCount { get; set; }
        public DateTime CreatedAt { get; set; }

        // Needs Store, Customer and Items loaded.
        public static OrderSummaryDto FromEntity(Order order) => new OrderSummaryDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            StoreId = order.StoreId,
            StoreName = order.Store?.Name ?? "",
            CustomerName = order.Customer == null ? "" : $"{order.Customer.FirstName} {order.Customer.LastName}",
            Status = order.Status,
            Total = order.Total,
            ItemCount = order.Items.Sum(i => i.Quantity),
            CreatedAt = order.CreatedAt
        };
    }

    // Full version for the order page: items, timeline, address, driver.
    public class OrderDetailsDto
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = "";
        public OrderStatus Status { get; set; }

        public int StoreId { get; set; }
        public string StoreName { get; set; } = "";
        public string StoreAddress { get; set; } = "";
        public string? StorePhone { get; set; }
        // Lets the page know if the viewer is the customer or the store owner.
        public string StoreOwnerId { get; set; } = "";
        public decimal? StoreLatitude { get; set; }
        public decimal? StoreLongitude { get; set; }

        public string CustomerId { get; set; } = "";
        public string CustomerName { get; set; } = "";
        public string? CustomerPhone { get; set; }

        public List<OrderItemDto> Items { get; set; } = new List<OrderItemDto>();
        public decimal Subtotal { get; set; }
        public decimal DeliveryFee { get; set; }
        public decimal Total { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public string? Notes { get; set; }

        public string DeliveryAddress { get; set; } = "";
        public decimal? DeliveryLatitude { get; set; }
        public decimal? DeliveryLongitude { get; set; }

        public DriverInfoDto? Driver { get; set; }
        public List<OrderStatusHistoryDto> Timeline { get; set; } = new List<OrderStatusHistoryDto>();
        // What the customer already rated (Store, Driver), so the page hides those buttons.
        public List<ReviewTarget> ReviewedTargets { get; set; } = new List<ReviewTarget>();
        public DateTime CreatedAt { get; set; }

        // Needs Store, Customer, Items, StatusHistory and Delivery.DriverProfile.User loaded.
        public static OrderDetailsDto FromEntity(Order order) => new OrderDetailsDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            Status = order.Status,
            StoreId = order.StoreId,
            StoreName = order.Store.Name,
            StoreAddress = order.Store.Address,
            StorePhone = order.Store.Phone,
            StoreOwnerId = order.Store.OwnerId,
            StoreLatitude = order.Store.Latitude,
            StoreLongitude = order.Store.Longitude,
            CustomerId = order.CustomerId,
            CustomerName = $"{order.Customer.FirstName} {order.Customer.LastName}",
            CustomerPhone = order.Customer.PhoneNumber,
            Items = order.Items.Select(OrderItemDto.FromEntity).ToList(),
            Subtotal = order.Subtotal,
            DeliveryFee = order.DeliveryFee,
            Total = order.Total,
            PaymentMethod = order.PaymentMethod,
            PaymentStatus = order.PaymentStatus,
            Notes = order.Notes,
            DeliveryAddress = order.DeliveryAddress,
            DeliveryLatitude = order.DeliveryLatitude,
            DeliveryLongitude = order.DeliveryLongitude,
            Driver = order.Delivery?.DriverProfile?.User == null ? null : new DriverInfoDto
            {
                Name = $"{order.Delivery.DriverProfile.User.FirstName} {order.Delivery.DriverProfile.User.LastName}",
                Phone = order.Delivery.DriverProfile.User.PhoneNumber,
                VehicleType = order.Delivery.DriverProfile.VehicleType,
                PlateNumber = order.Delivery.DriverProfile.PlateNumber,
                Latitude = order.Delivery.DriverProfile.CurrentLatitude,
                Longitude = order.Delivery.DriverProfile.CurrentLongitude
            },
            Timeline = order.StatusHistory
                .OrderBy(h => h.ChangedAt)
                .Select(h => new OrderStatusHistoryDto { Status = h.Status, ChangedAt = h.ChangedAt, Note = h.Note })
                .ToList(),
            ReviewedTargets = order.Reviews.Select(r => r.TargetType).ToList(),
            CreatedAt = order.CreatedAt
        };
    }

    public class OrderItemDto
    {
        public int? ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal LineTotal { get; set; }

        public static OrderItemDto FromEntity(OrderItem item) => new OrderItemDto
        {
            ProductId = item.ProductId,
            ProductName = item.ProductName,
            UnitPrice = item.UnitPrice,
            Quantity = item.Quantity,
            LineTotal = item.UnitPrice * item.Quantity
        };
    }

    public class OrderStatusHistoryDto
    {
        public OrderStatus Status { get; set; }
        public DateTime ChangedAt { get; set; }
        public string? Note { get; set; }
    }

    public class DriverInfoDto
    {
        public string Name { get; set; } = "";
        public string? Phone { get; set; }
        public VehicleType VehicleType { get; set; }
        public string? PlateNumber { get; set; }
        // Last known position, for the map on the order page.
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
    }

    // Checkout: the cart becomes an order.
    public class PlaceOrderRequest
    {
        [Required]
        public int? AddressId { get; set; }

        [Required]
        public PaymentMethod? PaymentMethod { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    public class UpdateOrderStatusRequest
    {
        [Required]
        public OrderStatus? Status { get; set; }

        [MaxLength(500)]
        public string? Note { get; set; }
    }

    public class CancelOrderRequest
    {
        [MaxLength(500)]
        public string? Reason { get; set; }
    }

    // ?status=Pending&page=1
    public class OrderQuery : PageQuery
    {
        public OrderStatus? Status { get; set; }
        public int? StoreId { get; set; }
    }
}
