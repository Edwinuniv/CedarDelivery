using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using CedarDelivery.Services;
using CedarDelivery.ViewModels.Api;

namespace CedarDelivery.ApiControllers
{
    // Catches exceptions thrown inside API actions and turns them into the ApiError JSON format.
    // - AppException (NotFound, Forbidden, BadRequest): its own status and message.
    // - Anything else: 500 with a generic message; the details go to the log, not to the client.
    public class ApiExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<ApiExceptionFilter> _logger;

        public ApiExceptionFilter(ILogger<ApiExceptionFilter> logger)
        {
            _logger = logger;
        }

        public void OnException(ExceptionContext context)
        {
            if (context.Exception is AppException appException)
            {
                context.Result = new ObjectResult(new ApiError(appException.Message))
                {
                    StatusCode = appException.StatusCode
                };
            }
            else
            {
                _logger.LogError(context.Exception, "Unhandled error in {Path}.", context.HttpContext.Request.Path);
                context.Result = new ObjectResult(new ApiError("Something went wrong. Please try again."))
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }

            context.ExceptionHandled = true;
        }
    }
}
