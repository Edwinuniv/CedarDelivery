using CedarDelivery.Models;
using CedarDelivery.Repositories.Interfaces;
using CedarDelivery.Services.Interfaces;
using CedarDelivery.ViewModels.Api;

namespace CedarDelivery.Services.Implementations
{
    public class AddressService : IAddressService
    {
        private const int MaxAddressesPerUser = 10;

        private readonly IAddressRepository _addressRepository;
        private readonly IActivityLogService _activityLog;

        public AddressService(IAddressRepository addressRepository, IActivityLogService activityLog)
        {
            _addressRepository = addressRepository;
            _activityLog = activityLog;
        }

        public async Task<List<AddressDto>> GetMineAsync(string userId)
        {
            var addresses = await _addressRepository.GetByUserAsync(userId);
            return addresses.Select(AddressDto.FromEntity).ToList();
        }

        public async Task<AddressDto> CreateAsync(string userId, AddressRequest request)
        {
            var existing = await _addressRepository.GetByUserAsync(userId);

            if (existing.Count >= MaxAddressesPerUser)
            {
                throw new BadRequestException($"You can save up to {MaxAddressesPerUser} addresses.");
            }

            var address = new Address { UserId = userId, CreatedAt = DateTime.Now };
            ApplyRequest(address, request);

            // The first address is always the default. A new default replaces the old one.
            address.IsDefault = existing.Count == 0 || request.IsDefault;
            if (address.IsDefault)
            {
                existing.ForEach(a => a.IsDefault = false);
            }

            await _addressRepository.AddAsync(address); // also saves the IsDefault changes above
            await _activityLog.LogAsync(userId, ActivityType.AddressAdded, $"Added address \"{address.Label}\".");

            return AddressDto.FromEntity(address);
        }

        public async Task<AddressDto> UpdateAsync(int id, string userId, AddressRequest request)
        {
            var address = await GetOwnAddressAsync(id, userId);
            ApplyRequest(address, request);

            if (request.IsDefault && !address.IsDefault)
            {
                await MakeDefaultAsync(address, userId);
            }

            await _addressRepository.SaveAsync();
            return AddressDto.FromEntity(address);
        }

        public async Task DeleteAsync(int id, string userId)
        {
            var address = await GetOwnAddressAsync(id, userId);

            await _addressRepository.DeleteAsync(id);

            // Keep exactly one default while the user has addresses left.
            if (address.IsDefault)
            {
                var next = (await _addressRepository.GetByUserAsync(userId)).FirstOrDefault();
                if (next != null)
                {
                    next.IsDefault = true;
                    await _addressRepository.SaveAsync();
                }
            }

            await _activityLog.LogAsync(userId, ActivityType.AddressRemoved, $"Removed address \"{address.Label}\".");
        }

        public async Task<AddressDto> SetDefaultAsync(int id, string userId)
        {
            var address = await GetOwnAddressAsync(id, userId);
            await MakeDefaultAsync(address, userId);
            await _addressRepository.SaveAsync();
            return AddressDto.FromEntity(address);
        }

        // ---------- Helpers ----------

        private async Task<Address> GetOwnAddressAsync(int id, string userId)
        {
            var address = await _addressRepository.GetByIdAsync(id);

            if (address == null || address.UserId != userId)
            {
                throw new NotFoundException("Address not found.");
            }

            return address;
        }

        private async Task MakeDefaultAsync(Address address, string userId)
        {
            var all = await _addressRepository.GetByUserAsync(userId);
            foreach (var a in all)
            {
                a.IsDefault = a.Id == address.Id;
            }
            address.IsDefault = true;
        }

        private static void ApplyRequest(Address address, AddressRequest request)
        {
            address.Label = request.Label.Trim();
            address.City = request.City.Trim();
            address.Street = request.Street.Trim();
            address.Building = request.Building?.Trim();
            address.Floor = request.Floor?.Trim();
            address.Notes = request.Notes?.Trim();
            address.Latitude = request.Latitude;
            address.Longitude = request.Longitude;
        }
    }
}
