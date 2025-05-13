namespace Booknix.Application.Results
{
    public class Result
    {
        public bool IsSuccess { get; private set; }
        public string Message { get; private set; } = string.Empty;
        public bool IsFailure => !IsSuccess;

        protected Result(bool isSuccess, string message)
        {
            IsSuccess = isSuccess;
            Message = message ?? string.Empty;
        }

        public static Result Success(string message = "")
        {
            return new Result(true, message);
        }

        public static Result Failure(string message)
        {
            return new Result(false, message);
        }
    }

    public class Result<T> : Result
    {
        private readonly T? _value;
        public T Value => IsSuccess ? _value! : throw new InvalidOperationException("Cannot access Value for a failed result.");

        protected Result(bool isSuccess, string message, T? value = default) : base(isSuccess, message)
        {
            _value = value;
        }

        public static Result<T> Success(T value, string message = "")
        {
            return new Result<T>(true, message, value);
        }

        public static new Result<T> Failure(string message)
        {
            return new Result<T>(false, message);
        }
    }
} 