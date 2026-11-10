using System.ComponentModel.DataAnnotations;
using CedarDelivery.Models;

namespace CedarDelivery.ViewModels.Api
{
    public class DriverProfileDto
    {
        public int Id { get; set; }
        public VehicleType VehicleType { get; set; }
        public string? PlateNumber { get; set; }
        public bool IsOnline { get; set; }
        public bool IsApproved { get; set; }
        public decimal? CurrentLatitude { get; set; }
        public decimal? CurrentLongitude { get; set; }

        public static DriverProfileDto FromEntity(DriverProfile profile) => new DriverProfileDto
        {
            Id = profile.Id,
            VehicleType = profile.VehicleType,
            PlateNumber = profile.PlateNumber,
            IsOnline = profile.IsOnline,
            IsApproved = profile.IsApproved,
            CurrentLatitude = profile.CurrentLatitude,
            CurrentLongitude = profile.CurrentLongitude
        };
    }

    public class SetOnlineRequest
    {
        [Required]
        public bool? IsOnline { get; set; }
    }

    public class UpdateLocationRequest
    {
        [Required, Range(-90, 90)]
        public decimal? Latitude { get; set; }

        [Required, Range(-180, 180)]
        public decimal? Longitude { get; set; }
    }

    // An order that is ready and has no driver yet: what a driver needs to decide.
    public class AvailableDeliveryDto
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = "";
        public string StoreName { get; set; } = "";
        public string StoreAddress { get; set; } = "";
        public decimal? StoreLatitude { get; set; }
        public decimal? StoreLongitude { get; set; }
        public string DeliveryAddress { get; set; } = "";
        public decimal? DeliveryLatitude { get; set; }
        public decimal? DeliveryLongitude { get; set; }
        public int ItemCount { get; set; }
        public decimal OrderTotal { get; set; }
        public decimal Earning { get; set; }
        public DateTime? ReadySince { get; set; }

        // Needs Store, Items and StatusHistory loaded.
        public static AvailableDeliveryDto FromOrder(Order order) => new AvailableDeliveryDto
        {
            OrderId = order.Id,
            OrderNumber = order.OrderNumber,
            StoreName = order.Store.Name,
            StoreAddress = order.Store.Address,
            StoreLatitude = order.Store.Latitude,
            StoreLongitude = order.Store.Longitude,
            DeliveryAddress = order.DeliveryAddress,
            DeliveryLatitude = order.DeliveryLatitude,
            DeliveryLongitude = order.DeliveryLongitude,
            ItemCount = order.Items.Sum(i => i.Quantity),
            OrderTotal = order.Total,
            Earning = order.DeliveryFee,
            ReadySince = order.StatusHistory
                .Where(h => h.Status == OrderStatus.ReadyForPickup)
                .Select(h => (DateTime?)h.ChangedAt)
                .LastOrDefault()
        };
    }

    // A delivery the driver accepted, with everything needed on the road.
    public class DeliveryDto
    {
        public int Id { get; set; }
        public DeliveryStatus Status { get; set; }
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = "";
        public OrderStatus OrderStatus { get; set; }

        public string StoreName { get; set; } = "";
        public string StoreAddress { get; set; } = "";
        public string? StorePhone { get; set; }

        public string CustomerName { get; set; } = "";
        public string? CustomerPhone { get; set; }
        public string DeliveryAddress { get; set; } = "";
        public decimal? DeliveryLatitude { get; set; }
        public decimal? DeliveryLongitude { get; set; }

        // Cash the driver collects at the door (cash on delivery).
        public decimal AmountToCollect { get; set; }
        public decimal DriverEarning { get; set; }

        public DateTime AssignedAt { get; set; }
        public DateTime? PickedUpAt { get; set; }
        public DateTime? DeliveredAt { get; set; }

        // Needs Order.Store and Order.Customer loaded.
        public static DeliveryDto FromEntity(Delivery delivery) => new DeliveryDto
        {
            Id = delivery.Id,
            Status = delivery.Status,
            OrderId = delivery.OrderId,
            OrderNumber = delivery.Order.OrderNumber,
            OrderStatus = delivery.Order.Status,
            StoreName = delivery.Order.Store.Name,
            StoreAddress = delivery.Order.Store.Address,
            StorePhone = delivery.Order.Store.Phone,
            CustomerName = $"{delivery.Order.Customer.FirstName} {delivery.Order.Customer.LastName}",
            CustomerPhone = delivery.Order.Customer.PhoneNumber,
            DeliveryAddress = delivery.Order.DeliveryAddress,
            DeliveryLatitude = delivery.Order.DeliveryLatitude,
            DeliveryLongitude = delivery.Order.DeliveryLongitude,
            AmountToCollect = delivery.Order.PaymentMethod == PaymentMethod.CashOnDelivery ? delivery.Order.Total : 0,
            DriverEarning = delivery.DriverEarning,
            AssignedAt = delivery.AssignedAt,
            PickedUpAt = delivery.PickedUpAt,
            DeliveredAt = delivery.DeliveredAt
        };
    }

    public class EarningsDto
    {
        public decimal Today { get; set; }
        public decimal ThisWeek { get; set; }
        public decimal AllTime { get; set; }
        public int CompletedDeliveries { get; set; }
    }

    public class DeliveryQuery : PageQuery
    {
        public DeliveryStatus? Status { get; set; }
    }
}
