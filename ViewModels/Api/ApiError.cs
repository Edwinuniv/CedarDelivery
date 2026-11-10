namespace CedarDelivery.ViewModels.Api
{
    // The one error format every /api endpoint returns:
    // { "success": false, "message": "...", "errors": [ ... ] }
    public class ApiError
    {
        public bool Success { get; } = false;
        public string Message { get; set; }
        public List<string> Errors { get; set; }

        public ApiError(string message, List<string>? errors = null)
        {
            Message = message;
            Errors = errors ?? new List<string>();
        }
    }
}
