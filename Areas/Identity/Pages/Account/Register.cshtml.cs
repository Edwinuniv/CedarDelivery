// Based on the ASP.NET Core Identity scaffolded page, adapted like CedarPay's.
#nullable disable

using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using CedarDelivery.Constants;
using CedarDelivery.Models;

namespace CedarDelivery.Areas.Identity.Pages.Account
{
    // Creates a new Customer account. Merchants and Drivers start as Customers
    // and apply for the role later (RoleApplication).
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<RegisterModel> _logger;

        public RegisterModel(UserManager<User> userManager, SignInManager<User> signInManager, IEmailSender emailSender, ILogger<RegisterModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required, StringLength(50)]
            [Display(Name = "First name")]
            public string FirstName { get; set; }

            [Required, StringLength(50)]
            [Display(Name = "Last name")]
            public string LastName { get; set; }

            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [StringLength(100, MinimumLength = 6, ErrorMessage = "The password must be at least 6 characters.")]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The passwords do not match.")]
            public string ConfirmPassword { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // The email is also the username, so users log in with their email.
            var user = new User
            {
                UserName = Input.Email,
                Email = Input.Email,
                FirstName = Input.FirstName.Trim(),
                LastName = Input.LastName.Trim(),
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            // CreateAsync hashes the password and checks the rules from Program.cs
            // (length, digit, upper/lower case, unique email).
            var result = await _userManager.CreateAsync(user, Input.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, Roles.Customer);
                _logger.LogInformation("New customer account created for {Email}.", user.Email);

                // With email set up (Program.cs), the user must click a link before logging in.
                if (_userManager.Options.SignIn.RequireConfirmedEmail)
                {
                    await SendConfirmationEmailAsync(user, returnUrl);
                    return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl });
                }

                // Without email (development), sign the user in directly.
                await _signInManager.SignInAsync(user, isPersistent: false);
                return LocalRedirect(returnUrl);
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return Page();
        }

        // Same link format as the scaffolded Identity page, so the built-in
        // /Identity/Account/ConfirmEmail page can check the token.
        private async Task SendConfirmationEmailAsync(User user, string returnUrl)
        {
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { area = "Identity", userId = user.Id, code, returnUrl },
                protocol: Request.Scheme);

            await _emailSender.SendEmailAsync(user.Email, "Confirm your CedarDelivery account",
                $"<h2 style='color:#0b6b4f;margin-top:0'>Welcome to CedarDelivery, {HtmlEncoder.Default.Encode(user.FirstName)}!</h2>" +
                $"<p>Please confirm your email to start ordering.</p>" +
                $"<p style='margin-top:24px'><a href='{HtmlEncoder.Default.Encode(callbackUrl)}' " +
                $"style='background:#12a87a;color:#fff;padding:12px 24px;border-radius:8px;text-decoration:none;font-weight:600'>Confirm my email</a></p>");
        }
    }
}
