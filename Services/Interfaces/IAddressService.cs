using CedarDelivery.ViewModels.Api;

namespace CedarDelivery.Services.Interfaces
{
    // A user's saved delivery addresses. Users only ever see and change their own.
    public interface IAddressService
    {
        Task<List<AddressDto>> GetMineAsync(string userId);
        Task<AddressDto> CreateAsync(string userId, AddressRequest request);
        Task<AddressDto> UpdateAsync(int id, string userId, AddressRequest request);
        Task DeleteAsync(int id, string userId);
        Task<AddressDto> SetDefaultAsync(int id, string userId);
    }
}
