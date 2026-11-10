using CedarDelivery.ViewModels.Api;

namespace CedarDelivery.ViewModels
{
    // The driver's main page: online switch, current job, jobs to take, earnings.
    public class DriverDashboardViewModel
    {
        public DriverProfileDto Profile { get; set; } = null!;
        public DeliveryDto? Current { get; set; }
        public List<AvailableDeliveryDto> Available { get; set; } = new List<AvailableDeliveryDto>();
        public EarningsDto Earnings { get; set; } = new EarningsDto();
        public ReviewListDto? Reviews { get; set; }
    }
}
