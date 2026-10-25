#nullable disable

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CedarDelivery.Constants;
using CedarDelivery.Models;

namespace CedarDelivery.Areas.Identity.Pages.Account
{
    // "Continue with Google".
    // 1. OnPost sends the user to Google.
    // 2. Google sends them back to OnGetCallbackAsync.
    // 3. We find their account (or create a Customer account from their Google name and email)
    //    and sign them in. No extra form: Google already verified the email.
    [AllowAnonymous]
    public class ExternalLoginModel : PageModel
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<ExternalLoginModel> _logger;

        public ExternalLoginModel(SignInManager<User> signInManager, UserManager<User> userManager, ILogger<ExternalLoginModel> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        // Read by the Login page after we redirect there.
        [TempData]
        public string ErrorMessage { get; set; }

        // Nothing to show on this page itself.
        public IActionResult OnGet() => RedirectToPage("./Login");

        public IActionResult OnPost(string provider, string returnUrl = null)
        {
            var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }

        public async Task<IActionResult> OnGetCallbackAsync(string returnUrl = null, string remoteError = null)
        {
            returnUrl ??= Url.Content("~/");

            if (remoteError != null)
            {
                return BackToLogin($"Error from the external provider: {remoteError}", returnUrl);
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return BackToLogin("Could not load the external login information. Please try again.", returnUrl);
            }

            // Already linked to this Google account?
            var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);

            if (user == null)
            {
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                if (string.IsNullOrEmpty(email))
                {
                    return BackToLogin("Your Google account did not share an email address.", returnUrl);
                }

                // An account with the same email may exist (registered with a password).
                user = await _userManager.FindByEmailAsync(email);

                if (user == null)
                {
                    user = new User
                    {
                        UserName = email,
                        Email = email,
                        EmailConfirmed = true,
                        FirstName = info.Principal.FindFirstValue(ClaimTypes.GivenName) ?? email.Split('@')[0],
                        LastName = info.Principal.FindFirstValue(ClaimTypes.Surname) ?? "",
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    };

                    var createResult = await _userManager.CreateAsync(user);
                    if (!createResult.Succeeded)
                    {
                        return BackToLogin(string.Join(" ", createResult.Errors.Select(e => e.Description)), returnUrl);
                    }

                    await _userManager.AddToRoleAsync(user, Roles.Customer);
                    _logger.LogInformation("Created customer account for {Email} with {Provider}.", email, info.LoginProvider);
                }

                // Link Google to the account so next time FindByLoginAsync finds it.
                var linkResult = await _userManager.AddLoginAsync(user, info);
                if (!linkResult.Succeeded)
                {
                    return BackToLogin(string.Join(" ", linkResult.Errors.Select(e => e.Description)), returnUrl);
                }
            }

            if (!user.IsActive)
            {
                return BackToLogin("This account has been deactivated. Please contact support.", returnUrl);
            }

            if (await _userManager.IsLockedOutAsync(user))
            {
                return BackToLogin("Too many failed attempts. Try again in 5 minutes.", returnUrl);
            }

            user.LastLoginAt = DateTime.Now;
            await _userManager.UpdateAsync(user);

            await _signInManager.SignInAsync(user, isPersistent: false, info.LoginProvider);
            _logger.LogInformation("{Email} logged in with {Provider}.", user.Email, info.LoginProvider);
            return LocalRedirect(returnUrl);
        }

        private IActionResult BackToLogin(string message, string returnUrl)
        {
            ErrorMessage = message;
            return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
        }
    }
}
