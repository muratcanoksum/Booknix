namespace Booknix.Application.DTOs
{
    public class RequestResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;

        public RequestResult() { }

        public RequestResult(bool success, string message)
        {
            Success = success;
            Message = message;
        }
    }
}
