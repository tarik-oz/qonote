namespace Qonote.Core.Application.Exceptions;

public class ConflictException : BaseException
{
    public IDictionary<string, string[]>? Errors { get; }

    public ConflictException(string message) : base(message) { }

    public ConflictException(string message, IDictionary<string, string[]> errors) : base(message)
    {
        Errors = errors;
    }
}
