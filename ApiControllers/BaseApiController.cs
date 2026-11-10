using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using CedarDelivery.Constants;

namespace CedarDelivery.ApiControllers
{
    // Every /api controller inherits from this (the API version of BaseController).
    // - [ApiController]: automatic 400 on invalid input (formatted as ApiError in Program.cs).
    // - ApiExceptionFilter: service exceptions become JSON errors.
    // - IgnoreAntiforgeryToken: the anti-forgery check (Program.cs) protects HTML forms;
    //   JSON clients don't have a form token. The login cookie is SameSite=Lax,
    //   so other sites can't send it with a cross-site POST anyway.
    [ApiController]
    [Produces("application/json")]
    [TypeFilter(typeof(ApiExceptionFilter))]
    [IgnoreAntiforgeryToken]
    public abstract class BaseApiController : ControllerBase
    {
        // The signed-in user's Id, or null for visitors.
        protected string? CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

        // Use only inside [Authorize] actions, where a user is always signed in.
        protected string RequiredUserId => CurrentUserId
            ?? throw new InvalidOperationException("RequiredUserId used outside an [Authorize] action.");

        protected bool IsAdmin => User.IsInRole(Roles.Admin);
    }
}
