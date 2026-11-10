using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CedarDelivery.Models;
using CedarDelivery.Repositories.Interfaces;
using CedarDelivery.Services.Interfaces;
using CedarDelivery.ViewModels;
using CedarDelivery.ViewModels.Api;

namespace CedarDelivery.Controllers
{
    [Authorize]
    public class AddressesController : BaseController
    {
        private readonly IAddressService _addressService;

        public AddressesController(UserManager<User> userManager, IUserRepository userRepository, IAddressService addressService)
            : base(userManager, userRepository)
        {
            _addressService = addressService;
        }

        public async Task<IActionResult> Index()
        {
            return View(new AddressesPageViewModel
            {
                Addresses = await _addressService.GetMineAsync(RequiredUserId)
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create([Bind(Prefix = "NewAddress")] AddressRequest newAddress, string? returnUrl)
        {
            // Show the form again with the errors next to the fields.
            if (!ModelState.IsValid)
            {
                return View(nameof(Index), new AddressesPageViewModel
                {
                    Addresses = await _addressService.GetMineAsync(RequiredUserId),
                    NewAddress = newAddress
                });
            }

            await TryAsync(() => _addressService.CreateAsync(RequiredUserId, newAddress), "Address saved.");
            return RedirectToLocal(returnUrl, nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> SetDefault(int id)
        {
            await TryAsync(() => _addressService.SetDefaultAsync(id, RequiredUserId), "Default address updated.");
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await TryAsync(() => _addressService.DeleteAsync(id, RequiredUserId), "Address removed.");
            return RedirectToAction(nameof(Index));
        }
    }
}
