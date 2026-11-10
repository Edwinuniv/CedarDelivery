using CedarDelivery.Models;

namespace CedarDelivery.ViewModels
{
    // Small display helpers shared by the Razor views, so every page shows
    // statuses and prices the same way.
    public static class Ui
    {
        // "$12.50"
        public static string Money(decimal amount) => "$" + amount.ToString("0.00");

        // "ReadyForPickup" -> "Ready for pickup"
        public static string Words(Enum value)
        {
            var text = value.ToString();
            var result = new System.Text.StringBuilder();
            for (var i = 0; i < text.Length; i++)
            {
                if (i > 0 && char.IsUpper(text[i]))
                {
                    result.Append(' ').Append(char.ToLower(text[i]));
                }
                else
                {
                    result.Append(text[i]);
                }
            }
            return result.ToString();
        }

        // badge-tp color for an order status.
        public static string BadgeClass(OrderStatus status) => status switch
        {
            OrderStatus.Pending => "warning",
            OrderStatus.Accepted or OrderStatus.Preparing => "info",
            OrderStatus.ReadyForPickup or OrderStatus.PickedUp => "cedar",
            OrderStatus.Delivered => "success",
            _ => "danger" // Rejected, Cancelled
        };

        public static string BadgeClass(DeliveryStatus status) => status switch
        {
            DeliveryStatus.Assigned => "info",
            DeliveryStatus.PickedUp => "cedar",
            DeliveryStatus.Delivered => "success",
            _ => "danger"
        };

        public static string BadgeClass(ApplicationStatus status) => status switch
        {
            ApplicationStatus.Pending => "warning",
            ApplicationStatus.Approved => "success",
            _ => "danger"
        };

        // Bootstrap icon for a store category.
        public static string CategoryIcon(StoreCategory category) => category switch
        {
            StoreCategory.Restaurant => "bi-egg-fried",
            StoreCategory.Grocery => "bi-basket2-fill",
            StoreCategory.Pharmacy => "bi-capsule",
            _ => "bi-shop"
        };

        // "09:00 – 23:00", or "Open 24 hours" when open and close times are the same.
        public static string Hours(TimeOnly opensAt, TimeOnly closesAt) =>
            opensAt == closesAt ? "Open 24 hours" : $"{opensAt:HH:mm} – {closesAt:HH:mm}";

        // "4.5 ★ (12)" or "New" when there are no reviews yet.
        public static string Rating(double? average, int count) =>
            average.HasValue ? $"{average:0.0} ★ ({count})" : "New";

        public static string Stars(int rating) => new string('★', rating) + new string('☆', 5 - rating);
    }
}
