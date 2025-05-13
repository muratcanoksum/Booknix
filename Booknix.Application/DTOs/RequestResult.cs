namespace Booknix.Application.DTOs
{
    public class RequestResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        
        // Add IsSuccess property for compatibility with Result class
        public bool IsSuccess => Success;
        public bool IsFailure => !Success;

        public RequestResult() { }

        public RequestResult(bool success, string message)
        {
            Success = success;
            Message = message;
        }
    }
}
