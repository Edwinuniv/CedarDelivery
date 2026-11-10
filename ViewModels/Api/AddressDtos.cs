using System.ComponentModel.DataAnnotations;
using CedarDelivery.Models;

namespace CedarDelivery.ViewModels.Api
{
    public class AddressDto
    {
        public int Id { get; set; }
        public string Label { get; set; } = "";
        public string City { get; set; } = "";
        public string Street { get; set; } = "";
        public string? Building { get; set; }
        public string? Floor { get; set; }
        public string? Notes { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public bool IsDefault { get; set; }

        // One-line version, e.g. "Hamra St, Bldg Azar, Floor 3, Beirut".
        public string FullText { get; set; } = "";

        public static AddressDto FromEntity(Address address) => new AddressDto
        {
            Id = address.Id,
            Label = address.Label,
            City = address.City,
            Street = address.Street,
            Building = address.Building,
            Floor = address.Floor,
            Notes = address.Notes,
            Latitude = address.Latitude,
            Longitude = address.Longitude,
            IsDefault = address.IsDefault,
            FullText = ToText(address)
        };

        public static string ToText(Address address)
        {
            var parts = new List<string> { address.Street };
            if (!string.IsNullOrWhiteSpace(address.Building)) parts.Add($"Bldg {address.Building}");
            if (!string.IsNullOrWhiteSpace(address.Floor)) parts.Add($"Floor {address.Floor}");
            parts.Add(address.City);
            return string.Join(", ", parts);
        }
    }

    public class AddressRequest
    {
        [Required, MaxLength(50)]
        public string Label { get; set; } = "";

        [Required, MaxLength(100)]
        public string City { get; set; } = "";

        [Required, MaxLength(200)]
        public string Street { get; set; } = "";

        [MaxLength(100)]
        public string? Building { get; set; }

        [MaxLength(20)]
        public string? Floor { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        [Range(-90, 90)]
        public decimal? Latitude { get; set; }

        [Range(-180, 180)]
        public decimal? Longitude { get; set; }

        public bool IsDefault { get; set; }
    }
}
