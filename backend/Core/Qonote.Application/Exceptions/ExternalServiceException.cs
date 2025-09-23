namespace Qonote.Core.Application.Exceptions;

public class ExternalServiceException : BaseException
{
    public string Provider { get; }
    public int? StatusCode { get; }
    public IDictionary<string, string[]>? Errors { get; }

    public ExternalServiceException(string provider, string message, int? statusCode = null, IDictionary<string, string[]>? errors = null)
        : base(message)
    {
        Provider = provider;
        StatusCode = statusCode;
        Errors = errors;
    }
}
