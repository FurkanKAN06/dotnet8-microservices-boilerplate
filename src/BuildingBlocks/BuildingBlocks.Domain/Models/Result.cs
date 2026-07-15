namespace BuildingBlocks.Domain.Models
{
    public class Error
    {
        public string Code { get; }
        public string Message { get; }

        public Error(string code, string message)
        {
            Code = code;
            Message = message;
        }

        public static Error None => new Error(string.Empty, string.Empty);
    }

    public class Result
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public Error Error { get; }

        protected Result(bool isSuccess, Error error)
        {
            IsSuccess = isSuccess;
            Error = error;
        }

        public static Result Success() => new Result(true, Error.None);
        public static Result Failure(Error error) => new Result(false, error);
    }

    public class Result<T> : Result
    {
        private readonly T? _value;
        public T Value => IsSuccess ? _value! : throw new System.InvalidOperationException("The value of a failure result can not be accessed.");

        protected internal Result(T? value, bool isSuccess, Error error) : base(isSuccess, error)
        {
            _value = value;
        }

        public static Result<T> Success(T value) => new Result<T>(value, true, Error.None);
        public static new Result<T> Failure(Error error) => new Result<T>(default, false, error);
    }
}
