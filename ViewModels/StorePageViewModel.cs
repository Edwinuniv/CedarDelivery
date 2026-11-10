using CedarDelivery.ViewModels.Api;

namespace CedarDelivery.ViewModels
{
    // The store page: its menu plus the latest reviews.
    public class StorePageViewModel
    {
        public MenuDto Menu { get; set; } = null!;
        public ReviewListDto? Reviews { get; set; }
    }
}
