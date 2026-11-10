using System.ComponentModel.DataAnnotations;
using CedarDelivery.Models;

namespace CedarDelivery.ViewModels.Api
{
    // What the API returns for a store. Entities are never returned directly,
    // so internal fields (OwnerId, navigation properties) don't leak.
    public class StoreDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public StoreCategory Category { get; set; }
        public string? LogoUrl { get; set; }
        public string Address { get; set; } = "";
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string? Phone { get; set; }
        public TimeOnly OpensAt { get; set; }
        public TimeOnly ClosesAt { get; set; }
        public decimal DeliveryFee { get; set; }
        public decimal MinimumOrder { get; set; }
        public bool IsOpen { get; set; }
        public bool IsApproved { get; set; }
        public DateTime CreatedAt { get; set; }

        public static StoreDto FromEntity(Store store) => new StoreDto
        {
            Id = store.Id,
            Name = store.Name,
            Description = store.Description,
            Category = store.Category,
            LogoUrl = store.LogoUrl,
            Address = store.Address,
            Latitude = store.Latitude,
            Longitude = store.Longitude,
            Phone = store.Phone,
            OpensAt = store.OpensAt,
            ClosesAt = store.ClosesAt,
            DeliveryFee = store.DeliveryFee,
            MinimumOrder = store.MinimumOrder,
            IsOpen = store.IsOpen,
            IsApproved = store.IsApproved,
            CreatedAt = store.CreatedAt
        };
    }

    // Body for creating or editing a store.
    // Nullable + [Required] means "must be sent": a missing enum or time
    // would otherwise silently become its default value.
    public class StoreRequest
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = "";

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        public StoreCategory? Category { get; set; }

        [MaxLength(500)]
        public string? LogoUrl { get; set; }

        [Required, MaxLength(300)]
        public string Address { get; set; } = "";

        [Range(-90, 90)]
        public decimal? Latitude { get; set; }

        [Range(-180, 180)]
        public decimal? Longitude { get; set; }

        [MaxLength(20)]
        public string? Phone { get; set; }

        [Required]
        public TimeOnly? OpensAt { get; set; }

        [Required]
        public TimeOnly? ClosesAt { get; set; }

        [Range(0, 100)]
        public decimal DeliveryFee { get; set; }

        [Range(0, 1000)]
        public decimal MinimumOrder { get; set; }
    }

    // ?category=Restaurant&search=pizza&page=1&pageSize=10
    public class StoreQuery : PageQuery
    {
        public StoreCategory? Category { get; set; }

        [MaxLength(100)]
        public string? Search { get; set; }
    }

    public class SetStoreOpenRequest
    {
        [Required]
        public bool? IsOpen { get; set; }
    }

    public class SetStoreApprovalRequest
    {
        [Required]
        public bool? IsApproved { get; set; }
    }

    // A store's public menu: categories with their available products.
    public class MenuDto
    {
        public StoreDto Store { get; set; } = null!;
        public List<MenuCategoryDto> Categories { get; set; } = new List<MenuCategoryDto>();
    }

    public class MenuCategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public List<ProductDto> Products { get; set; } = new List<ProductDto>();
    }
}
