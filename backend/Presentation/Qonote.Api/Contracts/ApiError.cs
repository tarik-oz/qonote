namespace Qonote.Presentation.Api.Contracts;

public class ApiError
{
    public string Message { get; }

    public IDictionary<string, string[]>? Errors { get; }

    public string? ErrorCode { get; }

    public ApiError(string message, IDictionary<string, string[]>? errors = null, string? errorCode = null)
    {
        Message = message;
        Errors = errors;
        ErrorCode = errorCode;
    }
}
