using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using CedarDelivery.Services;

namespace CedarDelivery.Controllers
{
    // The web-page version of ApiExceptionFilter. When a service throws inside an MVC action:
    // - NotFound  -> the "not found" page (404)
    // - Forbidden -> the Access Denied page
    // - BadRequest (or anything else from AppException) -> red message, back to the previous page
    // Other exceptions are left alone so the normal error page (/Home/Error) handles them.
    public class AppExceptionPageFilter : IExceptionFilter
    {
        private readonly ITempDataDictionaryFactory _tempDataFactory;

        public AppExceptionPageFilter(ITempDataDictionaryFactory tempDataFactory)
        {
            _tempDataFactory = tempDataFactory;
        }

        public void OnException(ExceptionContext context)
        {
            if (context.Exception is not AppException appException)
            {
                return;
            }

            if (appException is NotFoundException)
            {
                context.Result = new ViewResult { ViewName = "NotFound", StatusCode = StatusCodes.Status404NotFound };
            }
            else if (appException is ForbiddenException)
            {
                context.Result = new RedirectResult("/Identity/Account/AccessDenied");
            }
            else
            {
                var tempData = _tempDataFactory.GetTempData(context.HttpContext);
                tempData["Error"] = appException.Message;

                // Go back where the user came from, but only within this site.
                var referer = context.HttpContext.Request.Headers.Referer.ToString();
                var backUrl = Uri.TryCreate(referer, UriKind.Absolute, out var uri)
                              && uri.Host == context.HttpContext.Request.Host.Host
                    ? uri.PathAndQuery
                    : "/";
                context.Result = new RedirectResult(backUrl);
            }

            context.ExceptionHandled = true;
        }
    }
}
