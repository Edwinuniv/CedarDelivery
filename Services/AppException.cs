namespace CedarDelivery.Services
{
    // Errors that services throw on purpose. ApiExceptionFilter turns them into
    // a JSON ApiError with the matching HTTP status, so controllers stay short.
    public class AppException : Exception
    {
        public int StatusCode { get; }

        public AppException(string message, int statusCode) : base(message)
        {
            StatusCode = statusCode;
        }
    }

    // 400: the request breaks a business rule (e.g. category belongs to another store).
    public class BadRequestException : AppException
    {
        public BadRequestException(string message) : base(message, StatusCodes.Status400BadRequest) { }
    }

    // 403: logged in, but not allowed to touch this item (e.g. someone else's store).
    public class ForbiddenException : AppException
    {
        public ForbiddenException(string message = "You don't have permission to do this.")
            : base(message, StatusCodes.Status403Forbidden) { }
    }

    // 404: the item doesn't exist (or the user isn't allowed to know it exists).
    public class NotFoundException : AppException
    {
        public NotFoundException(string message) : base(message, StatusCodes.Status404NotFound) { }
    }
}
