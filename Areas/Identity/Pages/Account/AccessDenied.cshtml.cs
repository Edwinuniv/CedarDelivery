using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CedarDelivery.Areas.Identity.Pages.Account
{
    // Shown when a signed-in user opens a page their role can't access (403).
    public class AccessDeniedModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
